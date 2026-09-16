using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("InventoryDocItems")]

    [Index(nameof(OrgId))]
    [Index(nameof(DocumentNo))]
    [Index(nameof(LotId))]

    public class MInventoryDocItem
    {
        [Key]
        [Column("inventory_document_item_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("document_id")]
        public string? DocumentId { get; set; } /* UUID ไปยัง InventoryDocument */

        [Column("document_no")]
        public string? DocumentNo { get; set; }

        [Column("document_type")]
        public string? DocumentType { get; set; } /* StockIn, StockOut, StockTransfer */

        [Column("note")]
        public string? Note { get; set; }

        [Column("lot_id")]
        public string? LotId { get; set; }


        // ให้ copy มาจาก InventoryDocument เพื่อให้สามารถใช้ข้อมูลของ Location ได้โดยไม่ต้อง join table
        [Column("from_location_id")]
        public string? FromLocationId { get; set; } /* UUID ไปยัง InventoryLocation */

        [Column("from_location_code")]
        public string? FromLocationCode { get; set; }

        [Column("from_location_name")]
        public string? FromLocationName { get; set; }

        [Column("from_location_previous_quantity")]
        public decimal? FromLocationPreviousQuantity { get; set; }

        [Column("from_location_previous_amount")]
        public decimal? FromLocationPreviousAmount { get; set; }

        [Column("from_location_previous_unit_price")]
        public decimal? FromLocationPreviousUnitPrice { get; set; }

        [Column("from_location_current_quantity")]
        public decimal? FromLocationCurrentQuantity { get; set; }

        [Column("from_location_current_amount")]
        public decimal? FromLocationCurrentAmount { get; set; }

        [Column("from_location_current_unit_price")]
        public decimal? FromLocationCurrentUnitPrice { get; set; }



        // ให้ copy มาจาก InventoryDocument เพื่อให้สามารถใช้ข้อมูลของ Location ได้โดยไม่ต้อง join table
        [Column("to_location_id")]
        public string? ToLocationId { get; set; } /* UUID ไปยัง InventoryLocation */

        [Column("to_location_code")]
        public string? ToLocationCode { get; set; }

        [Column("to_location_name")]
        public string? ToLocationName { get; set; }

        [Column("to_location_previous_quantity")]
        public decimal? ToLocationPreviousQuantity { get; set; }

        [Column("to_location_previous_amount")]
        public decimal? ToLocationPreviousAmount { get; set; }

        [Column("to_location_previous_unit_price")]
        public decimal? ToLocationPreviousUnitPrice { get; set; }

        [Column("to_location_current_quantity")]
        public decimal? ToLocationCurrentQuantity { get; set; }

        [Column("to_location_current_amount")]
        public decimal? ToLocationCurrentAmount { get; set; }

        [Column("to_location_current_unit_price")]
        public decimal? ToLocationCurrentUnitPrice { get; set; }


        //Item
        [Column("item_id")]
        public string? ItemId { get; set; } /* UUID ไปยัง InventoryItem */

        [Column("item_code")]
        public string? ItemCode { get; set; } 

        [Column("item_name")]
        public string? ItemName { get; set; }

        [Column("item_quantity")]
        public decimal? ItemQuantity { get; set; }

        [Column("item_amount")]
        public decimal? ItemAmount { get; set; }

        [Column("item_unit_price")]
        public decimal? ItemUnitPrice { get; set; }


        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }


        public MInventoryDocItem()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
