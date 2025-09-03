using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("ScanItems")]

    [Index(nameof(OrgId))]
    public class MScanItem
    {
        [Key]
        [Column("scan_item_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("serial")]
        public string? Serial { get; set; }

        [Column("pin")]
        public string? Pin { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("product_code")]
        public string? ProductCode { get; set; }

        [Column("sequence_no")]
        public string? SequenceNo { get; set; }

        [Column("url")]
        public string? Url { get; set; }

        [Column("run_id")]
        public string? RunId { get; set; }

        [Column("uploaded_path")]
        public string? UploadedPath { get; set; }

        [Column("item_group")]
        public string? ItemGroup { get; set; }

        [Column("registered_flag")]
        public string? RegisteredFlag { get; set; } /* YES or NO */

        [Column("used_flag")]
        public string? UsedFlag { get; set; } /* TRUE or FALSE */


        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("registered_date")]
        public DateTime? RegisteredDate { get; set; }

        public MScanItem()
        {
            Id = Guid.NewGuid();
            UsedFlag = "FALSE";
            CreatedDate = DateTime.UtcNow;
        }
    }
}
