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
        public string? ProductId { get; set; } /* MasterRef(RefType=Service).Id หรือ InventoryItem.Id แล้วแต่ ProductType, ว่างได้ถ้า Custom */

        [Column("product_type")]
        public string? ProductType { get; set; } /* Service, InventoryItem, Custom */

        // คัดลอกมาเก็บตอนบันทึก (ไม่ join สดเพราะแถวแบบ Custom ไม่มี master ให้ join อยู่แล้ว)
        [Column("product_code")]
        public string? ProductCode { get; set; }

        [Column("product_desc")]
        public string? ProductDesc { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("quantity")]
        public double? Quantity { get; set; }

        [Column("unit_price")]
        public double? UnitPrice { get; set; }

        [Column("total_price")]
        public double? TotalPrice { get; set; } /* quantity * unit_price ก่อนหักส่วนลด */

        [Column("item_discount_type")]
        public string? ItemDiscountType { get; set; } /* Percent, Amount */

        [Column("item_discount_value")]
        public double? ItemDiscountValue { get; set; } /* ค่าที่กรอกดิบ ๆ (% หรือจำนวนเงิน ตาม ItemDiscountType) */

        [Column("item_discount_amount")]
        public double? ItemDiscountAmount { get; set; } /* คำนวณเป็นจำนวนเงินเสมอ */

        [Column("net_amount")]
        public double? NetAmount { get; set; } /* TotalPrice - ItemDiscountAmount, ฐานคำนวณ VAT/หัก ณ ที่จ่าย */

        [Column("vat_enabled")]
        public bool? VatEnabled { get; set; }

        [Column("vat_amount")]
        public double? VatAmount { get; set; } /* NetAmount * (AccountDoc.VatRate / 100), ถ้า VatEnabled */

        [Column("withholding_tax_enabled")]
        public bool? WithholdingTaxEnabled { get; set; }

        [Column("withholding_tax_rate")]
        public double? WithholdingTaxRate { get; set; } /* default 3 */

        [Column("withholding_tax_amount")]
        public double? WithholdingTaxAmount { get; set; } /* NetAmount * (WithholdingTaxRate / 100), ถ้า WithholdingTaxEnabled */

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
