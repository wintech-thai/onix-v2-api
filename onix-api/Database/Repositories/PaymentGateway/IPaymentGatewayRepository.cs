using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IPaymentGatewayRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<MPaymentGateway?> GetPaymentGatewayByCode(string code);
        public Task<MPaymentGateway> AddPaymentGateway(MPaymentGateway paymentGateway);
        public Task<MPaymentGateway?> DeletePaymentGatewayById(string paymentGatewayId);
        public Task<IEnumerable<MPaymentGateway>> GetPaymentGateways(VMPaymentGateway param);
        public Task<MPaymentGateway?> GetPaymentGatewayById(string paymentGatewayId);
        public Task<int> GetPaymentGatewayCount(VMPaymentGateway param);
        public Task<MPaymentGateway?> UpdatePaymentGatewayById(string paymentGatewayId, MPaymentGateway paymentGateway);
    }
}
