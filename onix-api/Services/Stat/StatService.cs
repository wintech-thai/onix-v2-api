using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class StatService : BaseService, IStatService
    {
        private readonly IStatRepository? repository = null;

        public StatService(IStatRepository repo) : base()
        {
            repository = repo;
        }

        public IEnumerable<MStat> GetStats(string orgId, VMStat param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetStats(param);

            return result;
        }

        public int GetStatCount(string orgId, VMStat param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetStatCount(param);

            return result;
        }
    }
}
