using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IStatRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public int GetStatCount(VMStat param);
        public IEnumerable<MStat> GetStats(VMStat param);
    }
}
