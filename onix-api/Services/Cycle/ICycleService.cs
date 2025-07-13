using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface ICycleService
    {
        public MCycle GetCycleById(string orgId, string cycleId);
        public MVCycle? AddCycle(string orgId, MCycle systemVariable);
        public MVCycle? DeleteCycleById(string orgId, string cycleId);
        public IEnumerable<MCycle> GetCycles(string orgId, VMCycle param);
        public int GetCycleCount(string orgId, VMCycle param);
        public MVCycle? UpdateCycleById(string orgId, string cycleId, MCycle systemVariable);
    }
}
