using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("PointsTriggers")]

    [Index(nameof(OrgId))]
    [Index(nameof(TriggerName))]

    public class MPointTrigger
    {
        [Key]
        [Column("trigger_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("wallet_id")]
        public string? WalletId { get; set; }

        [Column("trigger_name")]
        public string? TriggerName { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("triggered_event")]
        public string? TriggeredEvent { get; set; } /* CustomerRegistered */

        [Column("trigger_date")]
        public DateTime? TriggerDate { get; set; } /* ต้องส่ง time ให้เป็น 00:00:00 เอาเองนะ */

        [Column("points")]
        public int? Points { get; set; }

        [Column("trigger_params")]
        public string? TriggerParams { get; set; } /* เป็น JSON string เก็บ input ส่งให้กับ PointRule */


        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        public MPointTrigger()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
