using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface ISystemVariableRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public MSystemVariable AddSystemVariable(MSystemVariable systemVariable);
        public int GetSystemVariableCount(VMSystemVariable param);
        public IEnumerable<MSystemVariable> GetSystemVariables(VMSystemVariable param);
        public MSystemVariable GetSystemVariableById(string systemVariableId);
        public MSystemVariable GetSystemVariableByName(string systemVariableName);
        public MSystemVariable? DeleteSystemVariableById(string systemVariableId);
        public bool IsSystemVariableNameExist(string systemVariableName);
        public MSystemVariable? UpdateSystemVariableById(string systemVariableId, MSystemVariable systemVariable);
    }
}
