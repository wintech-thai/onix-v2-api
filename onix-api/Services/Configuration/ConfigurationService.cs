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
        private readonly IRedisHelper _redis;

        public ConfigurationService(
            IConfigurationRepository repo,
            IFileDocumentService fileDocumentService,
            IStorageUtilsS3 storageUtilsS3,
            IRedisHelper redis) : base()
        {
            repository = repo;
            _fileDocumentService = fileDocumentService;
            _storageUtilsS3 = storageUtilsS3;
            _redis = redis;
        }

        public async Task<MVConfiguration?> GetBrandConfig(string orgId, bool needDownloadUrl = false)
        {
            var r = new MVConfiguration() 
            { 
                Status = "OK",
                Description = "Brand configuration retrieved successfully"
            };

            repository!.SetCustomOrgId(orgId);

            var cacheKey = CacheHelper.CreateBrandConfigKey(orgId);
            var result = await _redis.GetObjectAsync<MConfiguration>(cacheKey);
            if (result == null)
            {
                result = await repository!.GetConfigurationByType("Brand");
                if (result != null)
                {
                    await _redis.SetObjectAsync(cacheKey, result, TimeSpan.FromHours(24));
                }
            }

            if (result == null)
            {
                r.Status = "NOT_FOUND";
                r.Description = "Brand configuration not found for the specified organization";
                return r;
            }

            var bc = JsonSerializer.Deserialize<MBrandConfig>(result.ConfigValue!);
            var hasLogo = !string.IsNullOrEmpty(bc!.DocumentId) || (!string.IsNullOrEmpty(bc.LogoPath) && bc.LogoPath != "DEFAULT");
            if (needDownloadUrl && hasLogo)
            {
                // Points at our own API instead of MinIO/storage-api now — <API-BASE> is replaced by the frontend,
                // same convention as the old <STORAGE-API-BASE> placeholder.
                bc.LogoImageUrl = "<API-BASE>/admin-api/AdminConfiguration/org/global/action/GetBrandLogoImage";
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

            var mvCfg = await GetBrandConfig(orgId);
            var existingBc = (mvCfg!.Status == "OK") ? mvCfg.Configuration!.BrandConfig : null;

            var hasNewLogo = !string.IsNullOrEmpty(config.BrandConfig.LogoBase64);
            if (hasNewLogo)
            {
                var fd = new MFileDocument()
                {
                    FileContent = config.BrandConfig.LogoBase64,
                    MimeType = config.BrandConfig.LogoMimeType,
                    DocumentType = "BrandLogo",
                };

                if (!string.IsNullOrEmpty(existingBc?.DocumentId))
                {
                    //มีโลโก้เดิมอยู่แล้ว ให้ update ทับ document เดิมแทนที่จะสร้างแถวใหม่ทุกครั้ง
                    var updatedFileDocument = await _fileDocumentService!.UpdateFileDocumentById(orgId, existingBc.DocumentId!, fd);
                    config.BrandConfig.DocumentId = updatedFileDocument.FileDocument!.Id!.ToString();
                }
                else
                {
                    var newFileDocument = await _fileDocumentService!.AddFileDocument(orgId, fd);
                    config.BrandConfig.DocumentId = newFileDocument.FileDocument!.Id!.ToString();
                }
            }
            else
            {
                //ไม่มีการอัปโหลดโลโก้ใหม่ ให้คงค่า document ของโลโก้เดิมไว้
                config.BrandConfig.DocumentId = existingBc?.DocumentId;
            }

            config.BrandConfig.LogoBase64 = null; //ห้ามเก็บ raw base64 ลงใน ConfigValue

            var jsonString = JsonSerializer.Serialize(config.BrandConfig);
            config.ConfigValue = jsonString;

            config.ConfigType = "Brand";
            var c = await repository!.UpsertConfiguration(config);

            await _redis.DeleteAsync(CacheHelper.CreateBrandConfigKey(orgId));
            await _redis.DeleteAsync(CacheHelper.CreateBrandLogoImageKey(orgId));

            r.Configuration = c;
            r.Configuration.ConfigValue = "";

            return r;
        }

        public async Task<MVConfiguration?> GetBackupPolicy(string orgId)
        {
            var r = new MVConfiguration()
            {
                Status = "OK",
                Description = "Backup policy retrieved successfully"
            };

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetConfigurationByType("BackupPolicy");

            if (result == null)
            {
                r.Status = "NOT_FOUND";
                r.Description = "Backup policy not found";
                return r;
            }

            if (!string.IsNullOrEmpty(result.ConfigValue))
            {
                result.BackupPolicy = JsonSerializer.Deserialize<MBackupPolicy>(result.ConfigValue);
                result.ConfigValue = "";
            }

            r.Configuration = result;
            return r;
        }

        public async Task<MVConfiguration> SetBackupPolicy(string orgId, MConfiguration config)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVConfiguration()
            {
                Status = "OK",
                Description = "Backup policy saved successfully"
            };

            if (config.BackupPolicy == null)
            {
                r.Status = "CONFIG_VALUE_MISSING";
                r.Description = "Backup policy data is missing";
                return r;
            }

            config.ConfigType = "BackupPolicy";
            config.ConfigValue = JsonSerializer.Serialize(config.BackupPolicy);
            config.Status = "Active";

            var c = await repository!.UpsertConfiguration(config);
            c.ConfigValue = "";
            r.Configuration = c;
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

            //Status เปลี่ยนแล้วต้อง invalidate cache ของ config ประเภทนั้น ๆ ไม่งั้น GetBrandConfig/GetClientIpSource จะยังอ่านค่า Status เก่าจาก cache ต่อไปอีกนานสุด 24 ชม.
            if (config.ConfigType == "Brand")
            {
                await _redis.DeleteAsync(CacheHelper.CreateBrandConfigKey(orgId));
            }
            else if (!string.IsNullOrEmpty(config.ConfigType) && config.ConfigType.StartsWith("ClientIpSource_"))
            {
                var scope = config.ConfigType.Substring("ClientIpSource_".Length);
                await _redis.DeleteAsync(CacheHelper.CreateClientIpSourceKey(orgId, scope));
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

        public async Task<MVConfiguration?> GetClientIpSource(string orgId, string scope)
        {
            var r = new MVConfiguration()
            {
                Status = "OK",
                Description = "Client IP source configuration retrieved successfully"
            };

            var configType = $"ClientIpSource_{scope}";
            var cacheKey = CacheHelper.CreateClientIpSourceKey(orgId, scope);
            var result = await _redis.GetObjectAsync<MConfiguration>(cacheKey);
            if (result == null)
            {
                repository!.SetCustomOrgId(orgId);
                result = await repository!.GetConfigurationByType(configType);
                if (result != null)
                {
                    await _redis.SetObjectAsync(cacheKey, result, TimeSpan.FromHours(24));
                }
            }

            if (result == null)
            {
                r.Status = "NOT_FOUND";
                r.Description = "Client IP source configuration not found";
                return r;
            }

            if (!string.IsNullOrEmpty(result.ConfigValue))
            {
                result.ClientIpSourceConfig = JsonSerializer.Deserialize<MClientIpSourceConfig>(result.ConfigValue);
                result.ConfigValue = "";
            }

            r.Configuration = result;
            return r;
        }

        public async Task<MVConfiguration> SetClientIpSource(string orgId, string scope, MConfiguration config)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVConfiguration()
            {
                Status = "OK",
                Description = "Client IP source configuration saved successfully"
            };

            if (config.ClientIpSourceConfig == null)
            {
                r.Status = "CONFIG_VALUE_MISSING";
                r.Description = "Client IP source configuration data is missing";
                return r;
            }

            config.ConfigType = $"ClientIpSource_{scope}";
            config.ConfigValue = JsonSerializer.Serialize(config.ClientIpSourceConfig);
            config.Status = "Active";

            var c = await repository!.UpsertConfiguration(config);

            await _redis.DeleteAsync(CacheHelper.CreateClientIpSourceKey(orgId, scope));

            c.ConfigValue = "";
            r.Configuration = c;
            return r;
        }

        [Obsolete("Brand logo is now uploaded as base64 directly via SetBrandConfig. Kept only until legacy clients stop calling it.")]
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

        public async Task<(byte[]? Bytes, string? MimeType)> GetBrandLogoImageBytes(string orgId)
        {
            var cacheKey = CacheHelper.CreateBrandLogoImageKey(orgId);
            var cached = await _redis.GetObjectAsync<CachedBrandLogo>(cacheKey);
            if (cached != null)
            {
                if (!cached.Exists) return (null, null);
                return (Convert.FromBase64String(cached.Base64!), cached.MimeType);
            }

            string? base64 = null;
            string? mimeType = null;

            var mvCfg = await GetBrandConfig(orgId);
            var bc = (mvCfg!.Status == "OK") ? mvCfg.Configuration!.BrandConfig : null;

            if (!string.IsNullOrEmpty(bc?.DocumentId))
            {
                var fdResult = await _fileDocumentService!.GetFileDocumentById(orgId, bc.DocumentId!);
                var fd = fdResult.FileDocument;

                if (!string.IsNullOrEmpty(fd?.FileContent))
                {
                    base64 = fd.FileContent;
                    mimeType = fd.MimeType;
                }
                else if (!string.IsNullOrEmpty(fd?.ObjectStoragePath))
                {
                    //ยังไม่เคย migrate มาเป็น base64 - โลโก้เดิมยังอยู่ที่ MinIO ให้ดึงมาเก็บเป็น FileContent แทนตอนนี้เลย
                    var bucket = Environment.GetEnvironmentVariable("MINIO_BUCKET");
                    if (!string.IsNullOrEmpty(bucket))
                    {
                        var bytes = await _storageUtilsS3!.DownloadObjectAsync(bucket, fd.ObjectStoragePath!);
                        if (bytes != null)
                        {
                            base64 = Convert.ToBase64String(bytes);
                            mimeType = fd.MimeType;

                            var migrated = new MFileDocument()
                            {
                                ObjectStoragePath = fd.ObjectStoragePath,
                                FileContent = base64,
                                MimeType = mimeType,
                            };
                            await _fileDocumentService!.UpdateFileDocumentById(orgId, fd.Id!.ToString()!, migrated);
                        }
                    }
                }
            }

            var toCache = new CachedBrandLogo()
            {
                Exists = base64 != null,
                Base64 = base64,
                MimeType = mimeType,
            };
            await _redis.SetObjectAsync(cacheKey, toCache, TimeSpan.FromHours(24));

            if (base64 == null) return (null, null);
            return (Convert.FromBase64String(base64), mimeType);
        }

        private class CachedBrandLogo
        {
            public bool Exists { get; set; }
            public string? Base64 { get; set; }
            public string? MimeType { get; set; }
        }
    }
}
