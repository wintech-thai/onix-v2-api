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

        public async Task<RevenueSummary> GetRevenueSummary(string orgId, VMSummary param)
        {
            _repo.SetCustomOrgId(orgId);

            var result = new RevenueSummary();

            var totals = await _repo.GetRevenueTotalSummary(param);
            var payIn = totals.FirstOrDefault(x => x.Direction == "PayIn");
            var payOut = totals.FirstOrDefault(x => x.Direction == "PayOut");

            result.TotalPayInAmount = payIn?.TxAmount ?? 0;
            result.TotalPayOutAmount = payOut?.TxAmount ?? 0;
            result.TotalPayInFee = payIn?.FeeAmount ?? 0;
            result.TotalPayOutFee = payOut?.FeeAmount ?? 0;
            result.TotalPayInCount = (int)(payIn?.TxCount ?? 0);
            result.TotalPayOutCount = (int)(payOut?.TxCount ?? 0);

            result.PayInByMerchant = await _repo.GetMerchantsPayInAmountSummary(param);
            result.PayOutByMerchant = await _repo.GetMerchantsPayOutAmountSummary(param);
            result.DailyRevenue = await _repo.GetDailyRevenueSummary(param);
            result.DailyMerchantRevenue = await _repo.GetDailyMerchantRevenueSummary(param);

            return result;
        }

        public async Task<ExpenseSummary> GetExpenseSummary(string orgId, VMSummary param)
        {
            _repo.SetCustomOrgId(orgId);

            var result = new ExpenseSummary();
            result.TotalAmount = await _repo.GetExpenseTotalAmount(param);
            result.TotalCount = await _repo.GetExpenseTotalCount(param);
            result.DailyExpense = await _repo.GetDailyExpenseSummary(param);
            result.ExpenseByCategory = await _repo.GetExpenseByCategorySummary(param);

            return result;
        }
    }
}
