using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("CaseManagementComments")]
    [Index(nameof(CaseId))]
    [Index(nameof(OrgId))]
    public class MCaseManagementComment : IOrgEntity
    {
        [Key]
        [Column("comment_id")]
        public Guid? Id { get; set; }

        [Column("case_id")]
        public Guid? CaseId { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("content")]
        public string? Content { get; set; }

        [Column("author_type")]
        public string? AuthorType { get; set; }

        [Column("created_by")]
        public string? CreatedBy { get; set; }

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("updated_by")]
        public string? UpdatedBy { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        public MCaseManagementComment()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
