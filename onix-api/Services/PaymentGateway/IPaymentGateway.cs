namespace Its.Onix.Api.Services
{
    // Interface เดียวกันทุก Scheme, implementation ต่างกันไปตาม Scheme (factory pattern เลือกจาก PaymentGatewayService)
    public interface IPaymentGateway
    {
        Task<PaymentGatewayQrResult> GenerateQR(PaymentGatewayQrRequest request);
    }
}
