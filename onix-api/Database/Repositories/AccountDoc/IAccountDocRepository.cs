using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IAccountDocRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<bool> IsAccountDocNoExist(string docNo);
        public Task<bool> IsAccountDocApproved(string accountDocId);
        public Task<bool> IsAccountDocItemApproved(string accountDocItemId);

        public Task<MAccountDoc> AddAccountDoc(MAccountDoc ad);
        public Task<MAccountDoc?> UpdateAccountDocById(string accountDocId, MAccountDoc ad);
        public Task<MAccountDoc?> ApproveAccountDocById(string accountDocId);
        public Task<MAccountDoc?> DeleteAccountDocById(string accountDocId);
        public Task<List<MAccountDoc>> GetAccountDocs(VMAccountDoc param);
        public Task<int> GetAccountDocCount(VMAccountDoc param);
        public Task<MAccountDoc?> GetAccountDocById(string accountDocId);
        public Task<List<MAccountDocItem>> GetAccountDocItemsById(string accountDocId);
        public Task<MAccountDocItem> AddAccountDocItem(string accountDocId, MAccountDocItem adi);
        public Task<MAccountDocItem?> UpdateAccountDocItemById(string accountDocItemId, MAccountDocItem adi);
        public Task<MAccountDocItem?> DeleteAccountDocItemById(string accountDocItemId);
    }
}
