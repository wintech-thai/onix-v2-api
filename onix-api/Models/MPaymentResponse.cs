using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class MPaymentResponse
    {
        public string? Id { get; set; } //UUID
        public string? ReferenceId { get; set; } //คำอธิบายการชำระเงิน
        public string? Type { get; set; } //PayIn
        public string? Status { get; set; } //PayIn
        public double? RequestedAmount { get; set; } //จำนวนเงิน > 0
        public double? GeneratedAmount { get; set; } //จำนวนเงิน > 0
        public string? Currency { get; set; }
        public string? QrCode { get; set; }
        public string? QrCodeImage { get; set; }
        public string? PaymentUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ExpireAt { get; set; }

        public MPaymentResponse()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
