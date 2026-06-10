using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("AgentEvents")]

    [Index(nameof(OrgId))]
    [Index(nameof(AgentId))]

    public class MAgentEvent
    {
        [Key]
        [Column("event_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("agent_id")]
        public string? AgentId { get; set; }

        [Column("event_type")]
        public string? EventType { get; set; } /* Heartbeat, PaymentTx */

        [Column("channel")]
        public string? Channel { get; set; } /* SMS, LINE */

        [Column("raw_data")]
        public string? RawData { get; set; } /* JSON string */

        [Column("tags")]
        public string? Tags { get; set; } /* metadata here */


        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }


        [NotMapped]
        public JsonElement RawDataObj { get; set; }

        public MAgentEvent()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
