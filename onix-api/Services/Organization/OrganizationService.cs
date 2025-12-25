using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;
using System.Text.Json;

namespace Its.Onix.Api.Services
{
    public class OrganizationService : BaseService, IOrganizationService
    {
        private readonly IOrganizationRepository? repository = null;
        private readonly IOrganizationUserRepository _orgUserRepo;
        private readonly IUserService userService;
        private readonly IStorageUtils _storageUtil;

        public OrganizationService(
            IOrganizationRepository repo,
            IOrganizationUserRepository orgUserRepo,
            IUserService userSvc,
            IStorageUtils storageUtil) : base()
        {
            repository = repo;
            userService = userSvc;
            _storageUtil = storageUtil;
            _orgUserRepo = orgUserRepo;
        }

        public bool IsOrgIdExist(string orgId)
        {
            var isExist = repository!.IsCustomOrgIdExist(orgId!);
            return isExist;
        }

        public MVOrganization AddOrganization(string orgId, MOrganization org)
        {
            var customOrgId = org.OrgCustomId;
            var r = new MVOrganization();

            var isExist = repository!.IsCustomOrgIdExist(customOrgId!);
            if (isExist)
            {
                r.Status = "ORGANIZATION_DUPLICATE";
                r.Description = $"Organization ID is duplicate [{customOrgId}] !!!";

                return r;
            }

            repository!.SetCustomOrgId(customOrgId!);
            var result = repository!.AddOrganization(org);

            r.Status = "OK";
            r.Description = "Success";
            r.Organization = result;

            return r;
        }

        public Task<MOrganization> GetOrganization(string orgId)
        {
            repository!.SetCustomOrgId(orgId);
            var t = repository!.GetOrganization();

            var org = t.Result;

            if (!string.IsNullOrEmpty(org.LogoImagePath))
            {
                var validFor = TimeSpan.FromMinutes(60);
                var contentType = "image/png";
                org.LogoImageUrl = _storageUtil.GenerateDownloadUrl(org.LogoImagePath!, validFor, contentType);
            }
                
            return t;
        }

        public IEnumerable<MOrganizationUser> GetUserAllowedOrganization(string userName)
        {
            var result = repository!.GetUserAllowedOrganization(userName);
            return result;
        }

        public bool IsUserNameExist(string orgId, string userName)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.IsUserNameExist(userName);

            return result;
        }

        private ValidationResult ValidateImageFormat(string bucket, MOrganization org)
        {
            ulong maxFileSize = 1 * 1024 * 1024; // 1 MB
            int maxWidth = 512;
            int maxHeight = 512;

            //วิธีนี้ใช้ได้แต่เฉพาะไฟล์ PNG เท่านั้น
            var r = new ValidationResult() { Status = "OK", Description = "" };

            var obj = _storageUtil.GetStorageObject(bucket, org.LogoImagePath!);
            if (obj == null)
            {
                r.Status = "OBJECT_NOT_FOUND";
                r.Description = $"Object name [{org.LogoImagePath}] not found !!!";
                return r;
            }

            if (obj.Size > maxFileSize)
            {
                r.Status = "FILE_TOO_BIG";
                r.Description = $"File must be less than 1MB !!!";
                return r;
            }

            if (obj.ContentType != "image/png")
            {
                r.Status = "FILE_TYPE_NOT_PNG";
                r.Description = $"Content type must be 'image/png' !!!";
                return r;
            }

            var t = _storageUtil.PartialDownloadToStream(bucket, org.LogoImagePath!, 0, 24);
            var header = t.Result;

            if (header.Length < 24)
            {
                r.Status = "NOT_VALID_PNG_FILE";
                r.Description = "File is not a valid PNG image!!!";
                return r;
            }

            int width = ServiceUtils.ReadInt32BigEndian(header, 16);
            int height = ServiceUtils.ReadInt32BigEndian(header, 20);

            if (width > maxWidth || height > maxHeight)
            {
                r.Status = "INVALID_IMAGE_DIMENSION";
                r.Description = $"Image dimention [w={width},h={height}] must be less than [w={maxWidth},h={maxHeight}]";
                return r;
            }

            return r;
        }

        private void DeleteStorageObject(MOrganization m)
        {
            var objectName = m.LogoImagePath;
            var bucket = Environment.GetEnvironmentVariable("STORAGE_BUCKET")!;
            _storageUtil.DeleteObject(bucket, objectName!);
        }


        private void NormalizeFields(MOrganization org)
        {
            if (org.ChannelsArray == null)
            {
                org.ChannelsArray = [];
            }
            org.Channels = JsonSerializer.Serialize(org.ChannelsArray);

            if (org.AddressesArray == null)
            {
                org.AddressesArray = [];
            }
            org.Addresses = JsonSerializer.Serialize(org.AddressesArray);
        }
        
        public Task<MVOrganization> UpdateOrganization(string orgId, MOrganization org)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVOrganization()
            {
                Status = "OK",
                Description = "Success"
            };

            NormalizeFields(org);

            if (!string.IsNullOrEmpty(org.LogoImagePath))
            {
                if (!_storageUtil.IsObjectExist(org.LogoImagePath))
                {
                    r.Status = "OBJECT_NOT_FOUND";
                    r.Description = $"Object name [{org.LogoImagePath}] not found!!!";

                    return Task.FromResult(r);
                }

                var bucket = Environment.GetEnvironmentVariable("STORAGE_BUCKET")!;

                //Allow only PNG to be uploaded
                var validateResult = ValidateImageFormat(bucket, org);
                if (validateResult.Status != "OK")
                {
                    //ให้ลบไฟล์ที่ upload มาออกไปเลย ไม่เก็บไว้ให้เป็นภาระ
                    DeleteStorageObject(org);

                    r.Status = validateResult.Status;
                    r.Description = validateResult.Description;
                    return Task.FromResult(r);
                }

                //Update metadata onix-is-temp-file to 'false'
                _storageUtil.UpdateMetaData(bucket, org.LogoImagePath, "onix-is-temp-file", "false");
            }

            var t = repository.UpdateOrganization(org);
            r.Organization = t.Result;

            return Task.FromResult(r);
        }

        public MVPresignedUrl GetLogoImageUploadPresignedUrl(string orgId)
        {
            var type = "png";
            var sec = "logo";

            var bucket = Environment.GetEnvironmentVariable("STORAGE_BUCKET")!;
            var objectName = $"{Environment.GetEnvironmentVariable("ENV_GROUP")}/{orgId}/Organization/{orgId}.{sec}.{type}";
            var validFor = TimeSpan.FromMinutes(15);
            var contentType = $"image/{type}";

            var url = _storageUtil.GenerateUploadUrl(bucket, objectName, validFor, contentType);
            var previewUrl = _storageUtil.GenerateDownloadUrl(objectName, validFor, contentType);

            var result = new MVPresignedUrl()
            {
                Status = "SUCCESS",
                Description = "",
                PresignedUrl = url,
                ObjectName = objectName,
                ImagePath = objectName,
                PreviewUrl = previewUrl,
            };

            return result;
        }

        public MVOrganizationUser AddUserToOrganization(string orgId, MOrganizationUser user)
        {
            //Improvement(validation) : Added validation here

            repository!.SetCustomOrgId(orgId);
            var r = new MVOrganizationUser();

            var f1 = userService.IsUserNameExist(orgId, user!.UserName!);
            if (!f1)
            {
                r.Status = "USER_NAME_NOTFOUND";
                r.Description = $"User name not found [{user.UserName}] !!!";

                return r;
            }

            var f2 = userService.IsUserIdExist(orgId, user!.UserId!);
            if (!f2)
            {
                r.Status = "USER_ID_NOTFOUND";
                r.Description = $"User ID not found [{user.UserId}] !!!";

                return r;
            }

            var f3 = IsUserNameExist(orgId, user!.UserName!);
            if (f3)
            {
                r.Status = "USER_DUPLICATE";
                r.Description = $"User [{user.UserName}] already in organization !!!";

                return r;
            }

            var result = repository!.AddUserToOrganization(user);

            r.Status = "OK";
            r.Description = "Success";
            r.OrgUser = result;

            return r;
        }

        public MVOrganizationUser VerifyUserInOrganization(string orgId, string userName)
        {
            repository!.SetCustomOrgId(orgId);
            _orgUserRepo.SetCustomOrgId(orgId);

            var u = userService.GetUserByName(orgId, userName);
            if (u == null)
            {
                var o = new MVOrganizationUser()
                {
                    Status = "NOTFOUND",
                    Description = $"User [{userName}] not found !!!"
                };

                return o;
            }

            //User _orgUserRepo instead
            var m = _orgUserRepo!.GetUserInOrganization(userName);
            if (m == null)
            {
                var o = new MVOrganizationUser()
                {
                    Status = "NOTFOUND_USER_IN_ORG",
                    Description = $"User [{userName}] has not been added to the organization [{orgId}] !!!",
                };

                return o;
            }

            if (m.UserStatus != "Active")
            {
                var o = new MVOrganizationUser()
                {
                    Status = "NOT_ACTIVE_STATUS_USER",
                    Description = $"User [{userName}] has status [{m.UserStatus}] in the organization [{orgId}] !!!",
                };

                return o;
            }

            var mv = new MVOrganizationUser()
            {
                User = u,
                OrgUser = m,
                Status = "OK",
                Description = "Success",
            };

            return mv;
        }

        public IEnumerable<NameValue> GetAllowChannelNames(string orgId)
        {
            repository!.SetCustomOrgId(orgId);

            var channels = new NameValue[]
            {
                new() { Name = "Company Website", Value = "https://" },
                new() { Name = "LINE", Value = "" },
                new() { Name = "Instragram", Value = "" },
                new() { Name = "Facebook", Value = "" },
                new() { Name = "Tiktok", Value = "" },
                new() { Name = "Youtube", Value = "" },
            };

            return channels;
        }

        public IEnumerable<NameValue> GetAllowAddressTypeNames(string orgId)
        {
            repository!.SetCustomOrgId(orgId);

            var addresses = new NameValue[]
            {
                new() { Name = "Default Address", Value = "" },
                new() { Name = "Billing Address", Value = "" },
                new() { Name = "Delivery Address", Value = "" },
            };

            return addresses;
        }
    }
}