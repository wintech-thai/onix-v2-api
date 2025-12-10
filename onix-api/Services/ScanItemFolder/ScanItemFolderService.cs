using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class ScanItemFolderService : BaseService, IScanItemFolderService
    {
        private readonly IScanItemFolderRepository? repository = null;

        public ScanItemFolderService(IScanItemFolderRepository repo) : base()
        {
            repository = repo;
        }

        public async Task<MVScanItemFolder> GetScanItemFolderById(string orgId, string folderId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVScanItemFolder()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(folderId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"ScanItemFolder ID [{folderId}] format is invalid";

                return r;
            }

            var result = await repository!.GetScanItemFolderById(folderId);
            r.ScanItemFolder = result;

            return r;
        }

        public async Task<MScanItemFolder?> GetScanItemFolder(string orgId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetScanItemFolder();

            return result;
        }

        private MVScanItemFolder ValidateFolder(MScanItemFolder folder)
        {
            var r = new MVScanItemFolder()
            {
                Status = "OK",
                Description = "Success",
            };

            return r;
        }

        public async Task<MVScanItemFolder> AddScanItemFolder(string orgId, MScanItemFolder folder)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVScanItemFolder();
            r.Status = "OK";
            r.Description = "Success";

            if (string.IsNullOrEmpty(folder.FolderName))
            {
                r.Status = "NAME_MISSING";
                r.Description = $"Folder name is missing!!!";

                return r;
            }

            var isExist = await repository!.IsScanItemFolderExist(folder.FolderName);
            if (isExist)
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"Folder name [{folder.FolderName}] already exist!!!";

                return r;
            }

            var validateResult = ValidateFolder(folder);
            if (validateResult.Status != "OK")
            {
                return validateResult;
            }

            var result = await repository!.AddScanItemFolder(folder);
            r.ScanItemFolder = result;

            return r;
        }

        public async Task<MVScanItemFolder> DeleteScanItemFolderById(string orgId, string folderId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVScanItemFolder()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(folderId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"ScanItem Folder ID [{folderId}] format is invalid";

                return r;
            }

            var m = await repository!.DeleteScanItemFolderById(folderId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"ScanItem Folder ID [{folderId}] not found for the organization [{orgId}]";

                return r;
            }

            r.ScanItemFolder = m;
            return r;
        }

        public async Task<int> GetScanItemFolderCount(string orgId, VMScanItemFolder param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetScanItemFolderCount(param);

            return result;
        }

        public async Task<List<MScanItemFolder>> GetScanItemFolders(string orgId, VMScanItemFolder param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetScanItemFolders(param);

            return result;
        }

        public async Task<MVScanItemFolder> AttachScanItemFolderToProduct(string orgId, string folderId, string productId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVScanItemFolder()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(folderId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"ScanItem Folder ID [{folderId}] format is invalid";

                return r;
            }

            if (!ServiceUtils.IsGuidValid(productId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"ScanItem Product ID [{folderId}] format is invalid";

                return r;
            }

            var result = await repository!.AttachScanItemFolderToProduct(folderId, productId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"ScanItem Folder ID [{folderId}] not found for the organization [{orgId}]";

                return r;
            }

            r.ScanItemFolder = result;
            return r;
        }

        public async Task<MVScanItemFolder> AttachScanItemFolderToAction(string orgId, string folderId, string actionId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVScanItemFolder()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(folderId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"ScanItem Folder ID [{folderId}] format is invalid";

                return r;
            }

            if (!ServiceUtils.IsGuidValid(actionId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"ScanItem Action ID [{folderId}] format is invalid";

                return r;
            }

            var result = await repository!.AttachScanItemFolderToAction(folderId, actionId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"ScanItem Folder ID [{folderId}] not found for the organization [{orgId}]";

                return r;
            }

            r.ScanItemFolder = result;
            return r;
        }

        public async Task<MVScanItemFolder> UpdateScanItemFolderById(string orgId, string folderId, MScanItemFolder folder)
        {
            //TODO : Check if folder name is duplicate
            repository!.SetCustomOrgId(orgId);

            var r = new MVScanItemFolder()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(folderId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"ScanItem Folder ID [{folderId}] format is invalid";

                return r;
            }

            var validateResult = ValidateFolder(folder);
            if (validateResult.Status != "OK")
            {
                return validateResult;
            }

            var result = await repository!.UpdateScanItemFolderById(folderId, folder);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"ScanItem Folder ID [{folderId}] not found for the organization [{orgId}]";

                return r;
            }

            r.ScanItemFolder = result;
            return r;
        }
    }
}
