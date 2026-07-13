using Its.Onix.Api.Models;

namespace Its.Onix.Api.Services
{
    public class ExpenseSummary
    {
        public decimal TotalAmount { get; set; }
        public int TotalCount { get; set; }
        public List<DailyExpenseSummaryData> DailyExpense { get; set; }
        public List<ExpenseByCategoryData> ExpenseByCategory { get; set; }

        public ExpenseSummary()
        {
            DailyExpense = [];
            ExpenseByCategory = [];
        }
    }
}
