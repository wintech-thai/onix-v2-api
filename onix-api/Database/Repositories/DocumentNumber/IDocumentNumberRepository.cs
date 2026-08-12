using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IDocumentNumberRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<MDocumentNumberConfig> AddDocumentNumberConfigAsync(MDocumentNumberConfig config);
        public Task<MDocumentNumberConfig?> GetDocumentNumberConfigByIdAsync(string id);
        public Task<List<MDocumentNumberConfig>> GetDocumentNumberConfigsAsync(VMDocumentNumberConfig param);
        public Task<int> GetDocumentNumberConfigCountAsync(VMDocumentNumberConfig param);
        public Task<MDocumentNumberConfig?> UpdateDocumentNumberConfigByIdAsync(string id, MDocumentNumberConfig config);
        public Task<MDocumentNumberConfig?> DeleteDocumentNumberConfigByIdAsync(string id);
        public Task<bool> IsDocumentTypeExistAsync(string documentType);
        public Task<MDocumentNumberConfig?> GetDocumentNumberConfigByTypeAsync(string documentType);
        public Task<MDocumentNumberConfig?> UpdateDocumentNumberSequenceAsync(string id, int sequenceNo, string sequenceKey, string? lastDocumentNumber = null);
    }
}
