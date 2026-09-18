using Its.Onix.Api.Models;

namespace Its.Onix.Api.Services
{
    public class BankSummary
    {
        public decimal TotalPayInAmount { get; set; }
        public decimal TotalPayOutAmount { get; set; }
        public decimal TotalWithdrawalAmount { get; set; }
        public int TotalPayInCount { get; set; }
        public int TotalPayOutCount { get; set; }
        public int TotalWithdrawalCount { get; set; }
        public List<DailyBankSummaryData> DailyBankSummary { get; set; }

        public BankSummary()
        {
            DailyBankSummary = [];
        }
    }
}
