using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;
using System.Text.Json;

namespace Its.Onix.Api.Services
{
    public class MerchantService : BaseService, IMerchantService
    {
        private readonly IMerchantRepository? repository = null;
        private readonly IOrganizationRepository? _orgRepo = null;
        private readonly IBankAccountRepository? _bankAccountRepo = null;
        private readonly IPointRepository? _pointRepo = null;
        private readonly IRedisHelper _redis;

        public MerchantService(IMerchantRepository repo,
            IApiKeyRepository apiKeyRepo,
            IBankAccountRepository bankAccountRepo,
            IPointRepository pointRepo,
            IRedisHelper redis,
            IOrganizationRepository orgRepo) : base()
        {
            repository = repo;
            _orgRepo = orgRepo;
            _redis = redis;
            _pointRepo = pointRepo;
            _bankAccountRepo = bankAccountRepo;
        }

        private async Task<MTxBalance> GetMerchantCurrentDailyTxBalance(string orgId, string merchantId)
        {
            var r = new MTxBalance()
            {
                TxCount = 0,
                TxAmount = 0
            };

            var key = CacheHelper.CreateMerchantDailyTxKey(orgId, merchantId);
            var cacheValue = await _redis.GetObjectAsync<MTxBalance>(key);
            if (cacheValue != null)
            {
                r = cacheValue;
            }

            return r;
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

            // ดึงข้อมูลจาก cache
            var currentDailyTxBalance = await GetMerchantCurrentDailyTxBalance("global", merchantId);
            result.CurrentPayinDailyTxCount = currentDailyTxBalance.TxCount;
            result.CurrentPayinDailyTxAmount = currentDailyTxBalance.TxAmount;

            _pointRepo!.SetCustomOrgId(result.OrgId!); //เอา OrgId ของ merchant มาใช้ได้เลย
            var wallet = await _pointRepo!.GetWalletByMerchantId(merchantId);
            if (wallet == null)
            {
                //ยังไม่เคยสร้าง wallet มาก่อนก็สร้างให้เลย
                var w = new MWallet()
                {
                    Name = $"THB:{merchantId}",
                    MerchantId = merchantId,
                    PointBalance = 0,
                    PointBalanceDecimal = 0,
                    Tags = $"MerchantName={result.Name}",
                    Description = $"Auto generated wallet for THB currency for [{result.Name}]",
                };

                var _ = await _pointRepo.AddWallet(w);
            }

            DeserializeWhitelistBankAccountNames(result);

            r.Merchant = result;

            return r;
        }

        private static void DeserializeWhitelistBankAccountNames(MMerchant merchant)
        {
            if (!string.IsNullOrEmpty(merchant.WhitelistBankAccountNames))
            {
                try
                {
                    merchant.WhitelistBankAccountNamesArr = JsonSerializer.Deserialize<List<string>>(merchant.WhitelistBankAccountNames);
                }
                catch { merchant.WhitelistBankAccountNamesArr = null; }
            }

            //ไม่ควร return ออกมาเป็น JSON string ดิบให้คนเรียกใช้ API เห็น
            merchant.WhitelistBankAccountNames = "";
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

            if (merchant.WhitelistBankAccountNamesArr != null)
            {
                merchant.WhitelistBankAccountNames = JsonSerializer.Serialize(merchant.WhitelistBankAccountNamesArr);
            }

            var result = await repository!.AddMerchant(merchant);

            if (result != null)
            {
                var merchantId = result.Id.ToString()!;

                //เพิ่ม wallet ให้อัตโนมัติ
                _pointRepo!.SetCustomOrgId(result.OrgId!); //เอา OrgId ของ merchant มาใช้ได้เลย
                var wallet = await _pointRepo!.GetWalletByMerchantId(merchantId);
                if (wallet == null)
                {
                    //ยังไม่เคยสร้าง wallet มาก่อนก็สร้างให้เลย
                    var w = new MWallet()
                    {
                        Name = $"THB:{merchantId}",
                        MerchantId = merchantId,
                        PointBalance = 0,
                        PointBalanceDecimal = 0,
                        Tags = $"MerchantName={result.Name}",
                        Description = $"Auto generated wallet for THB currency for [{result.Name}]",
                    };

                    var _ = await _pointRepo.AddWallet(w);
                }

                DeserializeWhitelistBankAccountNames(result);
            }

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
            _pointRepo!.SetCustomOrgId(orgId);

            var merchantBankAccountsAggr = await _bankAccountRepo!.GetBankAccountCountByMerchantId();
            var dict1 = merchantBankAccountsAggr.ToDictionary(g => $"{g.MerchantId!}:{g.AccountCategory}", g => g.BankAccountCount);

            var merchantBalanceAggr = await _pointRepo!.GetWalletBalancesGroupByMerchantId();
            var dict2 = merchantBalanceAggr.ToDictionary(g => $"{g.MerchantId!}", g => g.PointBalanceDecimal);

            var merchants = await repository!.GetMerchants(param);


            foreach (var merchant in merchants)
            {
                var payInKey = $"{merchant.Id.ToString()}:PayIn";
                var payOutKey = $"{merchant.Id.ToString()}:PayOut";

                merchant.PayInBankAccountCount = 0;
                if (dict1.TryGetValue(payInKey, out var payInBankAccountCount))
                {
                    merchant.PayInBankAccountCount = payInBankAccountCount;
                }

                merchant.PayOutBankAccountCount = 0;
                if (dict1.TryGetValue(payOutKey, out var payOutBankAccountCount))
                {
                    merchant.PayOutBankAccountCount = payOutBankAccountCount;
                }

                merchant.CurrentBalance = 0;
                if (dict2.TryGetValue(merchant.Id.ToString()!, out var currentWalletBalance))
                {
                    merchant.CurrentBalance = currentWalletBalance;
                }
            }

            return merchants;
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

            if (merchant.WhitelistBankAccountNamesArr != null)
            {
                merchant.WhitelistBankAccountNames = JsonSerializer.Serialize(merchant.WhitelistBankAccountNamesArr);
            }

            var result = await repository!.UpdateMerchantById(merchantId, merchant);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Merchant ID [{merchantId}] not found for the organization [{orgId}]";

                return r;
            }

            DeserializeWhitelistBankAccountNames(result);

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
