using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("financial_doc_item_expenses")]
    [Index(nameof(FinancialDocId))]
    [Index(nameof(OrgId))]
    public class MFinancialDocItemExpense
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("financial_doc_id")]
        public Guid FinancialDocId { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("expense_date")]
        public DateTime? ExpenseDate { get; set; }

        [Column("expense_code")]
        public string? ExpenseCode { get; set; }

        [Column("expense_desc")]
        public string? ExpenseDesc { get; set; }

        [Column("amount", TypeName = "decimal(18,4)")]
        public decimal? Amount { get; set; }

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        public MFinancialDocItemExpense()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
