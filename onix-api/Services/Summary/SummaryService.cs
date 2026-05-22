using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Microsoft.IdentityModel.Tokens;

namespace Its.Onix.Api.Services
{
    public class SummaryService : BaseService, ISummaryService
    {
        private readonly ISummaryRepository _repo;

        public SummaryService(ISummaryRepository repo)
        {
            _repo = repo;
        }

        public async Task<MerchantSummary> GetMerchantSummary(string orgId, VMSummary param)
        {
            _repo.SetCustomOrgId(orgId);

            var result = new MerchantSummary
            {
                MerchantCount = 0
            };

            var summ1 = await _repo.GetMerchantCount(param);
            if (!summ1.IsNullOrEmpty())
            {
                var t = summ1[0];
                result.MerchantCount = (int) t.MerchantCount!;
            }

            result.MerchantCountByStatus = await _repo.GetMerchantCountByStatus(param);
            result.MerchantsBalances = await _repo.GetMerchantsBalance();

            result.MerchantsPayInSummary = await _repo.GetMerchantsPayInAmountSummary(param);
            result.MerchantsPayOutSummary = await _repo.GetMerchantsPayOutAmountSummary(param);

            return result;
        }
    }
}
