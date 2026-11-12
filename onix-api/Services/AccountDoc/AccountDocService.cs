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
        private readonly IInventoryDocService _inventoryDocService;

        public AccountDocService(
            IAccountDocRepository repo,
            IItemService itemService,
            IPointRuleService pointRuleService,
            IDocumentNumberService documentNumberService,
            IEntityRepository entityRepository,
            IRedisHelper redis,
            IMerchantService merchantService,
            IPointService pointService,
            IInventoryDocService inventoryDocService) : base()
        {
            repository = repo;
            _itemService = itemService;
            _pointRuleService = pointRuleService;
            _documentNumberService = documentNumberService;
            _entityRepository = entityRepository;
            _redis = redis;
            _merchantService = merchantService;
            _pointService = pointService;
            _inventoryDocService = inventoryDocService;
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

            return await ApproveAccountDocCore(accountDocId);
        }

        // Admin เห็นข้าม org ทุก merchant — ไม่เรียก SetCustomOrgId เลย ปล่อยให้ repository ใช้ค่า RESERVE_ORG_ID default (ไม่กรองตาม org)
        public async Task<MVAccountDoc> ApproveAccountDocByIdAdmin(string accountDocId)
        {
            var failure = await ValidatePendingOrNull("global", accountDocId, "");
            if (failure != null) return failure;

            return await ApproveAccountDocCore(accountDocId);
        }

        // ข้อ 4b : ก่อน approve จริงต้องเช็คว่าถ้าตัด stock รายการ InventoryItem ทุกตัวแล้วจะติดลบไหม
        // ถ้าติดลบ ต้อง "approve ไม่ได้" เลย (ห้าม flip status ของ AccountDoc ไปแล้วค่อยพบว่าตัด stock ไม่ได้)
        private async Task<MVAccountDoc> ApproveAccountDocCore(string accountDocId)
        {
            var doc = await repository!.GetAccountDocById(accountDocId);

            var stockFailure = await ValidateSufficientStockForSaleOrNull(doc);
            if (stockFailure != null) return stockFailure;

            var result = await repository!.ApproveAccountDocById(accountDocId);
            if (result != null)
            {
                result.Items = doc?.Items;
                await ProcessApprovedDocumentSideEffects(result);
            }

            return new MVAccountDoc() { Status = "OK", Description = "Success", AccountDoc = result };
        }

        private async Task<MVAccountDoc?> ValidateSufficientStockForSaleOrNull(MAccountDoc? doc)
        {
            var items = (doc?.Items ?? new List<MAccountDocItem>())
                .Where(i => i.ProductType == "InventoryItem" && !string.IsNullOrEmpty(i.ProductId) && !string.IsNullOrEmpty(i.LocationId))
                .ToList();
            if (items.Count == 0 || string.IsNullOrEmpty(doc?.OrgId)) return null;

            foreach (var item in items)
            {
                var requested = (decimal)(item.Quantity ?? 0);
                if (requested <= 0) continue;

                var wallet = await _pointService.GetInventoryWalletByLocationItemId(doc.OrgId!, item.LocationId!, item.ProductId!);
                var available = wallet?.PointQuantityBalanceDecimal ?? 0;
                if (requested > available)
                {
                    return new MVAccountDoc
                    {
                        Status = "INSUFFICIENT_STOCK",
                        Description = $"Item [{item.ProductCode}] insufficient stock at location [{item.LocationName}]: available [{available}], requested [{requested}]",
                    };
                }
            }

            return null;
        }

        // ข้อ 5c : approve "PointTopUp" => เติม point wallet ของ merchant นั้น — แยกเป็น method เผื่อในอนาคตจะ reuse จาก flow auto-approve ตอนจ่ายเงินจริง (ข้อ 5d)
        // ข้อ 4 (x014) : approve ขายเชื่อ/ขายสด => ตัด stock รายการ InventoryItem + (เฉพาะขายเชื่อ) บันทึกยอดลูกหนี้
        private async Task ProcessApprovedDocumentSideEffects(MAccountDoc doc)
        {
            if (doc.DocumentType == "PurchaseCash" && doc.DocumentSubType == "PointTopUp")
            {
                await ProcessPointTopUpSideEffect(doc);
                return;
            }

            if (doc.DocumentType == "CashInvoice" || doc.DocumentType == "CreditInvoice")
            {
                await ProcessSaleStockDeductionAndReceivable(doc);
            }
        }

        private async Task ProcessPointTopUpSideEffect(MAccountDoc doc)
        {
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
                Tags = $"PointTopUp,AccountDocId=[{doc.Id}]",
                Tags2 = doc.DocumentNo,
                Description = $"Point top-up from PurchaseCash [{doc.DocumentNo}]",
            });
        }

        // ข้อ 4a/4b : เอาเฉพาะรายการ ProductType=InventoryItem มาสร้างเป็น InventoryDoc (StockOut) แยกตาม location ที่เลือก
        // แล้ว approve ให้เลย (reuse ValidateSufficientStockOrNull + DeductPointV2 ของ InventoryDocService โดยตรง ไม่ต้องเขียนซ้ำ)
        private async Task ProcessSaleStockDeductionAndReceivable(MAccountDoc doc)
        {
            var orgId = doc.OrgId!;
            var itemLines = (doc.Items ?? new List<MAccountDocItem>())
                .Where(i => i.ProductType == "InventoryItem" && !string.IsNullOrEmpty(i.ProductId) && !string.IsNullOrEmpty(i.LocationId))
                .ToList();

            if (itemLines.Count > 0)
            {
                var saleTypeLabel = doc.DocumentType == "CashInvoice" ? "ขายสด" : "ขายเชื่อ";
                foreach (var group in itemLines.GroupBy(i => i.LocationId))
                {
                    var first = group.First();
                    var inventoryDoc = new MInventoryDoc
                    {
                        FromLocationId = group.Key,
                        FromLocationCode = first.LocationCode,
                        FromLocationName = first.LocationName,
                        Description = $"ตัดสต็อกจาก{saleTypeLabel} เลขที่ {doc.DocumentNo}",
                        Items = group.Select(i => new MInventoryDocItem
                        {
                            ItemId = i.ProductId,
                            ItemCode = i.ProductCode,
                            ItemName = i.ProductDesc,
                            ItemQuantity = (decimal?)i.Quantity,
                        }).ToList(),
                    };

                    var addResult = await _inventoryDocService.AddInventoryDocStockOut(orgId, inventoryDoc);
                    if (addResult.Status != "OK" || addResult.InventoryDoc?.Id == null) continue;

                    var approveResult = await _inventoryDocService.ApproveInventoryDocStockOut(orgId, addResult.InventoryDoc.Id.ToString()!);

                    // ข้อ 4b.iv : เก็บมูลค่า item ณ ตอนตัด stock ไปขาย ไว้ที่ AccountDocItem (ไว้คิดต้นทุนสินค้าในอนาคต)
                    var approvedItems = approveResult.InventoryDoc?.Items ?? new List<MInventoryDocItem>();
                    var groupList = group.ToList();
                    for (int i = 0; i < groupList.Count && i < approvedItems.Count; i++)
                    {
                        if (groupList[i].Id.HasValue && approvedItems[i].ItemAmount.HasValue)
                        {
                            await repository!.UpdateAccountDocItemCostAmountById(
                                groupList[i].Id!.Value,
                                (double)approvedItems[i].ItemAmount!.Value,
                                (double)(approvedItems[i].ItemUnitPrice ?? 0));
                        }
                    }
                }
            }

            if (doc.DocumentType == "CreditInvoice")
            {
                await AddAccountReceivable(doc);
            }
        }

        // ข้อ 4c : ขายเชื่อเท่านั้นที่ต้อง track ยอดลูกหนี้ — เก็บไว้ที่ wallet แยกต่างหาก (RefId=CustomerId, WalletType=AccountReceivable)
        // สร้าง wallet ให้เลยถ้ายังไม่มี (ลูกค้าเก่าที่ยังไม่เคยขายเชื่อมาก่อน)
        private async Task AddAccountReceivable(MAccountDoc doc)
        {
            if (string.IsNullOrEmpty(doc.EntityId)) return;
            var orgId = doc.OrgId!;

            var walletResult = await _pointService.GetWalletByRefIdAndType(orgId, doc.EntityId, "AccountReceivable");
            var wallet = walletResult?.Wallet;
            if (wallet == null)
            {
                var createResult = await _pointService.AddWallet(orgId, new MWallet
                {
                    RefId = doc.EntityId,
                    WalletType = "AccountReceivable",
                    Name = $"AR-{doc.EntityCode ?? doc.EntityId}",
                    PointBalance = 0,
                    PointBalanceDecimal = 0,
                });
                wallet = createResult.Wallet;
            }
            if (wallet?.Id == null) return;

            var amount = doc.GrandTotal ?? doc.NetAmount ?? doc.TotalPrice ?? 0;
            await _pointService.AddPoint(orgId, new MPointTx
            {
                WalletId = wallet.Id.ToString(),
                TxAmount = (long)amount,
                TxAmountDecimal = (decimal)amount,
                Tags = doc.Id.ToString(), // ข้อ 5b : ใช้ลิงก์กลับไปที่เอกสารขายเชื่อต้นทางในหน้า Balance Transaction
                Tags2 = doc.DocumentNo,
                Description = "CreditInvoice",
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

        // Lets a re-approved PayIn request's webhook re-approve the linked AccountDoc too, instead of hitting INVALID_STATUS
        public async Task<MVAccountDoc> ResetAccountDocToPendingByIdAdmin(string accountDocId)
        {
            if (!ServiceUtils.IsGuidValid(accountDocId))
            {
                return new MVAccountDoc() { Status = "UUID_INVALID", Description = $"Account document ID [{accountDocId}] format is invalid" };
            }

            var existing = await repository!.GetAccountDocById(accountDocId);
            if (existing == null)
            {
                return new MVAccountDoc() { Status = "NOTFOUND", Description = $"Account document ID [{accountDocId}] not found" };
            }

            if (existing.Status != "Rejected")
            {
                return new MVAccountDoc() { Status = "INVALID_STATUS", Description = $"Only a Rejected account document can be reset to Pending, current status is [{existing.Status}]" };
            }

            var result = await repository!.ResetAccountDocToPendingById(accountDocId);
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
