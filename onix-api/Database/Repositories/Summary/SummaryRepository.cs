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
                MerchantCode = x.merchant.Code,
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
                    FeeAmount = g.Sum(x => x.PayInFeeDecimal)
                })
                .OrderByDescending(x => x.TxAmount)
                .ToListAsync();

            return result;
        }
    }
}