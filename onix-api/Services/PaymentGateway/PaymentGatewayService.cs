using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class PaymentGatewayService : BaseService, IPaymentGatewayService
    {
        private readonly IPaymentGatewayRepository? repository = null;

        public PaymentGatewayService(IPaymentGatewayRepository repo) : base()
        {
            repository = repo;
        }

        public async Task<MVPaymentGateway> AddPaymentGateway(string orgId, MPaymentGateway paymentGateway)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPaymentGateway()
            {
                Status = "OK",
                Description = "Success",
            };

            if (string.IsNullOrEmpty(paymentGateway.Code))
            {
                r.Status = "CODE_MISSING";
                r.Description = "Payment Gateway code is missing!!!";

                return r;
            }

            if (paymentGateway.Scheme != "PleasePayment")
            {
                r.Status = "SCHEME_INVALID";
                r.Description = "Scheme must be 'PleasePayment'!!!";

                return r;
            }

            var existing = await repository!.GetPaymentGatewayByCode(paymentGateway.Code);
            if (existing != null)
            {
                r.Status = "CODE_DUPLICATE";
                r.Description = $"Payment Gateway code [{paymentGateway.Code}] already exist!!!";

                return r;
            }

            var result = await repository!.AddPaymentGateway(paymentGateway);

            r.PaymentGateway = result;
            return r;
        }

        public async Task<MVPaymentGateway> DeletePaymentGatewayById(string orgId, string paymentGatewayId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPaymentGateway()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(paymentGatewayId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Payment Gateway ID [{paymentGatewayId}] format is invalid";

                return r;
            }

            var result = await repository!.DeletePaymentGatewayById(paymentGatewayId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Gateway ID [{paymentGatewayId}] not found for the organization [{orgId}]";

                return r;
            }

            r.PaymentGateway = result;
            return r;
        }

        public async Task<IEnumerable<MPaymentGateway>> GetPaymentGateways(string orgId, VMPaymentGateway param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPaymentGateways(param);

            foreach (var pg in result)
            {
                pg.ApiKey = ""; //ไม่ต้อง return secret กลับไปตอนแสดง list
            }

            return result;
        }

        public async Task<int> GetPaymentGatewayCount(string orgId, VMPaymentGateway param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPaymentGatewayCount(param);

            return result;
        }

        public async Task<MVPaymentGateway> GetPaymentGatewayById(string orgId, string paymentGatewayId)
        {
            var r = new MVPaymentGateway()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(paymentGatewayId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Payment Gateway ID [{paymentGatewayId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPaymentGatewayById(paymentGatewayId);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Gateway ID [{paymentGatewayId}] not found for the organization [{orgId}]";

                return r;
            }

            r.PaymentGateway = result;
            return r;
        }

        public async Task<MVPaymentGateway> UpdatePaymentGatewayById(string orgId, string paymentGatewayId, MPaymentGateway paymentGateway)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPaymentGateway()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(paymentGatewayId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Payment Gateway ID [{paymentGatewayId}] format is invalid";

                return r;
            }

            if (string.IsNullOrEmpty(paymentGateway.Code))
            {
                r.Status = "CODE_MISSING";
                r.Description = "Payment Gateway code is missing!!!";

                return r;
            }

            var existingByCode = await repository!.GetPaymentGatewayByCode(paymentGateway.Code);
            if ((existingByCode != null) && (existingByCode.Id.ToString() != paymentGatewayId))
            {
                r.Status = "CODE_DUPLICATE";
                r.Description = $"Payment Gateway code [{paymentGateway.Code}] already exist!!!";

                return r;
            }

            var result = await repository!.UpdatePaymentGatewayById(paymentGatewayId, paymentGateway);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Gateway ID [{paymentGatewayId}] not found for the organization [{orgId}]";

                return r;
            }

            r.PaymentGateway = result;
            return r;
        }
    }
}
