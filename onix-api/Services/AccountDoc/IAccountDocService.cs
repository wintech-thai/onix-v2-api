using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IAccountDocService
    {
        public Task<MVAccountDoc> AddAccountDoc(string orgId, MAccountDoc ad);
        public Task<MVAccountDoc> UpdateAccountDocById(string orgId, string accountDocId, MAccountDoc ad);
        public Task<MVAccountDoc> ApproveAccountDocById(string orgId, string accountDocId);
        public Task<MVAccountDoc> DeleteAccountDocById(string orgId, string accountDocId);
        public Task<List<MAccountDoc>> GetAccountDocs(string orgId, VMAccountDoc param);
        public Task<int> GetAccountDocCount(string orgId, VMAccountDoc param);
        public Task<MVAccountDoc> GetAccountDocById(string orgId, string accountDocIdId);
        public Task<MVAccountDocItem> AddAccountDocItem(string orgId, string accountDocId, MAccountDocItem adi);
        public Task<List<MAccountDocItem>> GetAccountDocItemsById(string orgId, string accountDocId);
        public Task<MVAccountDocItem> UpdateAccountDocItemById(string orgId, string accountDocItemId, MAccountDocItem adi);
        public Task<MVAccountDocItem> DeleteAccountDocItemById(string orgId, string accountDocItemId);
        public Task<MVAccountDocItem> CalculateAccountDocItemPrice(string orgId, MAccountDocItem adi);
    }
}
