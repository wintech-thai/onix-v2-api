using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Merchants")]

    [Index(nameof(OrgId))]
    public class MMerchant
    {
        [Key]
        [Column("merchant_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("code")]
        public string? Code { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("contact_email")]
        public string? ContactEmail { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("contact_phone")]
        public string? ContactPhone { get; set; }

        [Column("payin_fee_pct")]
        public double? PayinFeePct { get; set; }

        [Column("payout_fee_pct")]
        public double? PayoutFeePct { get; set; }

        [Column("payin_min_amount")]
        public double? PayinMinAmount { get; set; }

        [Column("payin_max_amount")]
        public double? PayinMaxAmount { get; set; }

        [Column("payout_min_amount")]
        public double? PayoutMinAmount { get; set; }

        [Column("payout_max_amount")]
        public double? PayoutMaxAmount { get; set; }

        [Column("Status")]
        public string? Status { get; set; } //Active, Pending, Disabled

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        public MMerchant()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
