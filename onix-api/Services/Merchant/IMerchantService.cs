using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IMerchantService
    {
        public Task<MVMerchant> GetMerchantById(string orgId, string merchantId);
        public Task<MVMerchant> AddMerchant(string orgId, MMerchant merchant);
        public Task<MVMerchant> DeleteMerchantById(string orgId, string merchantId);
        public Task<List<MMerchant>> GetMerchants(string orgId, VMMerchant param);
        public Task<int> GetMerchantCount(string orgId, VMMerchant param);
        public Task<MVMerchant> UpdateMerchantById(string orgId, string merchantId, MMerchant merchant);
        public Task<MVMerchant?> UpdateMerchantStatusById(string merchantId, string status);
    }
}
