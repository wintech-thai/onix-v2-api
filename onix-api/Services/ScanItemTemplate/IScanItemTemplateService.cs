using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IScanItemTemplateService
    {
        public Task<MVScanItemTemplate> GetScanItemTemplateById_V2(string orgId, string templateId);
        public Task<MScanItemTemplate?> GetScanItemTemplate_V2(string orgId);
        public Task<MVScanItemTemplate> AddScanItemTemplate_V2(string orgId, MScanItemTemplate template);
        public Task<MVScanItemTemplate> DeleteScanItemTemplateById_V2(string orgId, string templateId);
        public Task<List<MScanItemTemplate>> GetScanItemTemplates_V2(string orgId, VMScanItemTemplate param);
        public Task<int> GetScanItemTemplateCount_V2(string orgId, VMScanItemTemplate param);
        public Task<MVScanItemTemplate> UpdateScanItemTemplateById_V2(string orgId, string templateId, MScanItemTemplate template);
        public Task<MVScanItemTemplate> SetDefaultScanItemTemplateById_V2(string orgId, string templateId);
        public MScanItemTemplate GetScanItemTemplateDefault(string orgId, string userName);
    }
}
