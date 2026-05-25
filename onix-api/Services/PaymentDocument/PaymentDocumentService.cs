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

        public PaymentDocumentService(
            IPaymentDocumentRepository repo, 
            IFileDocumentService fileDocumentService) : base()
        {
            repository = repo;
            _fileDocumentService = fileDocumentService;
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
    }
}
