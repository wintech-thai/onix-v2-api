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
    }
}
