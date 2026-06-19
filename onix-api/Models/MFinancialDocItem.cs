using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class MFinancialDocItem
    {
        public string? Code { get; set; } /* ExpenseType code, shareholder name, or "PayInFee"/"PayOutFee" */
        public string? Label { get; set; } /* แสดงผลคำอธิบาย */
        public decimal? Amount { get; set; } /* จำนวนเงิน expense / revenue / commission */
        public decimal? Percent { get; set; } /* ใช้กับ sharing item เท่านั้น */
    }
}
