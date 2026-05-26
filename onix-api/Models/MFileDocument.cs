using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("FileDocuments")]

    [Index(nameof(OrgId))]
    public class MFileDocument
    {
        [Key]
        [Column("file_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("object_storage_path")]
        public string? ObjectStoragePath { get; set; } // Path in object storage

        [Column("document_type")]
        public string? DocumentType { get; set; } // Type of the document - Logo, PayInSlip, PayOutSlip

        [Column("mime_type")]
        public string? MimeType { get; set; } // MIME type of the document

        [Column("public_document_url")]
        public string? PublicDocumentUrl { get; set; }

        [Column("is_public")]
        public bool IsPublic { get; set; }


        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }


        [NotMapped]
        public string? DocumentUrl { get; set; }

        public MFileDocument()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            IsPublic = false;
        }
    }
}
