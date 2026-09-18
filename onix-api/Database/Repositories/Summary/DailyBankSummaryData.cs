using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class DailyBankSummaryData
    {
        public DateTime? Date { get; set; }
        public string? BankCode { get; set; }
        public string? AccountNumber { get; set; }
        public string? MerchantCode { get; set; }
        public decimal PayInAmount { get; set; }
        public decimal PayOutAmount { get; set; }
        public decimal WithdrawalAmount { get; set; }
        public int PayInCount { get; set; }
        public int PayOutCount { get; set; }
        public int WithdrawalCount { get; set; }

        public DailyBankSummaryData()
        {
        }
    }
}
