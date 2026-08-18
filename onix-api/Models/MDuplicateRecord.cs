using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("DuplicateRecords")]
    [Microsoft.EntityFrameworkCore.Index(nameof(First4), nameof(Last4))]
    public class MDuplicateRecord
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("dup_type")]
        public string? DupType { get; set; }

        [Column("first4")]
        public string? First4 { get; set; }

        [Column("last4")]
        public string? Last4 { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("document_id")]
        public string? DocumentId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        public MDuplicateRecord()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }
    }
}
