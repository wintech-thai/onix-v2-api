using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("AccountDocs")]

    [Index(nameof(OrgId))]
    [Index(nameof(Code))]
    [Index(nameof(DocumentType))]
    [Index(nameof(ProductType))]
    [Index(nameof(EntityId))]

    public class MAccountDoc
    {
        [Key]
        [Column("doc_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("code")]
        public string? Code { get; set; }

        //x011 : เลขที่เอกสารจริงที่ auto-gen จาก IDocumentNumberService ตอนสร้าง (ดูไอเดียจาก MInventoryDoc.DocumentNo)
        [Column("document_no")]
        public string? DocumentNo { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("entity_id")]
        public string? EntityId { get; set; } /* Customer Id */

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("document_type")] /* CashInvoice, CreditInvoice */
        public string? DocumentType { get; set; }

        [Column("product_type")] /* Privilege, Item */
        public string? ProductType { get; set; }

        [Column("document_date")]
        public DateTime? DocumentDate { get; set; }

        [Column("due_date")]
        public DateTime? DueDate { get; set; } /* ขายเชื่อ: วันครบกำหนดชำระ = document_date + credit term ของลูกค้า */

        [Column("total_price")]
        public double? TotalPrice { get; set; } /* ยอดรวมทุกรายการ ก่อนส่วนลด ก่อน vat */

        [Column("payment_method")]
        public string? PaymentMethod { get; set; } /* ขายสด: Cash, Transfer, QR ฯลฯ */

        [Column("paid_amount")]
        public double? PaidAmount { get; set; } /* ขายสด: จำนวนเงินที่รับชำระ */

        [Column("status")] /* Pending, Approved, Cancelled */
        public string? Status { get; set; }

        [Column("document_params")]
        public string? DocumentParams { get; set; } /* เป็น JSON string บอกว่า price calculate อย่างไร */

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        [Column("approved_date")]
        public DateTime? ApprovedDate { get; set; }

        [Column("status_date")]
        public DateTime? StatusDate { get; set; }

        // Carries line items for Add/Update requests and GetById responses — never persisted on
        // this table itself, see MAccountDocItem/AccountDocItems for the real storage (same
        // pattern as MInventoryDoc.Items).
        [NotMapped]
        public List<MAccountDocItem>? Items { get; set; }

        //Populated by the repository at read time (joined from Entities), never persisted here
        [NotMapped]
        public string? EntityCode { get; set; }
        [NotMapped]
        public string? EntityName { get; set; }

        public MAccountDoc()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            UpdatedDate = DateTime.UtcNow;
        }
    }
}
