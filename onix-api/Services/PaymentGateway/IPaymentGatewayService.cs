using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IPaymentGatewayService
    {
        public MVPaymentGateway AddPaymentGateway(string orgId, MPaymentGateway paymentGateway);
        public MVPaymentGateway DeletePaymentGatewayById(string orgId, string paymentGatewayId);
        public IEnumerable<MPaymentGateway> GetPaymentGateways(string orgId, VMPaymentGateway param);
        public int GetPaymentGatewayCount(string orgId, VMPaymentGateway param);
        public Task<MVPaymentGateway> GetPaymentGatewayById(string orgId, string paymentGatewayId);
        public Task<MVPaymentGateway> UpdatePaymentGatewayById(string orgId, string paymentGatewayId, MPaymentGateway paymentGateway);
    }
}
