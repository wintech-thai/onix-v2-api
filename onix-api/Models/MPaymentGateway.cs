using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("PaymentGateways")]
    [Index(nameof(OrgId))]
    public class MPaymentGateway
    {
        [Key]
        [Column("payment_gateway_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; } //มีค่าเป็น "global" เสมอ

        [Column("scheme")]
        public string? Scheme { get; set; } //(PleasePayment) กำหนดได้แค่ตอน Add เท่านั้น แก้ไขทีหลังไม่ได้

        [Column("code")]
        public string? Code { get; set; } //unique ภายใน OrgId เดียวกัน

        [Column("description")]
        public string? Description { get; set; }

        [Column("endpoint_base_url")]
        public string? EndpointBaseUrl { get; set; } //เช่น https://api-dev.please-payment.com

        [Column("endpoint_org_id")]
        public string? EndpointOrgId { get; set; }

        [Column("endpoint_merchant_id")]
        public string? EndpointMerchantId { get; set; }

        [Column("api_key")]
        public string? ApiKey { get; set; } //secret

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        public MPaymentGateway()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
