using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface ISummaryRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<List<MerchantSummaryData>> GetMerchantCountByStatus(VMSummary param);
        public Task<List<MerchantSummaryData>> GetMerchantCount(VMSummary param);
        public Task<List<MerchantSummaryData>> GetMerchantsBalance();
        public Task<List<MerchantSummaryData>> GetMerchantsPayInAmountSummary(VMSummary param);
        public Task<List<MerchantSummaryData>> GetMerchantsPayOutAmountSummary(VMSummary param);
        public Task<List<RevenueSummaryData>> GetRevenueTotalSummary(VMSummary param);
        public Task<List<DailyRevenueSummaryData>> GetDailyRevenueSummary(VMSummary param);
        public Task<List<DailyMerchantRevenueSummaryData>> GetDailyMerchantRevenueSummary(VMSummary param);
        public Task<decimal> GetExpenseTotalAmount(VMSummary param);
        public Task<int> GetExpenseTotalCount(VMSummary param);
        public Task<List<DailyExpenseSummaryData>> GetDailyExpenseSummary(VMSummary param);
        public Task<List<ExpenseByCategoryData>> GetExpenseByCategorySummary(VMSummary param);
    }
}
