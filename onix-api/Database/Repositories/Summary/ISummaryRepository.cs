using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface ISummaryRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<List<MAggregateData>> GetMerchantCountByStatus(VMSummary param);
        public Task<List<MAggregateData>> GetMerchantCount(VMSummary param);
        public Task<List<MAggregateData>> GetMerchantsBalance();
    }
}
