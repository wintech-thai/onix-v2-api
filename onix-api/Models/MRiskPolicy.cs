using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("RiskPolicies")]

    [Index(nameof(OrgId))]
    [Index(nameof(Name))]
    [Index(nameof(Status))]

    public class MRiskPolicy
    {
        [Key]
        [Column("risk_policy_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("tags")]
        public string? Tags { get; set; } /* Comma separated string */

        [Column("status")]
        public string? Status { get; set; } /* Active, Disabled */

        //Pay-In rules
        [Column("allow_blank_payer_name")]
        public bool AllowBlankPayerName { get; set; }

        [Column("allow_unknown_payer_name")]
        public bool AllowUnknownPayerName { get; set; } /* payer name ที่ไม่เคยพบใน IoC เลย */

        [Column("allow_suspicious_payer_name")]
        public bool AllowSuspiciousPayerName { get; set; } /* payer name ที่ตรงกับ IoC ที่มี Reputation = Suspicious */

        [Column("allow_malicious_payer_name")]
        public bool AllowMaliciousPayerName { get; set; } /* payer name ที่ตรงกับ IoC ที่มี Reputation = Malicious */

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        public MRiskPolicy()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
