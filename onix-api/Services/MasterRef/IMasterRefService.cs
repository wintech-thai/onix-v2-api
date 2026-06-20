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

        // V2 — async
        public Task<MVMasterRef> GetMasterRefByIdV2(string orgId, string masterRefId);
        public Task<MVMasterRef> AddMasterRefV2(string orgId, MMasterRef masterRef);
        public Task<MVMasterRef> UpdateMasterRefByIdV2(string orgId, string masterRefId, MMasterRef masterRef);
        public Task<MVMasterRef> DeleteMasterRefByIdV2(string orgId, string masterRefId);
        public Task<List<MMasterRef>> GetMasterRefsV2(string orgId, VMMasterRef param);
        public Task<int> GetMasterRefCountV2(string orgId, VMMasterRef param);
    }
}
