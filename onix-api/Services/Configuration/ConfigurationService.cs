using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ModelsViews;
using System.Text.Json;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class ConfigurationService : BaseService, IConfigurationService
    {
        private readonly IConfigurationRepository? repository = null;
        private readonly IFileDocumentService? _fileDocumentService = null;
        private readonly IStorageUtilsS3? _storageUtilsS3 = null;

        public ConfigurationService(
            IConfigurationRepository repo,
            IFileDocumentService fileDocumentService,
            IStorageUtilsS3 storageUtilsS3) : base()
        {
            repository = repo;
            _fileDocumentService = fileDocumentService;
            _storageUtilsS3 = storageUtilsS3;
        }

        public async Task<MVConfiguration?> GetBrandConfig(string orgId, bool needDownloadUrl = false)
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

            var bucket = Environment.GetEnvironmentVariable("MINIO_BUCKET")!;
            if (string.IsNullOrEmpty(bucket))
            {
                r.Status = "ERROR_BUCKET_NAME_NOT_CONFIGURED";
                r.Description = "Bucket name is not configured in environment variable [MINIO_BUCKET]";

                return r;
            }

            var bc = JsonSerializer.Deserialize<MBrandConfig>(result.ConfigValue!);
            if (needDownloadUrl && !string.IsNullOrEmpty(bc!.LogoPath))
            {
                var objectName = bc.LogoPath;
                var previewUrl = await _storageUtilsS3!.GenerateDownloadUrl(bucket, objectName, TimeSpan.FromMinutes(5 * 24 * 60), bc.LogoMimeType);
                var uri = new Uri(previewUrl);
                // เอาเฉพาะ path + query
                var relativeUrl = uri.PathAndQuery;
                // ใส่ placeholder
                bc.LogoImageUrl = $"<STORAGE-API-BASE>{relativeUrl}";
            }

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

            var bucket = Environment.GetEnvironmentVariable("MINIO_BUCKET")!;
            if (string.IsNullOrEmpty(bucket))
            {
                r.Status = "ERROR_BUCKET_NAME_NOT_CONFIGURED";
                r.Description = "Bucket name is not configured in environment variable [MINIO_BUCKET]";

                return r;
            }

            var mvCfg = await GetBrandConfig(orgId);

            var needToCreateDoc = false;
            if (mvCfg!.Status == "NOT_FOUND")
            {
                //ยังไม่มีของเดิมอยู่
                needToCreateDoc = true;
            }
            else
            {
                var existingConfig = mvCfg.Configuration!;
                if (config.BrandConfig.LogoPath != existingConfig.BrandConfig!.LogoPath)
                {
                    //มีการเปลี่ยนโลโก้ ต้องสร้าง document ใหม่
                    needToCreateDoc = true;
                }
            }

            if (needToCreateDoc)
            {
                var fd = new MFileDocument()
                {
                    ObjectStoragePath = config.BrandConfig.LogoPath,
                    MimeType = config.BrandConfig.LogoMimeType,
                    DocumentType = "BrandLogo",
                };

                var newFileDocument = await _fileDocumentService!.AddFileDocument(orgId, fd);
                config.BrandConfig.DocumentId = newFileDocument.FileDocument!.Id!.ToString();
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

        public async Task<MVPresignedUrl> GetBrandLogoUploadPresignedUrl(string orgId, VMUploadDocument param)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPresignedUrl()
            {
                Status = "OK",
                Description = "Success"
            };

            var bucket = Environment.GetEnvironmentVariable("MINIO_BUCKET")!;
            if (string.IsNullOrEmpty(bucket))
            {
                r.Status = "ERROR_BUCKET_NAME_NOT_CONFIGURED";
                r.Description = "Bucket name is not configured in environment variable [MINIO_BUCKET]";

                return r;
            }

            if (string.IsNullOrEmpty(param.MimeType))
            {
                r.Status = "ERROR_MIME_TYPE_IS_REQUIRED";
                r.Description = "Mime type is required in request body";

                return r;
            }

            var fileName = Guid.NewGuid().ToString();

            var objectName = $"{orgId}/brand-logo/{fileName}";
            var url = await _storageUtilsS3!.GenerateUploadUrl(bucket, objectName, TimeSpan.FromMinutes(15), param.MimeType);

            var uri = new Uri(url);
            // เอาเฉพาะ path + query
            var relativeUrl = uri.PathAndQuery;
            // ใส่ placeholder
            var resultUrl = $"<STORAGE-API-BASE>{relativeUrl}";

            r.PresignedUrl = resultUrl;
            r.ObjectName = objectName;

            return r;
        }
    }
}
