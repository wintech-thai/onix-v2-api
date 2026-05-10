using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Services
{
    public class MerchantService : BaseService, IMerchantService
    {
        private readonly IMerchantRepository? repository = null;
        private readonly IOrganizationRepository? _orgRepo = null;
        //private readonly IRedisHelper _redis;

        public MerchantService(IMerchantRepository repo,
            IApiKeyRepository apiKeyRepo,
            //IRedisHelper redis,
            IOrganizationRepository orgRepo) : base()
        {
            repository = repo;
            _orgRepo = orgRepo;
           // _redis = redis;
        }

        public async Task<MVMerchant> GetMerchantById(string orgId, string merchantId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVMerchant()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(merchantId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Merchant ID [{merchantId}] format is invalid";

                return r;
            }

            var result = await repository!.GetMerchantById(merchantId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Merchant ID [{merchantId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Merchant = result;

            return r;
        }

        public async Task<MVMerchant> AddMerchant(string orgId, MMerchant merchant)
        {
            repository!.SetCustomOrgId(orgId);
            _orgRepo!.SetCustomOrgId(orgId);

            var r = new MVMerchant()
            {
                Status = "OK",
                Description = "Success",
            };


            if (string.IsNullOrEmpty(merchant.Code))
            {
                r.Status = "CODE_MISSING";
                r.Description = $"Merchant code is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(merchant.Name))
            {
                r.Status = "NAME_MISSING";
                r.Description = $"Merchant name is missing!!!";

                return r;
            }

            var isCodeExist = await repository!.IsMerchantCodeExist(merchant.Code);
            if (isCodeExist)
            {
                r.Status = "CODE_DUPLICATE";
                r.Description = $"Merchant code [{merchant.Code}] already exist!!!";

                return r;
            }

            var isNameExist = await repository!.IsMerchantNameExist(merchant.Name);
            if (isNameExist)
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"Merchant name [{merchant.Name}] already exist!!!";

                return r;
            }

            var result = await repository!.AddMerchant(merchant);

            r.Merchant = result;
            return r;
        }

        public async Task<MVMerchant> DeleteAgentById(string orgId, string merchantId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVMerchant()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(merchantId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Merchant ID [{merchantId}] format is invalid";

                return r;
            }

            var currentAgent = await GetMerchantById(orgId, merchantId);
            if (currentAgent.Merchant == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Merchant ID [{merchantId}] not found for the organization [{orgId}]";

                return r;
            }


            var m = await repository!.DeleteMerchantById(merchantId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Merchant ID [{merchantId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Merchant = m;
            return r;
        }

        public async Task<List<MMerchant>> GetMerchants(string orgId, VMMerchant param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetMerchants(param);

            return result;
        }

        public async Task<int> GetMerchantCount(string orgId, VMMerchant param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetMerchantCount(param);

            return result;
        }

        public async Task<MVMerchant> UpdateMerchantById(string orgId, string merchantId, MMerchant merchant)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVMerchant()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(merchantId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Merchant ID [{merchantId}] format is invalid";

                return r;
            }

            var code = merchant.Code;
            var cr = await repository!.GetMerchantByCode(code!);
            if ((cr != null) && (cr.Id.ToString() != merchantId))
            {
                r.Status = "CODE_DUPLICATE";
                r.Description = $"Merchant code [{code}] already exist!!!";

                return r;
            }

            var name = merchant.Name;
            var mr = await repository!.GetMerchantByName(name!);
            if ((mr != null) && (mr.Id.ToString() != merchantId))
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"Merchant name [{name}] already exist!!!";

                return r;
            }

            var result = await repository!.UpdateMerchantById(merchantId, merchant);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Merchant ID [{merchantId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Merchant = result;
            return r;
        }

        public async Task<MVMerchant> AddMerchantStat(string orgId, MMerchant merchant)
        {
           repository!.SetCustomOrgId(orgId);

            var r = new MVMerchant()
            {
                Status = "OK",
                Description = "Success",
            };

            if (string.IsNullOrEmpty(merchant.Code))
            {
                r.Status = "CODE_MISSING";
                r.Description = $"Merchant code is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(merchant.Name))
            {
                r.Status = "NAME_MISSING";
                r.Description = $"Merchant name is missing!!!";

                return r;
            }

            var isCodeExist = await repository!.IsMerchantCodeExist(merchant.Code);
            if (isCodeExist)
            {
                r.Status = "CODE_DUPLICATE";
                r.Description = $"Merchant code [{merchant.Code}] already exist!!!";

                return r;
            }

            var isNameExist = await repository!.IsMerchantNameExist(merchant.Name);
            if (isNameExist)
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"Merchant name [{merchant.Name}] already exist!!!";

                return r;
            }

            var result = await repository!.AddMerchant(merchant);
            r.Merchant = result;

            return r;
        }

        public async Task<MVMerchant> DeleteMerchantById(string orgId, string merchantId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVMerchant()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(merchantId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Merchant ID [{merchantId}] format is invalid";

                return r;
            }

            var result = await repository!.DeleteMerchantById(merchantId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Merchant ID [{merchantId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Merchant = result;
            return r;
        }

        public async Task<MVMerchant?> UpdateMerchantStatusById(string merchantId, string status)
        {
            var r = new MVMerchant()
            {
                Status = "OK",
                Description = "Success"
            };

            var result = await repository!.UpdateMerchantStatusById(merchantId, status);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Merchant ID [{merchantId}] not found";

                return r;
            }

            var orgId = result.OrgId;
            _orgRepo!.SetCustomOrgId(orgId!);

            var org = await _orgRepo.UpdateOrganizationStatus(status);
            if (org == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Org ID [{orgId}] not found";

                return r;
            }

            r.Merchant = result;

            return r;
        }
    }
}
