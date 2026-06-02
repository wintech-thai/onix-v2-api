using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IConfigurationRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<MConfiguration?> GetConfigurationByType(string configType);
        public Task<MConfiguration> UpsertConfiguration(MConfiguration config);
        public Task<MConfiguration?> SetConfigurationStatusById(string configId, string status);
    }
}
