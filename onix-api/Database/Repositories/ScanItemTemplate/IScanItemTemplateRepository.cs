using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IScanItemTemplateRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<bool> IsScanItemTemplateExist(string templateName);
        public Task<MScanItemTemplate> GetScanItemTemplate_V2();
        public Task<List<MScanItemTemplate>> GetScanItemTemplates_V2(VMScanItemTemplate param);
        public Task<int> GetScanItemTemplateCount_V2(VMScanItemTemplate param);
        public Task<MScanItemTemplate?> GetScanItemTemplateById_V2(string templateId);
        public Task<MScanItemTemplate> AddScanItemTemplate_V2(MScanItemTemplate template);
        public Task<MScanItemTemplate?> DeleteScanItemTemplateById_V2(string templateId);
        public Task<MScanItemTemplate?> UpdateScanItemTemplateById_V2(string templateId, MScanItemTemplate template);
        public Task<MScanItemTemplate?> SetScanItemTemplateDefault_V2(string templateId);
    }
}
