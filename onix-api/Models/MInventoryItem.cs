using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("InventoryItems")]

    [Index(nameof(OrgId))]
    [Index(nameof(ItemType))]
    public class MInventoryItem
    {
        [Key]
        [Column("inventory_item_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("code")]
        public string? Code { get; set; }

        [Column("reference_code")]
        public string? ReferenceCode { get; set; }

        [Column("name_th")]
        public string? NameTh { get; set; }

        [Column("name_en")]
        public string? NameEn { get; set; }

        [Column("item_type")]
        public string? ItemType { get; set; } /* Ref to MasterRef RefType=InventoryItemType */

        [Column("unit")]
        public string? Unit { get; set; } /* Ref to MasterRef RefType=InventoryItemUnit */

        [Column("item_group")]
        public string? ItemGroup { get; set; }

        [Column("remark")]
        public string? Remark { get; set; }

        [Column("minimum_quantity")]
        public decimal? MinimumQuantity { get; set; }

        [Column("price")]
        public decimal? Price { get; set; }

        [Column("is_vat_included")]
        public bool IsVatIncluded { get; set; }

        [Column("image_base64")]
        public string? ImageBase64 { get; set; }

        [NotMapped]
        public string? MimeType { get; set; }

        [NotMapped]
        public string? PreviewUrl { get; set; }

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        public MInventoryItem()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            UpdatedDate = DateTime.UtcNow;
        }
    }
}
