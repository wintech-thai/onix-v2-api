using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Services
{
    public interface IConfigurationService
    {
        public Task<MVConfiguration?> GetBrandConfig(string orgId);
        public Task<MVConfiguration> SetBrandConfig(string orgId, MConfiguration config);
    }
}
