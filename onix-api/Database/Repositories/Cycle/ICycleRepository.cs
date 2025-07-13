using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface ICycleRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public MCycle AddCycle(MCycle cycle);
        public int GetCycleCount(VMCycle param);
        public IEnumerable<MCycle> GetCycles(VMCycle param);
        public MCycle GetCycleById(string cycleId);
        public MCycle? DeleteCycleById(string cycleId);
        public bool IsCycleCodeExist(string cycleName);
        public MCycle? UpdateCycleById(string cycleId, MCycle cycle);
    }
}
