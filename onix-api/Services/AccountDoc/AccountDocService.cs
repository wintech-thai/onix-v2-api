using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Services
{
    public class AccountDocService : BaseService, IAccountDocService
    {
        private readonly IAccountDocRepository repository = null!;
        private readonly IItemService _itemService;
        private readonly IPointRuleService _pointRuleService;
        private readonly IDocumentNumberService _documentNumberService;
        private readonly IEntityRepository _entityRepository;
        private readonly IRedisHelper _redis;
        private readonly IMerchantService _merchantService;
        private readonly IPointService _pointService;

        public AccountDocService(
            IAccountDocRepository repo,
            IItemService itemService,
            IPointRuleService pointRuleService,
            IDocumentNumberService documentNumberService,
            IEntityRepository entityRepository,
            IRedisHelper redis,
            IMerchantService merchantService,
            IPointService pointService) : base()
        {
            repository = repo;
            _itemService = itemService;
            _pointRuleService = pointRuleService;
            _documentNumberService = documentNumberService;
            _entityRepository = entityRepository;
            _redis = redis;
            _merchantService = merchantService;
            _pointService = pointService;
        }

        public async Task<MVAccountDoc> AddAccountDoc(string orgId, MAccountDoc ad)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVAccountDoc()
            {
                Status = "OK",
                Description = "Success",
            };

            if (string.IsNullOrEmpty(ad.DocumentType))
            {
                r.Status = "DOCTYPE_MISSING";
                r.Description = $"Document type is missing!!!";

                return r;
            }

            // PurchaseCash ไม่มีลูกค้าจริง (เป็นคำสั่งซื้อฝั่ง merchant เอง เช่น เติม point) — ข้าม customer lookup + document number service
            if (ad.DocumentType == "PurchaseCash")
            {
                ad.EntityId = "Internal";
                ad.DocumentNo = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                ad.DocumentDate ??= DateTime.UtcNow;

                var purchaseResult = await repository!.AddAccountDoc(ad);
                r.AccountDoc = purchaseResult;

                return r;
            }

            if (string.IsNullOrEmpty(ad.EntityId) || !ServiceUtils.IsGuidValid(ad.EntityId))
            {
                r.Status = "CUSTOMER_MISSING";
                r.Description = $"Customer is missing or invalid!!!";

                return r;
            }

            _entityRepository.SetCustomOrgId(orgId);
            var customer = _entityRepository.GetEntityById(ad.EntityId);
            if (customer == null)
            {
                r.Status = "CUSTOMER_NOTFOUND";
                r.Description = $"Customer ID [{ad.EntityId}] not found for the organization [{orgId}]";

                return r;
            }

            try
            {
                ad.DocumentNo = await _documentNumberService.GetDocumentNumber(orgId, ad.DocumentType);
            }
            catch (KeyNotFoundException)
            {
                r.Status = "DOCTYPE_NOT_CONFIGURED";
                r.Description = $"Document number is not configured for type [{ad.DocumentType}]. Please add it in Document Number settings first.";

                return r;
            }
            catch (InvalidOperationException)
            {
                r.Status = "DOCNO_LOCK_FAILED";
                r.Description = "Unable to generate document number right now, please try again.";

                return r;
            }

            ad.DocumentDate ??= DateTime.UtcNow;

            //ขายเชื่อ: คำนวณวันครบกำหนดชำระจาก credit term ของลูกค้า (ถ้าลูกค้าไม่มีกำหนดไว้ ปล่อยว่าง ให้ไปกำหนดเองที่เอกสาร)
            if (ad.DocumentType == "CreditInvoice" && ad.DueDate == null && customer.CreditTermDay.HasValue)
            {
                ad.DueDate = ad.DocumentDate!.Value.AddDays(customer.CreditTermDay.Value);
            }

            var result = await repository!.AddAccountDoc(ad);
            r.AccountDoc = result;

            return r;
        }

        public async Task<MVAccountDoc> UpdateAccountDocById(string orgId, string accountDocId, MAccountDoc ad)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVAccountDoc()
            {
                Status = "OK",
                Description = "Success",
            };

            var failure = await ValidatePendingOrNull(orgId, accountDocId, " and can no longer be edited");
            if (failure != null) return failure;

            var result = await repository!.UpdateAccountDocById(accountDocId, ad);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Account document ID [{accountDocId}] not found for the organization [{orgId}]";

                return r;
            }

            r.AccountDoc = result;
            return r;
        }

        public async Task<MVAccountDoc> ApproveAccountDocById(string orgId, string accountDocId)
        {
            repository!.SetCustomOrgId(orgId);

            var failure = await ValidatePendingOrNull(orgId, accountDocId, "");
            if (failure != null) return failure;

            var result = await repository!.ApproveAccountDocById(accountDocId);
            if (result != null)
            {
                await ProcessApprovedDocumentSideEffects(result);
            }

            return new MVAccountDoc() { Status = "OK", Description = "Success", AccountDoc = result };
        }

        // Admin เห็นข้าม org ทุก merchant — ไม่เรียก SetCustomOrgId เลย ปล่อยให้ repository ใช้ค่า RESERVE_ORG_ID default (ไม่กรองตาม org)
        public async Task<MVAccountDoc> ApproveAccountDocByIdAdmin(string accountDocId)
        {
            var failure = await ValidatePendingOrNull("global", accountDocId, "");
            if (failure != null) return failure;

            var result = await repository!.ApproveAccountDocById(accountDocId);
            if (result != null)
            {
                await ProcessApprovedDocumentSideEffects(result);
            }

            return new MVAccountDoc() { Status = "OK", Description = "Success", AccountDoc = result };
        }

        // ข้อ 5c : approve "PointTopUp" => เติม point wallet ของ merchant นั้น — แยกเป็น method เผื่อในอนาคตจะ reuse จาก flow auto-approve ตอนจ่ายเงินจริง (ข้อ 5d)
        private async Task ProcessApprovedDocumentSideEffects(MAccountDoc doc)
        {
            if (doc.DocumentType != "PurchaseCash" || doc.DocumentSubType != "PointTopUp") return;

            var merchants = await _merchantService.GetMerchants(doc.OrgId!, new VMMerchant { Limit = 100 });
            var merchant = merchants.FirstOrDefault(m => m.OrgId == doc.OrgId);
            if (merchant?.Id == null) return;

            var walletResult = await _pointService.GetWalletByRefIdAndType(doc.OrgId!, merchant.Id.ToString()!, "MerchantPoint");
            if (walletResult?.Wallet?.Id == null) return;

            var amount = doc.GrandTotal ?? doc.NetAmount ?? doc.TotalPrice ?? 0;
            await _pointService.AddPoint(doc.OrgId!, new MPointTx
            {
                WalletId = walletResult.Wallet.Id.ToString(),
                TxAmount = (long)amount,
                TxAmountDecimal = (decimal)amount,
                Tags = "PointTopUp",
                Tags2 = doc.DocumentNo,
                Description = $"Point top-up from PurchaseCash [{doc.DocumentNo}]",
            });
        }

        public async Task<MVAccountDoc> UpdateAccountDocParamsById(string orgId, string accountDocId, string paramsJson)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.UpdateAccountDocParamsById(accountDocId, paramsJson);
            return new MVAccountDoc() { Status = result != null ? "OK" : "NOTFOUND", Description = result != null ? "Success" : $"Account document ID [{accountDocId}] not found for the organization [{orgId}]", AccountDoc = result };
        }

        public async Task<MVAccountDoc> RejectAccountDocById(string orgId, string accountDocId, string reason)
        {
            repository!.SetCustomOrgId(orgId);

            var failure = await ValidatePendingOrNull(orgId, accountDocId, "");
            if (failure != null) return failure;

            var result = await repository!.RejectAccountDocById(accountDocId, reason);
            return new MVAccountDoc() { Status = "OK", Description = "Success", AccountDoc = result };
        }

        // Admin เห็นข้าม org ทุก merchant — ไม่เรียก SetCustomOrgId เลย (เหมือน ApproveAccountDocByIdAdmin)
        public async Task<MVAccountDoc> RejectAccountDocByIdAdmin(string accountDocId, string reason)
        {
            var failure = await ValidatePendingOrNull("global", accountDocId, "");
            if (failure != null) return failure;

            var result = await repository!.RejectAccountDocById(accountDocId, reason);
            return new MVAccountDoc() { Status = "OK", Description = "Success", AccountDoc = result };
        }

        public async Task<MVAccountDoc> CancelAccountDocById(string orgId, string accountDocId)
        {
            repository!.SetCustomOrgId(orgId);

            var failure = await ValidatePendingOrNull(orgId, accountDocId, "");
            if (failure != null) return failure;

            var result = await repository!.CancelAccountDocById(accountDocId);
            return new MVAccountDoc() { Status = "OK", Description = "Success", AccountDoc = result };
        }

        // Shared by Update/Approve/Cancel — all three require the doc to still be Pending.
        private async Task<MVAccountDoc?> ValidatePendingOrNull(string orgId, string accountDocId, string statusMessageSuffix)
        {
            if (!ServiceUtils.IsGuidValid(accountDocId))
            {
                return new MVAccountDoc()
                {
                    Status = "UUID_INVALID",
                    Description = $"Account document ID [{accountDocId}] format is invalid",
                };
            }

            var existing = await repository.GetAccountDocById(accountDocId);
            if (existing == null)
            {
                return new MVAccountDoc()
                {
                    Status = "NOTFOUND",
                    Description = $"Account document ID [{accountDocId}] not found for the organization [{orgId}]",
                };
            }

            if (!await repository.IsAccountDocPending(accountDocId))
            {
                return new MVAccountDoc()
                {
                    Status = "INVALID_STATUS",
                    Description = $"Account document [{existing.DocumentNo}] is already [{existing.Status}]{statusMessageSuffix}",
                    AccountDoc = existing,
                };
            }

            return null;
        }

        public async Task<MVAccountDoc> DeleteAccountDocById(string orgId, string accountDocId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVAccountDoc()
            {
                Status = "SUCCESS",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(accountDocId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Account document ID [{accountDocId}] format is invalid";

                return r;
            }

            var acd = await repository.GetAccountDocById(accountDocId);
            if (acd == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Account document ID [{accountDocId}] not found for the organization [{orgId}]";

                return r;
            }

            if (acd.Status == "Approved")
            {
                r.Status = "ALREADY_APPROVED";
                r.Description = $"Account document ID [{accountDocId}] is already approved and cannot be deleted";

                return r;
            }

            var result = await repository!.DeleteAccountDocById(accountDocId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Account document ID [{accountDocId}] not found for the organization [{orgId}]";

                return r;
            }

            r.AccountDoc = result;
            return r;
        }

        public async Task<List<MAccountDoc>> GetAccountDocs(string orgId, VMAccountDoc param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetAccountDocs(param);

            return result;
        }

        public async Task<int> GetAccountDocCount(string orgId, VMAccountDoc param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetAccountDocCount(param);

            return result;
        }

        // Admin เห็นข้าม org ทุก merchant — ไม่เรียก SetCustomOrgId เลย, กรองเฉพาะ org เดียวได้ผ่าน param.OrgIdFilter
        public async Task<List<MAccountDoc>> GetAccountDocsAdmin(VMAccountDoc param)
        {
            return await repository!.GetAccountDocs(param);
        }

        public async Task<int> GetAccountDocCountAdmin(VMAccountDoc param)
        {
            return await repository!.GetAccountDocCount(param);
        }

        public async Task<MVAccountDoc> GetAccountDocByIdAdmin(string accountDocId)
        {
            var r = new MVAccountDoc() { Status = "OK", Description = "Success" };

            if (!ServiceUtils.IsGuidValid(accountDocId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Account document ID [{accountDocId}] format is invalid";

                return r;
            }

            var result = await repository!.GetAccountDocById(accountDocId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Account document ID [{accountDocId}] not found";
            }

            r.AccountDoc = result;

            return r;
        }

        public async Task<MVAccountDoc> GetAccountDocById(string orgId, string accountDocIdId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVAccountDoc()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(accountDocIdId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Account document ID [{accountDocIdId}] format is invalid";

                return r;
            }

            var result = await repository!.GetAccountDocById(accountDocIdId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Account document ID [{accountDocIdId}] not found for the organization [{orgId}]";
            }

            r.AccountDoc = result;

            return r;
        }

        public async Task<List<MAccountDocItem>> GetAccountDocItemsById(string orgId, string accountDocId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetAccountDocItemsById(accountDocId);

            return result;
        }

        public async Task<MVAccountDocItem> AddAccountDocItem(string orgId, string accountDocId, MAccountDocItem adi)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVAccountDocItem()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(accountDocId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Account document ID [{accountDocId}] format is invalid";

                return r;
            }

            //Check if status is approved, cannot add item
            var isApproved = await repository.IsAccountDocApproved(accountDocId);
            if (isApproved)
            {
                r.Status = "ALREADY_APPROVED";
                r.Description = $"Account document ID [{accountDocId}] is already approved and cannot add item";

                return r;
            }

            var result = await repository!.AddAccountDocItem(accountDocId, adi);
            r.AccountDocItem = result;
            return r;
        }

        public async Task<MVAccountDocItem> UpdateAccountDocItemById(string orgId, string accountDocItemId, MAccountDocItem adi)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVAccountDocItem()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(accountDocItemId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Account document item ID [{accountDocItemId}] format is invalid";

                return r;
            }

            //Check if status is approved, cannot add item
            var isApproved = await repository.IsAccountDocItemApproved(accountDocItemId);
            if (isApproved)
            {
                r.Status = "ALREADY_APPROVED";
                r.Description = $"Account document item ID [{accountDocItemId}] is already approved and cannot be changed";

                return r;
            }

            var result = await repository!.UpdateAccountDocItemById(accountDocItemId, adi);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Account document item ID [{accountDocItemId}] not found for the organization [{orgId}]";

                return r;
            }

            r.AccountDocItem = result;
            return r;
        }

        public async Task<MVAccountDocItem> DeleteAccountDocItemById(string orgId, string accountDocItemId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVAccountDocItem()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(accountDocItemId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Account document item ID [{accountDocItemId}] format is invalid";

                return r;
            }

            var isApproved = await repository.IsAccountDocItemApproved(accountDocItemId);
            if (isApproved)
            {
                r.Status = "ALREADY_APPROVED";
                r.Description = $"Account document item ID [{accountDocItemId}] is already approved and cannot be changed";

                return r;
            }

            var result = await repository!.DeleteAccountDocItemById(accountDocItemId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Account document item ID [{accountDocItemId}] not found for the organization [{orgId}]";

                return r;
            }

            r.AccountDocItem = result;
            return r;
        }

        public Task<MVAccountDocItem> CalculateAccountDocItemPrice(string orgId, MAccountDocItem adi)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVAccountDocItem()
            {
                Status = "OK",
                Description = "Success"
            };

            //TODO : Implement here 

            var unitPrice = 15.00;
            var itemPrice = new MAccountDocItem()
            {
                UnitPrice = unitPrice,
                TotalPrice = (adi.Quantity != null) ? (adi.Quantity! * unitPrice) : 0,
                IncentiveRate = 0.00,
                IncentiveTotalPrice = 0.00
            };

            r.AccountDocItem = itemPrice;

            return Task.FromResult(r);
        }
    }
}
