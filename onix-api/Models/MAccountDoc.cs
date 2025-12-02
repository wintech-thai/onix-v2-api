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

        [Column("description")]
        public string? Description { get; set; }

        [Column("entity_id")]
        public string? EntityId { get; set; } /* Customer Id */

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("document_type")] /* CashInvoice */
        public string? DocumentType { get; set; }

        [Column("product_type")] /* Privilege, Item */
        public string? ProductType { get; set; }

        [Column("document_date")]
        public DateTime? DocumentDate { get; set; }

        [Column("total_price")]
        public double? TotalPrice { get; set; } /* ยอดรวมทุกรายการ ก่อนส่วนลด ก่อน vat */

        [Column("status")] /* Pending, Approved */
        public string? Status { get; set; }

        [Column("document_params")]
        public string? DocumentParams { get; set; } /* เป็น JSON string บอกว่า price calculate อย่างไร */

        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        public MAccountDoc()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            UpdatedDate = DateTime.UtcNow;
        }
    }
}
