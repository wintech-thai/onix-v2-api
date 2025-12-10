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

        [Column("scan_count")]
        public int? ScanCount { get; set; }


        [Column("used_flag")]
        public string? UsedFlag { get; set; } /* YES or NO */
        [Column("item_id")]
        public Guid? ItemId { get; set; }

        [Column("applied_flag")]
        public string? AppliedFlag { get; set; } /* TRUE or FALSE */
        [Column("customer_id")]
        public Guid? CustomerId { get; set; }

        [Column("folder_id")]
        public Guid? FolderId { get; set; }

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("registered_date")]
        public DateTime? RegisteredDate { get; set; }


        [NotMapped]
        public string? ScanItemActionId { get; set; } /* ใช้บอกว่าไป match กับ ScanItemAction ตัวไหน */

        [NotMapped]
        public string? ScanItemActionName { get; set; }
        [NotMapped]
        public string? ProductDesc { get; set; }
        [NotMapped]
        public string? CustomerEmail { get; set; }
        [NotMapped]
        public string? FolderName { get; set; }

        [NotMapped]
        public string? ProductDescLegacy { get; set; }
        [NotMapped]
        public string? ProductCodeLegacy { get; set; }


        public MScanItem()
        {
            Id = Guid.NewGuid();
            UsedFlag = "FALSE";
            CreatedDate = DateTime.UtcNow;
        }
    }
}
