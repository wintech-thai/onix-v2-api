using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IMerchantRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<bool> IsMerchantCodeExist(string code);
        public Task<MMerchant?> GetMerchantByCode(string code);

        public Task<bool> IsMerchantNameExist(string name);
        public Task<MMerchant?> GetMerchantByName(string name);

        public Task<List<MMerchant>> GetMerchants(VMMerchant param);
        public Task<int> GetMerchantCount(VMMerchant param);
        public Task<MMerchant?> GetMerchantById(string merchantId);
        public Task<MMerchant> AddMerchant(MMerchant merchant);
        public Task<MMerchant?> DeleteMerchantById(string merchantId);
        public Task<MMerchant?> UpdateMerchantById(string merchantId, MMerchant merchant);
        public Task<MMerchant?> UpdateMerchantStatusById(string merchantId, string status);
    }
}
