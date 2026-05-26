using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IFileDocumentRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<bool> IsStoragePathExist(string storagePath);
        public Task<MFileDocument?> GetFileDocumentByStoragePath(string storagePath);
        public Task<List<MFileDocument>> GetFileDocuments(VMFileDocument param);
        public Task<int> GetFileDocumentCount(VMFileDocument param);
        public Task<MFileDocument?> GetFileDocumentById(string fileDocumentId);
        public Task<MFileDocument> AddFileDocument(MFileDocument fileDocument);
        public Task<MFileDocument?> DeleteFileDocumentById(string fileDocumentId);
        public Task<MFileDocument?> UpdateFileDocumentById(string fileDocumentId, MFileDocument fileDocument);
    }
}
