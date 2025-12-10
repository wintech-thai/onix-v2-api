using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IScanItemFolderService
    {
        public Task<MVScanItemFolder> GetScanItemFolderById(string orgId, string folderId);
        public Task<MVScanItemFolder> AddScanItemFolder(string orgId, MScanItemFolder folder);
        public Task<MVScanItemFolder> DeleteScanItemFolderById(string orgId, string folderId);
        public Task<List<MScanItemFolder>> GetScanItemFolders(string orgId, VMScanItemFolder param);
        public Task<int> GetScanItemFolderCount(string orgId, VMScanItemFolder param);
        public Task<MVScanItemFolder> UpdateScanItemFolderById(string orgId, string folderId, MScanItemFolder folder);
        public Task<MVScanItemFolder> AttachScanItemFolderToAction(string orgId, string folderId, string actionId);
        public Task<MVScanItemFolder> AttachScanItemFolderToProduct(string orgId, string folderId, string productId);
    }
}
