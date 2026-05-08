using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("AgentPolicies")]

    [Index(nameof(OrgId))]
    public class MAgentPolicy
    {
        [Key]
        [Column("policy_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("code")]
        public string? Code { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("policy_definition")]
        public string? PolicyDefinition { get; set; } /* JSON string */


        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [NotMapped]
        public string? ApiKey { get; set; }

        public MAgentPolicy()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
