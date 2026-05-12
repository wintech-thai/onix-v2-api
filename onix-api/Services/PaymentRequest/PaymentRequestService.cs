using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;
using System.Text.Json;
using System.Text;

namespace Its.Onix.Api.Services
{
    public class PaymentRequestService : BaseService, IPaymentRequestService
    {
        private readonly IPaymentRequestRepository? repository = null;

        public PaymentRequestService(IPaymentRequestRepository repo) : base()
        {
            repository = repo;
        }

        public async Task<MVPaymentRequest> GetPaymentRequestById(string orgId, string paymentRequestId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPaymentRequest()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(paymentRequestId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Payment Request ID [{paymentRequestId}] format is invalid";

                return r;
            }

            var result = await repository!.GetPaymentRequestById(paymentRequestId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Request ID [{paymentRequestId}] not found for the organization [{orgId}]";

                return r;
            }

            r.PaymentRequest = result;

            return r;
        }

        public async Task<MVPaymentResponse> AddPaymentRequestPayIn(string orgId, MPaymentRequest paymentRequest)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPaymentResponse()
            {
                Status = "OK",
                Description = "Success",
            };

            if (string.IsNullOrEmpty(paymentRequest.RefId))
            {
                r.Status = "REF_ID_MISSING";
                r.Description = $"Ref ID is missing!!!";

                return r;
            }

            if (paymentRequest.Currency != "THB")
            {
                r.Status = "CURRENCY_NOT_SUPPORT";
                r.Description = $"Currency [{paymentRequest.Currency}] not currently support, only THB is allowed.";

                return r;
            }

            if (paymentRequest.QrProvider != "PP") //PromptPay
            {
                //ตอนนี้ support แค่ PromptPay
                r.Status = "BANK_PROVIDER_NOT_SUPPORT";
                r.Description = $"Provider [{paymentRequest.QrProvider}] not currently support, only PP is allowed.";

                return r;
            }

            if (paymentRequest.RequestedAmount <= 0)
            {
                r.Status = "INVALID_PAYMENT_AMOUNT";
                r.Description = $"Request amount [{paymentRequest.RequestedAmount}] must be greater than 0.00";

                return r;
            }

            //TODO : เพิ่ม logic สำหรับการสร้าง QR payment ตรงนี้
            paymentRequest.ResponseData = "This should not be seen data";
            paymentRequest.Status = "Pending";
            paymentRequest.Direction = "PayIn";

            var pmResponse = CreatePaymentResponse(paymentRequest);
            var jsonString = JsonSerializer.Serialize(pmResponse);

            paymentRequest.ResponseData = jsonString;
            _ = await repository!.AddPaymentRequest(paymentRequest);

            r.PaymentResponse = pmResponse;
            return r;
        }

        private MPaymentResponse CreatePaymentResponse(MPaymentRequest pr)
        {
            //TODO : Implement this
            var pmr = new MPaymentResponse()
            {
                CreatedAt = pr.CreatedDate,
                ExpireAt = pr.ExpireDate,

                Id = pr.Id.ToString(),
                ReferenceId = pr.RefId,
                Type = pr.Direction,
                Status = pr.Status,
                RequestedAmount = pr.RequestedAmount,
                GeneratedAmount = pr.GeneratedAmount,
                Currency = pr.Currency,
            };

            return pmr;
        }

        public async Task<List<MPaymentRequest>> GetPaymentRequests(string orgId, VMPaymentRequest param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPaymentRequests(param);

            //ไม่ให้ return ออกไปเพราะว่าใหญ่มาก
            result.ForEach(p => p.ResponseData = "");

            return result;
        }

        public async Task<int> GetPaymentRequestCount(string orgId, VMPaymentRequest param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPaymentRequestCount(param);

            return result;
        }

        public async Task<MVPaymentRequest> UpdatePaymentRequestById(string orgId, string paymentRequestId, MPaymentRequest paymentRequest)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPaymentRequest()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(paymentRequestId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Payment Request ID [{paymentRequestId}] format is invalid";

                return r;
            }

            var result = await repository!.UpdatePaymentRequestById(paymentRequestId, paymentRequest);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Request ID [{paymentRequestId}] not found for the organization [{orgId}]";

                return r;
            }

            r.PaymentRequest = result;

            return r;
        }
    }
}
