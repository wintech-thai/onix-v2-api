using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IPaymentGatewayRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<MPaymentGateway?> GetPaymentGatewayByCode(string code);
        public MPaymentGateway AddPaymentGateway(MPaymentGateway paymentGateway);
        public MPaymentGateway? DeletePaymentGatewayById(string paymentGatewayId);
        public IEnumerable<MPaymentGateway> GetPaymentGateways(VMPaymentGateway param);
        public Task<MPaymentGateway?> GetPaymentGatewayById(string paymentGatewayId);
        public int GetPaymentGatewayCount(VMPaymentGateway param);
        public MPaymentGateway? UpdatePaymentGatewayById(string paymentGatewayId, MPaymentGateway paymentGateway);
    }
}
