using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IDocumentNumberService
    {
        public Task<MDocumentNumberConfig> AddDocumentNumberConfig(string orgId, MDocumentNumberConfig config);
        public Task<MDocumentNumberConfig?> GetDocumentNumberConfigById(string orgId, string id);
        public Task<List<MDocumentNumberConfig>> GetDocumentNumberConfigs(string orgId, VMDocumentNumberConfig param);
        public Task<int> GetDocumentNumberConfigCount(string orgId, VMDocumentNumberConfig param);
        public Task<MDocumentNumberConfig?> UpdateDocumentNumberConfigById(string orgId, string id, MDocumentNumberConfig config);
        public Task<MDocumentNumberConfig?> DeleteDocumentNumberConfigById(string orgId, string id);
        public Task<string> GetDocumentNumber(string orgId, string documentType);
        public Task<MDocumentNumberConfig?> UpdateDocumentNumberSequence(string orgId, string id, int sequenceNo, string sequenceKey);
    }
}
