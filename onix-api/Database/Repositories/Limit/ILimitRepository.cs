using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface ILimitRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<int> GetLimitCount(VMLimit param);
        public Task<IEnumerable<MLimit>> GetLimits(VMLimit param);
        public Task<MLimit> UpsertLimit(MLimit limit);
    }
}
