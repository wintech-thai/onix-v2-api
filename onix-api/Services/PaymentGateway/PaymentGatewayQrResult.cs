namespace Its.Onix.Api.Services
{
    public class PaymentGatewayQrResult
    {
        public string Status { get; set; } = "OK";
        public string Description { get; set; } = "Success";

        public string? QrCode { get; set; }
        public string? QrCodeImageBase64 { get; set; }
        public string? PaymentUrl { get; set; }
        public string? ExternalPaymentRequestId { get; set; }
    }
}
