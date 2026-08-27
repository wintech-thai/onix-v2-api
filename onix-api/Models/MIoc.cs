using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Iocs")]

    [Index(nameof(OrgId))]
    [Index(nameof(IocType))]
    [Index(nameof(IocValue))]

    public class MIoc
    {
        [Key]
        [Column("oic_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("ioc_type")]
        public string? IocType { get; set; } /* PayerName, PayerEmail */

        [Column("ioc_value")]
        public string? IocValue { get; set; }

        [Column("status")]
        public string? Status { get; set; } /* Disabled, Active */

        [Column("source")]
        public string? Source { get; set; } /* แหล่งที่มาของข้อมูล เช่น internet */

        [Column("risk_score")]
        public int RiskScore { get; set; } /* 0-100 */

        [Column("confidence_score")]
        public int ConfidenceScore { get; set; } /* 0-100 */

        [Column("reputation")]
        public string? Reputation { get; set; } /* Unknown, Neutral, Trusted, Suspicious, Malicious */


        [Column("note")]
        public string? Noted { get; set; } /* หมายเหตุคำอธิบายต่าง ๆ */

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("seen_count")]
        public int SeenCount { get; set; }

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("last_seen_date")]
        public DateTime? LastSeenDate { get; set; }

        [Column("first_seen_date")]
        public DateTime? FirstSeenDate { get; set; }


        public MIoc()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            SeenCount = 0;
            RiskScore = 0;
        }
    }
}
