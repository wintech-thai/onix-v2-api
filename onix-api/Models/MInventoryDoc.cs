using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("InventoryDocs")]

    [Index(nameof(OrgId))]
    [Index(nameof(DocumentStatus))]
    [Index(nameof(DocumentNo))]
    [Index(nameof(Description))]

    public class MInventoryDoc
    {
        [Key]
        [Column("inventory_document_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("document_no")]
        public string? DocumentNo { get; set; }

        [Column("document_type")]
        public string? DocumentType { get; set; } /* StockIn, StockOut, StockTransfer */

        [Column("description")]
        public string? Description { get; set; }

        [Column("document_status")]
        public string? DocumentStatus { get; set; } /* Pending, Approved, Cancelled */


        // StockOut, StockTransfer จะใช้ field นี้เพื่อระบุว่าเอกสารนี้จะมีต้นทางจาก Location ไหน
        [Column("from_location_id")]
        public string? FromLocationId { get; set; } /* UUID ไปยัง InventoryLocation */

        [Column("from_location_code")]
        public string? FromLocationCode { get; set; }

        [Column("from_location_name")]
        public string? FromLocationName { get; set; }


        // StockIn, StockTransfer จะใช้ field นี้เพื่อระบุว่าเอกสารนี้จะมีปลายทางจาก Location ไหน
        [Column("to_location_id")]
        public string? ToLocationId { get; set; } /* UUID ไปยัง InventoryLocation */

        [Column("to_location_code")]
        public string? ToLocationCode { get; set; }

        [Column("to_location_name")]
        public string? ToLocationName { get; set; }


        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("approved_date")]
        public DateTime? ApprovedDate { get; set; }

        // Set whenever DocumentStatus transitions (Approved/Cancelled) so the search screen
        // can show "as of when" without guessing from ApprovedDate alone (e.g. Cancelled has none).
        [Column("status_date")]
        public DateTime? StatusDate { get; set; }

        // Carries the item rows for Add/Update requests and GetById responses — never persisted
        // on this table itself, see MInventoryDocItem/InventoryDocItems for the real storage.
        [NotMapped]
        public List<MInventoryDocItem>? Items { get; set; }


        public MInventoryDoc()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            ApprovedDate = DateTime.UtcNow;
        }
    }
}
