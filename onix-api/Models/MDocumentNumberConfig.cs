using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("DocumentNumberConfig")]

    [Index(nameof(OrgId))]
    public class MDocumentNumberConfig
    {
        [Key]
        [Column("document_number_config_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("document_type")]
        public string? DocumentType { get; set; } /* InventoryImport, InventoryExport */

        [Column("document_format")]
        public string? DocumentFormat { get; set; } /* เช่น IMP-${yyyy}-${mm}-${seq} */

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("seq_digit")]
        public int? SeqDigit { get; set; }

        [Column("reset_type")]
        public string? ResetType { get; set; } /* Monthly, Yearly */

        [Column("year_offset")]
        public int? YearOffset { get; set; } /* เช่น 543 */


        [Column("current_sequence_key")]
        public string? CurrentSequenceKey { get; set; } /* keyที่เก็บ เช่น 2026-01 */

        [Column("current_sequence_no")]
        public int? CurrentSequenceNo { get; set; } /* หมายเลขลำดับปัจจุบัน */

        [Column("last_document_number")]
        public string? LastDocumentNumber { get; set; } /* Doc No ล่าสุดที่ gen */

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }


        public MDocumentNumberConfig()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            CurrentSequenceNo = 0;
        }
    }
}
