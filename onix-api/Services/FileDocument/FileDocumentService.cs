using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Services
{
    public class FileDocumentService : BaseService, IFileDocumentService
    {
        private readonly IFileDocumentRepository? repository = null;
        private readonly IRedisHelper _redis;

        public FileDocumentService(IFileDocumentRepository repo, IRedisHelper redis) : base()
        {
            repository = repo;
            _redis = redis;
        }

        public async Task<MVFileDocument> GetFileDocumentById(string orgId, string fileDocumentId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVFileDocument()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(fileDocumentId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"File document ID [{fileDocumentId}] format is invalid";

                return r;
            }

            var result = await repository!.GetFileDocumentById(fileDocumentId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"File document ID [{fileDocumentId}] not found for the organization [{orgId}]";

                return r;
            }

            //TODO : Add logic to get pre-signed URL for object storage access and set to result.ObjectStoragePath
            //ให้ใช้จาก cache ก่อน ถ้าไม่มีค่อยไป generate ใหม่แล้วเก็บลง cache อีกทีนึง

            r.FileDocument = result;
            return r;
        }

        public async Task<MVFileDocument> AddFileDocument(string orgId, MFileDocument fileDocument)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVFileDocument
            {
                Status = "OK",
                Description = "Success"
            };

            var storagePath = fileDocument.ObjectStoragePath;

            if (string.IsNullOrEmpty(storagePath))
            {
                r.Status = "NAME_MISSING";
                r.Description = $"File document storage path is missing!!!";

                return r;
            }

            var isExist = await repository!.IsStoragePathExist(storagePath!);
            if (isExist)
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"File document storage path [{storagePath}] already exist!!!";

                return r;
            }

            var result = await repository!.AddFileDocument(fileDocument);
            r.FileDocument = result;

            return r;
        }

        public async Task<MVFileDocument> DeleteFileDocumentById(string orgId, string fileDocumentId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVFileDocument()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(fileDocumentId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"File document ID [{fileDocumentId}] format is invalid";

                return r;
            }

            var m = await repository!.DeleteFileDocumentById(fileDocumentId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"File document ID [{fileDocumentId}] not found for the organization [{orgId}]";

                return r;
            }

            //TODO : Add logic to delete file from object storage by using the storage path in m.ObjectStoragePath

            r.FileDocument = m;
            return r;
        }

        public async Task<List<MFileDocument>> GetFileDocuments(string orgId, VMFileDocument param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetFileDocuments(param);

            return result;
        }

        public async Task<int> GetFileDocumentCount(string orgId, VMFileDocument param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetFileDocumentCount(param);

            return result;
        }

        public async Task<MVFileDocument> UpdateFileDocumentById(string orgId, string fileDocumentId, MFileDocument fileDocument)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVFileDocument()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(fileDocumentId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"File document ID [{fileDocumentId}] format is invalid";

                return r;
            }

            var newStoragePath = fileDocument.ObjectStoragePath;
            var cr = await repository!.GetFileDocumentByStoragePath(newStoragePath!);
            if ((cr != null) && (cr.Id.ToString() != fileDocumentId))
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"File document storage path [{newStoragePath}] already exist!!!";

                return r;
            }

            var currentDoc = await repository!.GetFileDocumentById(fileDocumentId);
            var oldStoragePath = currentDoc?.ObjectStoragePath;

            var result = await repository!.UpdateFileDocumentById(fileDocumentId, fileDocument);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"File document ID [{fileDocumentId}] not found for the organization [{orgId}]";

                return r;
            }

            if (oldStoragePath != null && oldStoragePath != newStoragePath)
            {
                // Handle storage path change logic here if needed
                // เช่น ลบไฟล์เก่าจาก object storage ถ้า storage path เปลี่ยนไป
            }

            r.FileDocument = result;

            return r;
        }
    }
}
