using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IPointTriggerRepository
    {
        public void SetCustomOrgId(string customOrgId);
        
        public Task<MPointTrigger> AddPointTrigger(MPointTrigger pt);
        public Task<List<MPointTrigger>> GetPointTriggers(VMPointTrigger param);
        public Task<int> GetPointTriggerCount(VMPointTrigger param);
        public Task<MPointTrigger?> GetPointTriggerById(string triggerId);
        public Task<bool> IsTriggerNameExist(string triggerName);

    }
}
