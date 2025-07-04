using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class SystemVariableService : BaseService, ISystemVariableService
    {
        private readonly ISystemVariableRepository? repository = null;

        public SystemVariableService(ISystemVariableRepository repo) : base()
        {
            repository = repo;
        }

        public MSystemVariable GetSystemVariableById(string orgId, string iocHostId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetSystemVariableById(iocHostId);

            return result;
        }

        public MSystemVariable GetSystemVariableByName(string orgId, string systemVariableName)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetSystemVariableByName(systemVariableName);

            return result;
        }

        public MVSystemVariable? AddSystemVariable(string orgId, MSystemVariable systemVariable)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVSystemVariable();

            var isExist = repository!.IsSystemVariableNameExist(systemVariable.VariableName!);

            if (isExist)
            {
                r.Status = "DUPLICATE";
                r.Description = $"System variable [{systemVariable.VariableName}] is duplicate";

                return r;
            }

            var result = repository!.AddSystemVariable(systemVariable);

            r.Status = "OK";
            r.Description = "Success";
            r.SystemVariable = result;

            return r;
        }

        public MVSystemVariable? UpdateSystemVariableById(string orgId, string systemVariableId, MSystemVariable systemVariable)
        {
            //TODO : Implement this
            return null;
        }

        public MVSystemVariable? DeleteSystemVariableById(string orgId, string systemVariableId)
        {
            var r = new MVSystemVariable()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(systemVariableId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"System variable ID [{systemVariableId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = repository!.DeleteSystemVariableById(systemVariableId);

            r.SystemVariable = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"System variable ID [{systemVariableId}] not found for the organization [{orgId}]";
            }

            return r;
        }

        public IEnumerable<MSystemVariable> GetSystemVariables(string orgId, VMSystemVariable param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetSystemVariables(param);

            return result;
        }

        public int GetSystemVariableCount(string orgId, VMSystemVariable param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetSystemVariableCount(param);

            return result;
        }
    }
}
