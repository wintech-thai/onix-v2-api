using Its.Onix.Api.Models;

namespace Its.Onix.Api.Services
{
    public class PayerSummary
    {
        public int TotalPayers { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalTransactionCount { get; set; }
        public List<PayerSummaryData> Payers { get; set; }

        public PayerSummary()
        {
            Payers = [];
        }
    }
}
