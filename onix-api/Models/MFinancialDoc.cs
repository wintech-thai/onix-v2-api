using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("FinancialDocs")]

    [Index(nameof(OrgId))]
    public class MFinancialDoc
    {
        [Key]
        [Column("financial_doc_id")]
        public Guid? Id { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("document_no")]
        public string? DocumentNo { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("from_date")]
        public DateTime FromDate { get; set; }

        [Column("to_date")]
        public DateTime ToDate { get; set; }

        [Column("expense_items_def")]
        public string? ExpenseItemsDefinition { get; set; } /* JSON string เก็บรายการฝั่ง expense */

        [Column("revenue_items_def")]
        public string? RevenueItemsDefinition { get; set; } /* JSON string เก็บรายการฝั่ง revenue */

        [Column("sharing_items_def")]
        public string? SharingItemsDefinition { get; set; } /* JSON string เก็บรายการฝั่งสัดส่วนผู้ถือหุ้น */


        [Column("total_revenue")]
        public decimal? TotalRevenue { get; set; }

        [Column("total_expense")]
        public decimal? TotalExpense { get; set; }

        [Column("profit_loss")]
        public decimal? ProfitLoss { get; set; } /* ถ้าติดลบ หมายถึงขาดทุน */


        //System fields
        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }


        [NotMapped]
        public List<MFinancialDocItem> ExpenseItemsArr { get; set; }

        [NotMapped]
        public List<MFinancialDocItem> RevenueItemsArr { get; set; }

        [NotMapped]
        public List<MFinancialDocItem> SharingItemsArr { get; set; }

        public MFinancialDoc()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            ExpenseItemsArr = [];
            RevenueItemsArr = [];
            SharingItemsArr = [];
        }
    }
}
