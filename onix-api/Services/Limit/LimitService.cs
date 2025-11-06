using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Services
{
    public class LimitService : BaseService, ILimitService
    {
        private readonly ILimitRepository? repository = null;

        public LimitService(ILimitRepository repo) : base()
        {
            repository = repo;
        }

        public async Task<int> GetLimitCount(string orgId, VMLimit param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetLimitCount(param);

            return result;
        }

        public async Task<IEnumerable<MLimit>> GetLimits(string orgId, VMLimit param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetLimits(param);

            return result;
        }

        public async Task<MVLimit> UpsertLimit(string orgId, MLimit limit)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVLimit()
            {
                Status = "SUCCESS",
                Description = "Success",
            };

            if (string.IsNullOrEmpty(limit.StatCode))
            {
                r.Status = "INVALID_STAT_CODE";
                r.Description = "StatCode name must not be blank!!!";

                return r;
            }

            var result = await repository!.UpsertLimit(limit);
            if (result == null)
            {
                r.Status = "UNKNOWN_ERROR";
                r.Description = $"Unknown error";

                return r;
            }

            r.Limit = result;
            return r;
        }
    }
}
