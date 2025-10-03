using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IScanItemTemplateService
    {
        public MScanItemTemplate GetScanItemTemplateById(string orgId, string templateId);
        public MScanItemTemplate GetScanItemTemplate(string orgId);
        public MScanItemTemplate GetScanItemTemplateDefault(string orgId, string userName);
        public MVScanItemTemplate? AddScanItemTemplate(string orgId, MScanItemTemplate action);
        public MVScanItemTemplate? DeleteScanItemTemplateById(string orgId, string templateId);
        public int GetScanItemTemplateCount(string orgId, VMScanItemTemplate param);
        public MVScanItemTemplate? UpdateScanItemTemplateById(string orgId, string templateId, MScanItemTemplate template);
    }
}
