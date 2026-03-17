using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Agents")]

    [Index(nameof(OrgId))]
    public class MAgent
    {
        [Key]
        [Column("agent_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("code")]
        public string? Code { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("api_key_id")]
        public string ApiKeyId { get; set; }

        [Column("registration_url")]
        public string? RegistrationUrl { get; set; }

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("last_seen_date")]
        public DateTime? LastSeenDate { get; set; }

        [NotMapped]
        public string? ApiKey { get; set; }

        public MAgent()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            ApiKeyId = "";
        }
    }
}
