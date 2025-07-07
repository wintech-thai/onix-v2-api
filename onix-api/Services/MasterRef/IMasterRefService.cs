using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IMasterRefService
    {
        public MMasterRef GetMasterRefById(string orgId, string systemVariableId);
        public MVMasterRef? AddMasterRef(string orgId, MMasterRef systemVariable);
        public MVMasterRef? DeleteMasterRefById(string orgId, string systemVariableId);
        public IEnumerable<MMasterRef> GetMasterRefs(string orgId, VMMasterRef param);
        public int GetMasterRefCount(string orgId, VMMasterRef param);
        public MVMasterRef? UpdateMasterRefById(string orgId, string systemVariableId, MMasterRef systemVariable);
    }
}
