using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Jobs")]

    [Index(nameof(OrgId))]
    [Index(nameof(Type))]
    [Index(nameof(Status))]
    public class MJob
    {
        [Key]
        [Column("job_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("status")] /* Pending, Submitted, Running, Success, Failed */
        public string? Status { get; set; }

        [Column("job_message")]
        public string? JobMessage { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("type")]
        public string? Type { get; set; } /* ScanItemGenerator */

        [Column("progress_pct")]
        public int? ProgressPct { get; set; } /* Progress percentage */

        [Column("configuration")]
        public string? Configuration { get; set; } /* Environment variables passed to job */

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        [Column("pickup_date")]
        public DateTime? PickupDate { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [NotMapped]
        public NameValue[] Parameters { get; set; }

        public MJob()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            Parameters = Array.Empty<NameValue>();
        }
    }
}
