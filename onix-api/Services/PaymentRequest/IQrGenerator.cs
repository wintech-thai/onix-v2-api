using Its.Onix.Api.Models;

namespace Its.Onix.Api.Services
{
    public interface IQrGenerator
    {
        public string GenerateQrBarcode(MPaymentRequest paymentRequest);
        public string GenerateQrImage(MPaymentRequest paymentRequest);
    }
}
