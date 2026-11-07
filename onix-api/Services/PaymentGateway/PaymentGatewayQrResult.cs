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

        // ปรับแก้ ข้อ 1 : ไว้แสดงหน้า QR แบบ PromptPay (ชื่อบัญชี/เลขบัญชี/ธนาคาร/ยอดเงินจริงที่ฝังใน QR) + ข้อ 2 (link payment status / upload slip)
        public double? GeneratedAmount { get; set; }
        public string? PayInBankAccountNo { get; set; }
        public string? PayInBankAccountName { get; set; }
        public string? PayInBankCode { get; set; }
        public string? PayInPromptPayId { get; set; }
        public string? SlipUploadUrl { get; set; }
        public string? PaymentStatusUrl { get; set; }
        public DateTime? ExpireAt { get; set; }
        public string? WebsocketPath { get; set; }
        public string? SessionId { get; set; }
        // base URL ของ onix instance ที่ implementation นี้ยิง API ไปขอ QR (ไว้ให้ frontend ต่อ websocket ไปที่ instance เดียวกัน เพราะอาจคนละ domain กับของ merchant เอง)
        public string? GatewayBaseUrl { get; set; }
    }
}
