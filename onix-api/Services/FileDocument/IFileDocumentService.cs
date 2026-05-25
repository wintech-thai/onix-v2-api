using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IFileDocumentService
    {
        public Task<MVFileDocument> GetFileDocumentById(string orgId, string fileDocumentId);
        public Task<MVFileDocument> AddFileDocument(string orgId, MFileDocument fileDocument);
        public Task<MVFileDocument> DeleteFileDocumentById(string orgId, string fileDocumentId);
        public Task<List<MFileDocument>> GetFileDocuments(string orgId, VMFileDocument param);
        public Task<int> GetFileDocumentCount(string orgId, VMFileDocument param);
        public Task<MVFileDocument> UpdateFileDocumentById(string orgId, string fileDocumentId, MFileDocument fileDocument);
    }
}
