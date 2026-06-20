using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IMasterRefRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public MMasterRef AddMasterRef(MMasterRef masterRef);
        public int GetMasterRefCount(VMMasterRef param);
        public IEnumerable<MMasterRef> GetMasterRefs(VMMasterRef param);
        public MMasterRef GetMasterRefById(string masterRefId);
        public MMasterRef? DeleteMasterRefById(string masterRefId);
        public bool IsMasterRefCodeExist(string masterRefName);
        public MMasterRef? UpdateMasterRefById(string masterRefId, MMasterRef masterRef);

        // V2 — async
        public Task<bool> IsMasterRefCodeExistV2(string code);
        public Task<MMasterRef> AddMasterRefV2(MMasterRef masterRef);
        public Task<int> GetMasterRefCountV2(VMMasterRef param);
        public Task<List<MMasterRef>> GetMasterRefsV2(VMMasterRef param);
        public Task<MMasterRef?> GetMasterRefByIdV2(string masterRefId);
        public Task<MMasterRef?> DeleteMasterRefByIdV2(string masterRefId);
        public Task<MMasterRef?> UpdateMasterRefByIdV2(string masterRefId, MMasterRef masterRef);
    }
}
