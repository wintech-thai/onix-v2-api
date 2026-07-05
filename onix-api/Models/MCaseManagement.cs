using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("CaseManagements")]
    [Index(nameof(OrgId))]
    [Index(nameof(Ref))]
    [Index(nameof(Status))]
    public class MCaseManagement : IOrgEntity
    {
        [Key]
        [Column("case_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("ref")]
        public string? Ref { get; set; }

        [Column("subject")]
        public string? Subject { get; set; }

        [Column("priority")]
        public string? Priority { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("created_by")]
        public string? CreatedBy { get; set; }

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("updated_by")]
        public string? UpdatedBy { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        [Column("closed_by")]
        public string? ClosedBy { get; set; }

        [Column("closed_date")]
        public DateTime? ClosedDate { get; set; }

        public MCaseManagement()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            Status = "New";
        }
    }
}
