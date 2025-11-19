using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IPointTriggerService
    {
        public Task<MVPointTrigger> AddPointTrigger(string orgId, PointTriggerInput pt);
        public Task<MVPointTrigger?> GetPointTriggerById(string orgId, string pointTriggerId);
        public Task<List<MPointTrigger>> GetPointTriggers(string orgId, VMPointTrigger param);
        public Task<int> GetPointTriggersCount(string orgId, VMPointTrigger param);
    }
}
