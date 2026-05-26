using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;
using System.Text.Json;

namespace Its.Onix.Api.Services
{
    public class PaymentDocumentService : BaseService, IPaymentDocumentService
    {
        private readonly IPaymentDocumentRepository? repository = null;
        private readonly IFileDocumentService? _fileDocumentService = null;
        private readonly IStorageUtilsS3? _storageUtilsS3 = null;

        public PaymentDocumentService(
            IPaymentDocumentRepository repo,
            IStorageUtilsS3 storageUtilsS3,
            IFileDocumentService fileDocumentService) : base()
        {
            repository = repo;
            _fileDocumentService = fileDocumentService;
            _storageUtilsS3 = storageUtilsS3;
        }

        public async Task<MVPresignedUrl> GetPayInSlipUploadPresignedUrl(string orgId, MMerchant merchant, VMUploadDocument param)
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

            var merchantId = merchant.Id!.ToString();
            var fileName = Guid.NewGuid().ToString();

            var objectName = $"{orgId}/{merchantId}/pay-in-slip/{fileName}";
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

        public async Task<MVPaymentDocument> GetPaymentDocumentById(string orgId, string paymentDocumentId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPaymentDocument()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(paymentDocumentId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Payment Doc ID [{paymentDocumentId}] format is invalid";

                return r;
            }

            var bucket = Environment.GetEnvironmentVariable("MINIO_BUCKET")!;
            if (string.IsNullOrEmpty(bucket))
            {
                r.Status = "ERROR_BUCKET_NAME_NOT_CONFIGURED";
                r.Description = "Bucket name is not configured in environment variable [MINIO_BUCKET]";

                return r;
            }

            var result = await repository!.GetPaymentDocumentById(paymentDocumentId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Doc ID [{paymentDocumentId}] not found for the organization [{orgId}]";

                return r;
            }

            if (orgId != "global")
            {
                //Filter ว่า data ที่ส่งออกไปต้องเป็นของ orgId นั้น ๆ เท่าน้้น
                if (result.OrgId != orgId)
                {
                    r.Status = "ERROR_DATA_NOT_OWN_BY_ORG_ID";
                    r.Description = $"Payment Doc ID [{paymentDocumentId}] own by organization [{orgId}] --> [{result.OrgId}]";

                    return r;
                }
            }

            List<string> lines;
            try
            {
                lines = JsonSerializer.Deserialize<List<string>>(result.ProcessingMessages!) ?? new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR1 - [{ex.Message}]");
                lines = [];
            }

            if (string.IsNullOrEmpty(result.MimeType))
            {
                r.Status = "ERROR_MIME_TYPE_IS_REQUIRED";
                r.Description = "Mime type is required in request body";

                return r;
            }

            var objectName = result.UploadedFilePath!;
            if (!string.IsNullOrEmpty(objectName))
            {
                var previewUrl = await _storageUtilsS3!.GenerateDownloadUrl(bucket, objectName, TimeSpan.FromMinutes(15), result.MimeType);

                var uri = new Uri(previewUrl);
                // เอาเฉพาะ path + query
                var relativeUrl = uri.PathAndQuery;
                // ใส่ placeholder
                result.PreviewUrl = $"<STORAGE-API-BASE>{relativeUrl}";
            }

            result.ProcessingSteps = lines;
            result.ProcessingMessages = "";

            r.PaymentDocument = result;

            return r;
        }

        public async Task<List<MPaymentDocument>> GetPaymentDocuments(string orgId, VMPaymentDocument param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPaymentDocuments(param);

            // ลบ ResponseData ออกเพื่อลด payload
            result.ForEach(p => 
            { 
                p.ProcessingMessages = "";
            });

            // ถ้าไม่ใช่ global ให้เหลือเฉพาะรายการของ orgId นั้น
            if (orgId != "global")
            {
                //ป้องกันความผิดพลาดไม่ให้ payment request ของ org หนึ่งไปโผล่ในอีก org หนึ่ง
                result.RemoveAll(p => p.OrgId != orgId);
            }

            return result;
        }

        public async Task<int> GetPaymentDocumentCount(string orgId, VMPaymentDocument param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPaymentDocumentCount(param);

            return result;
        }

        public async Task<MVPaymentDocument> AddPaymentDocument(string orgId, MPaymentDocument paymentDocument)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPaymentDocument()
            {
                Status = "OK",
                Description = "Success"
            };

            var fd = new MFileDocument()
            {
                ObjectStoragePath = paymentDocument.UploadedFilePath,
                MimeType = paymentDocument.MimeType,
                DocumentType = paymentDocument.DocumentType,
            };

            var newFileDocument = await _fileDocumentService!.AddFileDocument(orgId, fd);

            paymentDocument.FileDocumentId = newFileDocument.FileDocument!.Id!.ToString();

            var result = await repository!.AddPaymentDocument(paymentDocument);
            if (result == null)
            {
                r.Status = "ERROR_ADD_PAYMENT_DOCUMENT";
                r.Description = $"Error while adding payment document for the organization [{orgId}]";

                return r;
            }

            r.PaymentDocument = result;

            return r;
        }

        public async Task<MVPaymentDocument> UpdatePaymentDocumentById(string orgId, string paymentDocumentId, MPaymentDocument paymentDocument)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPaymentDocument()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(paymentDocumentId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Payment Doc ID [{paymentDocumentId}] format is invalid";

                return r;
            }

            //TODO : เช็คว่าต้องเป็น Pending เท่านั้นถึงจะ Update ได้

            var result = await repository!.UpdatePaymentDocumentById(paymentDocumentId, paymentDocument);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Doc ID [{paymentDocumentId}] not found for the organization [{orgId}]";

                return r;
            }

            r.PaymentDocument = result;

            return r;
        }

        public async Task<MVPaymentDocument> ApprovePaymentDocumentById(string orgId, string paymentDocumentId, MPaymentDocument paymentDocument)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPaymentDocument()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(paymentDocumentId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Payment Doc ID [{paymentDocumentId}] format is invalid";

                return r;
            }

            //เช็คว่า fields ที่จำเป็นต้องถูก populate เข้ามาด้วย ประกอบไปด้วย
            // MerchantId, RefId, PayInBankAccountId, TxAmount, TxAmountDecimal, Currency
            if (string.IsNullOrEmpty(paymentDocument.MerchantId) || 
                string.IsNullOrEmpty(paymentDocument.RefId) || 
                string.IsNullOrEmpty(paymentDocument.Currency) || 
                string.IsNullOrEmpty(paymentDocument.PayInBankAccountId) || 
                paymentDocument.TxAmount == null || 
                paymentDocument.TxAmountDecimal == null)
            {
                r.Status = "ERROR_REQUIRED_FIELDS_MISSING";
                r.Description = "Required fields missing. Please make sure MerchantId, RefId, Currency, PayInBankAccountId, TxAmount and TxAmountDecimal are provided in request body";

                return r;
            }

            var existingPd = await repository!.GetPaymentDocumentById(paymentDocumentId);
            if (existingPd == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Doc ID [{paymentDocumentId}] not found for the organization [{orgId}]";

                return r;
            }

            //เช็คว่าต้องเป็น Pending เท่านั้นถึงจะ Approve ได้
            if (existingPd.Status != "Pending")
            {
                r.Status = "ERROR_ONLY_PENDING_PAYMENT_DOCUMENT_CAN_BE_APPROVED";
                r.Description = $"Only payment document with status 'Pending' can be approved. Current status of this payment document is [{existingPd.Status}]";

                return r;
            }

            //เช็คว่าไม่เคยมี RefId ไหนที่ถูก Approve มาก่อน
            var prevApprovedPdWithSameRefId = await repository!.GetApprovedPaymentDocumentByRefId(paymentDocument.RefId!);
            if (prevApprovedPdWithSameRefId != null)
            {
                r.Status = "ERROR_REF_ID_ALREADY_USED_BY_APPROVED_PAYMENT_DOCUMENT";
                r.Description = $"Ref ID [{paymentDocument.RefId}] is already used by another approved payment document with ID [{prevApprovedPdWithSameRefId.Id}], OrgId=[{prevApprovedPdWithSameRefId.OrgId}]";

                return r;
            }

            //TODO : ให้สร้าง Payment Transaction ขึ้นมาใหม่ด้วย โดยมีข้อมูลบางส่วนมาจาก Payment Document ตัวนี้ และมีการเชื่อมโยงกันผ่าน PaymentDocumentId
            //เอา PaymentTransactionId ไปใส่ใน PaymentDocument ด้วย เผื่อไว้สำหรับการอ้างอิงในอนาคต

            var result = await repository!.ApprovePaymentDocumentById(paymentDocumentId, paymentDocument);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Doc ID [{paymentDocumentId}] not found for the organization [{orgId}]";

                return r;
            }

            r.PaymentDocument = result;

            return r;
        }

        public async Task<MVPaymentDocument> RejectPaymentDocumentById(string orgId, string paymentDocumentId, MPaymentDocument paymentDocument)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPaymentDocument()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(paymentDocumentId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Payment Doc ID [{paymentDocumentId}] format is invalid";

                return r;
            }

            if (paymentDocument.RejectReason == null || paymentDocument.RejectReason.Trim() == "")
            {
                r.Status = "ERROR_REJECT_REASON_IS_REQUIRED";
                r.Description = "Reject reason is required in request body when rejecting a payment document";

                return r;
            }

            var existingPd = await repository!.GetPaymentDocumentById(paymentDocumentId);
            if (existingPd == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Doc ID [{paymentDocumentId}] not found for the organization [{orgId}]";

                return r;
            }

            //เช็คว่าต้องเป็น Pending เท่านั้นถึงจะ Reject ได้
            if (existingPd.Status != "Pending")
            {
                r.Status = "ERROR_ONLY_PENDING_PAYMENT_DOCUMENT_CAN_BE_REJECTED";
                r.Description = $"Only payment document with status 'Pending' can be rejected. Current status of this payment document is [{existingPd.Status}]";

                return r;
            }

            var result = await repository!.RejectPaymentDocumentById(paymentDocumentId, paymentDocument);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Doc ID [{paymentDocumentId}] not found for the organization [{orgId}]";

                return r;
            }

            r.PaymentDocument = result;

            return r;
        }
    }
}
