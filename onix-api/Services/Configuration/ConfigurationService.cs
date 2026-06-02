using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ModelsViews;
using System.Text.Json;

namespace Its.Onix.Api.Services
{
    public class ConfigurationService : BaseService, IConfigurationService
    {
        private readonly IConfigurationRepository? repository = null;

        public ConfigurationService(IConfigurationRepository repo) : base()
        {
            repository = repo;
        }

        public async Task<MVConfiguration?> GetBrandConfig(string orgId)
        {
            var r = new MVConfiguration() 
            { 
                Status = "OK",
                Description = "Brand configuration retrieved successfully"
            };

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetConfigurationByType("Brand");

            if (result == null)
            {
                r.Status = "NOT_FOUND";
                r.Description = "Brand configuration not found for the specified organization";
                return r;
            }

            var bc = JsonSerializer.Deserialize<MBrandConfig>(result.ConfigValue!);
            result.BrandConfig = bc;

            r.Configuration = result;
            r.Configuration.ConfigValue = "";

            return r;
        }

        public async Task<MVConfiguration> SetBrandConfig(string orgId, MConfiguration config)
        {
            repository!.SetCustomOrgId(orgId);
            
            var r = new MVConfiguration() 
            { 
                Status = "OK",
                Description = "Brand configuration set successfully"
            };

            if (config.BrandConfig == null)
            {
                r.Status = "CONFIG_VALUE_MISSING";
                r.Description = "Brand configuration data is missing";
                return r;
            }

            var jsonString = JsonSerializer.Serialize(config.BrandConfig);
            config.ConfigValue = jsonString;

            config.ConfigType = "Brand";
            var c = await repository!.UpsertConfiguration(config);

            r.Configuration = c;
            r.Configuration.ConfigValue = "";

            return r;
        }

        public async Task<MVConfiguration?> SetConfigStatusById(string orgId, string configId, string status)
        {
            repository!.SetCustomOrgId(orgId);
            var config = await repository!.SetConfigurationStatusById(configId, status);

            if (config == null)
            {
                return null;
            }

            var r = new MVConfiguration()
            {
                Status = "OK",
                Description = "Configuration status updated successfully"
            };

            r.Configuration = config;
            r.Configuration.ConfigValue = "";

            return r;
        }
    }
}
