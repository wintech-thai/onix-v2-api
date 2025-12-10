using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IScanItemFolderRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<bool> IsScanItemFolderExist(string folderName);
        public Task<MScanItemFolder> GetScanItemFolder();
        public Task<List<MScanItemFolder>> GetScanItemFolders(VMScanItemFolder param);
        public Task<int> GetScanItemFolderCount(VMScanItemFolder param);
        public Task<MScanItemFolder?> GetScanItemFolderById(string folderId);
        public Task<MScanItemFolder> AddScanItemFolder(MScanItemFolder folder);
        public Task<MScanItemFolder?> DeleteScanItemFolderById(string folderId);
        public Task<MScanItemFolder?> UpdateScanItemFolderById(string folderId, MScanItemFolder folder);
        public Task<MScanItemFolder?> AttachScanItemFolderToAction(string folderId, string actionId);
    }
}
