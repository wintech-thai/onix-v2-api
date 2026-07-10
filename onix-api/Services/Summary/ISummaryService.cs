using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface ISummaryService
    {
        public Task<MerchantSummary> GetMerchantSummary(string orgId, VMSummary param);
        public Task<RevenueSummary> GetRevenueSummary(string orgId, VMSummary param);
        public Task<ExpenseSummary> GetExpenseSummary(string orgId, VMSummary param);
    }
}
