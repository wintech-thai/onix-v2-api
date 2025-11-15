using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("PointsRules")]

    [Index(nameof(OrgId))]

    public class MPointRule
    {
        [Key]
        [Column("rule_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("rule_name")]
        public string? RuleName { get; set; }

        [Column("rule_definition")]
        public string? RuleDefinition { get; set; } /* เป็น YAML เอาไปรันกับ RuleEngine */

        [Column("description")]
        public string? Description { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("triggered_event")]
        public string? TriggeredEvent { get; set; } /* CustomerRegistered */

        [Column("status")]
        public string? Status { get; set; } /* Disable, Active */

        [Column("points_return")]
        public long? PointsReturn { get; set; }

        [Column("priority")]
        public int? Priority { get; set; } /* เอาไว้ sorint ว่าจะ execute function ไหนก่อน */


        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        public MPointRule()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
