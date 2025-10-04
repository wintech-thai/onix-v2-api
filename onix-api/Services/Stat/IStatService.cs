using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IStatService
    {
        public IEnumerable<MStat> GetStats(string orgId, VMStat param);
        public int GetStatCount(string orgId, VMStat param);
    }
}
