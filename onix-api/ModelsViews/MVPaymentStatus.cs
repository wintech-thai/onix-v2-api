using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVPaymentStatus
    {
        public string? Status { get; set; }
        public string? Description { get; set; }

        public string? PaymentRequestId { get; set; }
        public string? PaymentStatus { get; set; } //Pending, Approved, Paid, Rejected - สถานะจริงของ payment request
        public string? RefId1 { get; set; }
        public string? RefId2 { get; set; }
        public string? RefId3 { get; set; }
        public string? PayerName { get; set; }
        public double? Amount { get; set; }
        public string? Currency { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ExpireAt { get; set; }

        //QrCode เป็น payload เดิมที่เคย generate ไว้ตอนสร้าง payment request (ไม่ re-generate ใหม่)
        //ฝั่ง client ต้อง render เป็นรูป QR เอง (เหมือนที่ทำตอนแสดงผลครั้งแรก)
        public string? QrCode { get; set; }
        public bool? IsQrAvailable { get; set; }

        public string? PayInBankAccountName { get; set; }
        public string? PayInBankAccountNo { get; set; }
        public string? PayInBankCode { get; set; }
        public string? PayInPromptPayId { get; set; }

        public string? MerchantName { get; set; }

        public string? SlipUploadUrl { get; set; }
    }
}
