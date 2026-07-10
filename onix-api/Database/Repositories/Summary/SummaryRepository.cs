using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class SummaryRepository : BaseRepository, ISummaryRepository
    {
        public SummaryRepository(IDataContext ctx)
        {
            context = ctx;
        }

        private ExpressionStarter<T> IsOrgMatchPredicate<T>() where T : IOrgEntity
        {
            var pd = PredicateBuilder.New<T>(true);
            if (orgId != "global")
            {
                //ต้องเอา orgId มา where ด้วย
                var orgPd = PredicateBuilder.New<T>(true);
                orgPd = orgPd.And(p => p.OrgId!.Equals(orgId));
                pd = pd.And(orgPd);
            }

            return pd;
        }

        public async Task<List<MerchantSummaryData>> GetMerchantCountByStatus(VMSummary param)
        {
            var result = await context!.Merchants!.AsExpandable()
                .Where(IsOrgMatchPredicate<MMerchant>())
                .GroupBy(x => x.Status)
                .Select(g => new MerchantSummaryData()
                {
                    MerchantStatus = g.Key,
                    MerchantCount = g.Count()
                }).ToListAsync();

            return result;
        }

        public async Task<List<MerchantSummaryData>> GetMerchantCount(VMSummary param)
        {
            var result = await context!.Merchants!.AsExpandable()
                .Where(IsOrgMatchPredicate<MMerchant>())
                .GroupBy(x => 1)
                .Select(g => new MerchantSummaryData()
                {
                    MerchantCount = g.Count()
                }).ToListAsync();

            return result;
        }

        public IQueryable<MWallet> GetSelectionWallet()
        {
            var query =
                from wllet in context!.Wallets

                join mc in context.Merchants!
                    on wllet.MerchantId equals mc.Id.ToString() into merchants
                from merchant in merchants.DefaultIfEmpty()

                select new { wllet, merchant };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MWallet
            {
                MerchantCode = x.merchant.Code,
                PointBalanceDecimal = x.wllet.PointBalanceDecimal,
            });
        }

        public async Task<List<MerchantSummaryData>> GetMerchantsBalance()
        {
            var result = await GetSelectionWallet().AsExpandable()
                .Where(IsOrgMatchPredicate<MWallet>())
                .Where(x => x.MerchantCode != null)
                .GroupBy(x => x.MerchantCode)
                .Select(g => new MerchantSummaryData()
                {
                    MerchantCode = g.Key,
                    BalanceAmount = g.Sum(x => x.PointBalanceDecimal)
                })
                .OrderByDescending(x => x.BalanceAmount)
                .ToListAsync();

            return result;
        }

        public IQueryable<MPaymentTransaction> GetSelectionPaymentTx()
        {
            var query =
                from pmt in context!.PaymentTransactions

                join mc in context.Merchants!
                    on pmt.MerchantId equals mc.Id.ToString() into merchants
                from merchant in merchants.DefaultIfEmpty()

                select new { pmt, merchant };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MPaymentTransaction
            {
                OrgId = x.pmt.OrgId,
                MerchantCode = x.merchant.Code,
                TxAmountDecimal = x.pmt.TxAmountDecimal,
                PayInFeeDecimal = x.pmt.PayInFeeDecimal,
                PayoutFeeDecimal = x.pmt.PayoutFeeDecimal,
                Direction = x.pmt.Direction,
                CreatedDate = x.pmt.CreatedDate,
            });
        }

        private ExpressionStarter<T> DateRangePredicate<T>(VMSummary param) where T : IOrgEntity
        {
            var pd = PredicateBuilder.New<T>(true);

            // FromDate
            if (param.FromDate.HasValue)
            {
                var fromDatePd = PredicateBuilder.New<T>(true);
                fromDatePd = fromDatePd.And(p => p.CreatedDate >= param.FromDate.Value);

                pd = pd.And(fromDatePd);
            }

            // ToDate
            if (param.ToDate.HasValue)
            {
                var toDatePd = PredicateBuilder.New<T>(true);
                toDatePd = toDatePd.And(p => p.CreatedDate <= param.ToDate.Value);

                pd = pd.And(toDatePd);
            }

            return pd;
        }

        public async Task<List<MerchantSummaryData>> GetMerchantsPayInAmountSummary(VMSummary param)
        {
            var result = await GetSelectionPaymentTx().AsExpandable()
                .Where(IsOrgMatchPredicate<MPaymentTransaction>())
                .Where(DateRangePredicate<MPaymentTransaction>(param))
                .Where(x => x.Direction == "PayIn")
                .Where(x => x.MerchantCode != null)
                .GroupBy(x => x.MerchantCode)
                .Select(g => new MerchantSummaryData()
                {
                    MerchantCode = g.Key,
                    TxAmount = g.Sum(x => x.TxAmountDecimal),
                    FeeAmount = g.Sum(x => x.PayInFeeDecimal)
                })
                .OrderByDescending(x => x.TxAmount)
                .ToListAsync();

            return result;
        }

        public async Task<List<MerchantSummaryData>> GetMerchantsPayOutAmountSummary(VMSummary param)
        {
            var result = await GetSelectionPaymentTx().AsExpandable()
                .Where(IsOrgMatchPredicate<MPaymentTransaction>())
                .Where(DateRangePredicate<MPaymentTransaction>(param))
                .Where(x => x.Direction == "PayOut")
                .Where(x => x.MerchantCode != null)
                .GroupBy(x => x.MerchantCode)
                .Select(g => new MerchantSummaryData()
                {
                    MerchantCode = g.Key,
                    TxAmount = g.Sum(x => x.TxAmountDecimal),
                    FeeAmount = g.Sum(x => x.PayoutFeeDecimal)
                })
                .OrderByDescending(x => x.TxAmount)
                .ToListAsync();

            return result;
        }

        public async Task<List<DailyRevenueSummaryData>> GetDailyRevenueSummary(VMSummary param)
        {
            var result = await GetSelectionPaymentTx().AsExpandable()
                .Where(IsOrgMatchPredicate<MPaymentTransaction>())
                .Where(DateRangePredicate<MPaymentTransaction>(param))
                .Where(x => x.MerchantCode != null)
                .GroupBy(x => x.CreatedDate!.Value.Date)
                .Select(g => new DailyRevenueSummaryData
                {
                    Date = g.Key,
                    PayInFee = g.Where(x => x.Direction == "PayIn").Sum(x => x.PayInFeeDecimal),
                    PayOutFee = g.Where(x => x.Direction == "PayOut").Sum(x => x.PayoutFeeDecimal)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return result;
        }

        public async Task<List<DailyMerchantRevenueSummaryData>> GetDailyMerchantRevenueSummary(VMSummary param)
        {
            var result = await GetSelectionPaymentTx().AsExpandable()
                .Where(IsOrgMatchPredicate<MPaymentTransaction>())
                .Where(DateRangePredicate<MPaymentTransaction>(param))
                .Where(x => x.MerchantCode != null)
                .GroupBy(x => new { x.CreatedDate!.Value.Date, x.MerchantCode })
                .Select(g => new DailyMerchantRevenueSummaryData
                {
                    Date = g.Key.Date,
                    MerchantCode = g.Key.MerchantCode,
                    PayInAmount = g.Where(x => x.Direction == "PayIn").Sum(x => x.TxAmountDecimal),
                    PayOutAmount = g.Where(x => x.Direction == "PayOut").Sum(x => x.TxAmountDecimal),
                    PayInFee = g.Where(x => x.Direction == "PayIn").Sum(x => x.PayInFeeDecimal),
                    PayOutFee = g.Where(x => x.Direction == "PayOut").Sum(x => x.PayoutFeeDecimal),
                    PayInCount = g.Count(x => x.Direction == "PayIn"),
                    PayOutCount = g.Count(x => x.Direction == "PayOut")
                })
                .OrderBy(x => x.Date)
                .ThenBy(x => x.MerchantCode)
                .ToListAsync();

            return result;
        }

        public async Task<List<RevenueSummaryData>> GetRevenueTotalSummary(VMSummary param)
        {
            var result = await GetSelectionPaymentTx().AsExpandable()
                .Where(IsOrgMatchPredicate<MPaymentTransaction>())
                .Where(DateRangePredicate<MPaymentTransaction>(param))
                .Where(x => x.MerchantCode != null)
                .GroupBy(x => x.Direction)
                .Select(g => new RevenueSummaryData()
                {
                    Direction = g.Key,
                    TxAmount = g.Sum(x => x.TxAmountDecimal),
                    FeeAmount = g.Sum(x => (x.PayInFeeDecimal ?? 0) + (x.PayoutFeeDecimal ?? 0)),
                    TxCount = g.Count()
                })
                .ToListAsync();

            return result;
        }

        private IQueryable<MFinancialDocItemExpense> GetExpenseBaseQuery(VMSummary param)
        {
            var query = context!.FinancialDocItemExpenses!.AsQueryable();

            if (orgId != "global")
                query = query.Where(x => x.OrgId == orgId);

            if (param.FromDate.HasValue)
                query = query.Where(x => x.ExpenseDate >= param.FromDate.Value);
            if (param.ToDate.HasValue)
                query = query.Where(x => x.ExpenseDate <= param.ToDate.Value);

            return query;
        }

        public async Task<decimal> GetExpenseTotalAmount(VMSummary param)
        {
            return await GetExpenseBaseQuery(param).SumAsync(x => x.Amount ?? 0);
        }

        public async Task<int> GetExpenseTotalCount(VMSummary param)
        {
            return await GetExpenseBaseQuery(param).CountAsync();
        }

        public async Task<List<DailyExpenseSummaryData>> GetDailyExpenseSummary(VMSummary param)
        {
            return await GetExpenseBaseQuery(param)
                .Where(x => x.ExpenseDate != null)
                .GroupBy(x => x.ExpenseDate!.Value.Date)
                .Select(g => new DailyExpenseSummaryData
                {
                    Date = g.Key,
                    Amount = g.Sum(x => x.Amount ?? 0),
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        public async Task<List<ExpenseByCategoryData>> GetExpenseByCategorySummary(VMSummary param)
        {
            return await GetExpenseBaseQuery(param)
                .GroupBy(x => new { x.ExpenseCode, x.ExpenseDesc })
                .Select(g => new ExpenseByCategoryData
                {
                    Code = g.Key.ExpenseCode,
                    Desc = g.Key.ExpenseDesc,
                    Amount = g.Sum(x => x.Amount ?? 0),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Amount)
                .ToListAsync();
        }
    }
}