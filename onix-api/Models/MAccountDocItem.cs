using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("AccountDocItems")]

    [Index(nameof(OrgId))]
    [Index(nameof(AccountDocId))]
    [Index(nameof(ProductId))]

    public class MAccountDocItem
    {
        [Key]
        [Column("doc_item_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("account_doc_id")]
        public string? AccountDocId { get; set; }

        [Column("product_id")]
        public string? ProductId { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("quantity")]
        public double? Quantity { get; set; }

        [Column("unit_price")]
        public double? UnitPrice { get; set; }

        [Column("total_price")]
        public double? TotalPrice { get; set; }

        [Column("document_params")]
        public string? DocumentParams { get; set; } /* เป็น JSON string บอกว่า price calculate อย่างไร */

        [Column("incentive_rate")]
        public double? IncentiveRate { get; set; } /* เช่น อัตรารางวัลเมื่อถูกหวย */

        [Column("incentive_price")]
        public double? IncentiveTotalPrice { get; set; } /* เช่น รางวัลเมื่อถูกหวย */


        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        [NotMapped]
        public string? ProductCode { get; set; }
        [NotMapped]
        public string? ProductDesc { get; set; }
        [NotMapped]
        public string? CustomerCode { get; set; }
        [NotMapped]
        public string? Status { get; set; }

        public MAccountDocItem()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            UpdatedDate = DateTime.UtcNow;
        }
    }
}
