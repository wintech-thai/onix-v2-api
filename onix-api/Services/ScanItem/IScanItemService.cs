using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Services
{
    public interface IScanItemService
    {
        public MVScanItemResult VerifyScanItem(string orgId, string serial, string pin);
    }
}
