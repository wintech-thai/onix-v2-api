using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Utils;
using System.Text.Json;

namespace Its.Onix.Api.Services
{
    public class AccountDocService : BaseService, IAccountDocService
    {
        private readonly IAccountDocRepository repository = null!;
        private readonly IItemService _itemService;
        private readonly IPointRuleService _pointRuleService;
        private readonly IRedisHelper _redis;

        public AccountDocService(
            IAccountDocRepository repo,
            IItemService itemService,
            IPointRuleService pointRuleService,
            IRedisHelper redis) : base()
        {
            repository = repo;
            _itemService = itemService;
            _pointRuleService = pointRuleService;
            _redis = redis;
        }

        public async Task<MVAccountDoc> AddAccountDoc(string orgId, MAccountDoc ad)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVAccountDoc()
            {
                Status = "OK",
                Description = "Success",
            };

            if (string.IsNullOrEmpty(ad.Code))
            {
                r.Status = "CODE_MISSING";
                r.Description = $"Document code is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(ad.DocumentType))
            {
                r.Status = "DOCTYPE_MISSING";
                r.Description = $"Document type is missing!!!";

                return r;
            }

            var isDocNoExist = await repository.IsAccountDocNoExist(ad.Code!);
            if (isDocNoExist)
            {
                r.Status = "DOC_NO_DUPLICATE";
                r.Description = $"Document number [{ad.Code}] is duplicate!!!";

                return r;
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
                Status = "SUCCESS",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(accountDocId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Account document ID [{accountDocId}] format is invalid";

                return r;
            }

            if (string.IsNullOrEmpty(ad.Code))
            {
                r.Status = "CODE_MISSING";
                r.Description = $"Document code is missing!!!";

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
                r.Description = $"Account document ID [{accountDocId}] is already approved and cannot be changed";

                return r;
            }

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
                r.Description = $"Account document ID [{accountDocId}] is already approved";

                return r;
            }

            var result = await repository!.ApproveAccountDocById(accountDocId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Account document ID [{accountDocId}] not found for the organization [{orgId}]";

                return r;
            }

            //TODO : Implement here to process point deduction or addition base on document type

            r.AccountDoc = result;
            return r;
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
