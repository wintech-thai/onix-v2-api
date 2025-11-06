using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface ILimitService
    {
        public Task<int> GetLimitCount(string orgId, VMLimit param);
        public Task<IEnumerable<MLimit>> GetLimits(string orgId, VMLimit param);
        public Task<MVLimit> UpsertLimit(string orgId, MLimit limit);
    }
}
