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

        public async Task<List<MAggregateData>> GetMerchantCountByStatus(VMSummary param)
        {
            var result = await context!.Merchants!.AsExpandable()
                .Where(IsOrgMatchPredicate<MMerchant>())
                .GroupBy(x => x.Status)
                .Select(g => new MAggregateData()
                {
                    AggregateKey1 = g.Key,
                    AggregateCount1 = g.Count()
                }).ToListAsync();

            return result;
        }

        public async Task<List<MAggregateData>> GetMerchantCount(VMSummary param)
        {
            var result = await context!.Merchants!.AsExpandable()
                .Where(IsOrgMatchPredicate<MMerchant>())
                .GroupBy(x => 1)
                .Select(g => new MAggregateData()
                {
                    AggregateCount1 = g.Count()
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

        public async Task<List<MAggregateData>> GetMerchantsBalance()
        {
            var result = await GetSelectionWallet().AsExpandable()
                .Where(IsOrgMatchPredicate<MWallet>())
                .Where(x => x.MerchantCode != null)
                .GroupBy(x => x.MerchantCode)
                .Select(g => new MAggregateData()
                {
                    AggregateKey1 = g.Key,
                    AggregateAmount1 = g.Sum(x => x.PointBalanceDecimal)
                })
                .OrderByDescending(x => x.AggregateAmount1)
                .ToListAsync();

            return result;
        }

        public IQueryable<MPaymentTransaction> GetSelectionPaymentTx()
        {
            var query =
                from pmt in context!.PaymentTransactions
                select new { pmt };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MPaymentTransaction
            {
                MerchantCode = x.pmt.MerchantCode,
                TxAmountDecimal = x.pmt.TxAmountDecimal,
                PayInFeeDecimal = x.pmt.PayInFeeDecimal,
                Direction = x.pmt.Direction,
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

        public async Task<List<MAggregateData>> GetMerchantsPayInAmountSummary(VMSummary param)
        {
            var result = await GetSelectionPaymentTx().AsExpandable()
                .Where(IsOrgMatchPredicate<MPaymentTransaction>())
                .Where(DateRangePredicate<MPaymentTransaction>(param))
                .Where(x => x.Direction == "PayIn")
                .GroupBy(x => x.MerchantCode)
                .Select(g => new MAggregateData()
                {
                    AggregateKey1 = g.Key,
                    AggregateAmount1 = g.Sum(x => x.TxAmountDecimal),
                    AggregateAmount2 = g.Sum(x => x.PayInFeeDecimal)
                })
                .OrderByDescending(x => x.AggregateAmount1)
                .ToListAsync();

            return result;
        }

        public async Task<List<MAggregateData>> GetMerchantsPayOutAmountSummary(VMSummary param)
        {
            var result = await GetSelectionPaymentTx().AsExpandable()
                .Where(IsOrgMatchPredicate<MPaymentTransaction>())
                .Where(DateRangePredicate<MPaymentTransaction>(param))
                .Where(x => x.Direction == "PayOut")
                .GroupBy(x => x.MerchantCode)
                .Select(g => new MAggregateData()
                {
                    AggregateKey1 = g.Key,
                    AggregateAmount1 = g.Sum(x => x.TxAmountDecimal),
                    AggregateAmount2 = g.Sum(x => x.PayInFeeDecimal)
                })
                .OrderByDescending(x => x.AggregateAmount1)
                .ToListAsync();

            return result;
        }
    }
}