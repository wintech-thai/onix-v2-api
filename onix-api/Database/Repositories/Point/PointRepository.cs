using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace Its.Onix.Api.Database.Repositories
{
    public class PointRepository : BaseRepository, IPointRepository
    {
        public PointRepository(IDataContext ctx)
        {
            context = ctx;
        }

        private ExpressionStarter<MPointTx> PointTxsPredicate(VMPointTx param)
        {
            var pd = PredicateBuilder.New<MPointTx>();

            pd = pd.And(p => p.OrgId!.Equals(orgId) && p.WalletId!.Equals(param.WalletId));

            if (param.FromDate != null)
            {
                var fromDatePd = PredicateBuilder.New<MPointTx>();
                fromDatePd = fromDatePd.Or(p => p.CreatedDate! >= param.FromDate);
                pd = pd.And(fromDatePd);
            }

            if (param.ToDate != null)
            {
                var toDatePd = PredicateBuilder.New<MPointTx>();
                toDatePd = toDatePd.Or(p => p.CreatedDate! <= param.ToDate);
                pd = pd.And(toDatePd);
            }

            return pd;
        }

        public async Task<List<MPointTx>> GetPointTxsByWalletId(VMPointTx param)
        {
            var predicate = PointTxsPredicate(param!);
            var r = await context!.PointTxs!.Where(predicate).ToListAsync();

            return r;
        }

        public async Task<int> GetPointTxsCountByWalletId(VMPointTx param)
        {
            var predicate = PointTxsPredicate(param!);
            var r = await context!.PointTxs!.Where(predicate).CountAsync();

            return r;
        }

        public async Task<MPointTx> AddPointTx(MPointTx tx)
        {
            tx.OrgId = orgId;

            context!.PointTxs!.Add(tx);
            await context.SaveChangesAsync();

            return tx;
        }

        private async Task<MPointBalance> UpsertPointBalance(MPointBalance bal)
        {
            //จะไม่มีการเรียก SaveChange() ในนี้
            if (bal.IsNew)
            {
                await context!.PointBalances!.AddAsync(bal);
            }
            else
            {
                var result = await context!.PointBalances!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(bal.Id)).FirstOrDefaultAsync();
                if (result == null)
                {
                    return null!;
                }

                result.BalanceDate = bal.BalanceDate;
                result.TxIn = bal.TxIn;
                result.TxOut = bal.TxOut;
                result.BalanceBegin = bal.BalanceBegin;
                result.BalanceEnd = bal.BalanceEnd;
            }

            return bal;
        }

        public async Task<MPointTx> AddPointTxWithBalance(MPointTx tx, MPointBalance currBal, MPointBalance dailyBal)
        {
            tx.OrgId = orgId;
            currBal.OrgId = orgId;
            dailyBal.OrgId = orgId;

            //Transaction controll จะถูกจัดการเองในนี้เลย

            context!.PointTxs!.Add(tx);
            await UpsertPointBalance(currBal);
            await UpsertPointBalance(dailyBal);

            await context.SaveChangesAsync();

            return tx;
        }

        private ExpressionStarter<MPointBalance> PointBalancePredicate(VMPointBalance param)
        {
            var pd = PredicateBuilder.New<MPointBalance>();

            pd = pd.And(p => p.OrgId!.Equals(orgId) && p.WalletId!.Equals(param.WalletId) && p.StatCode!.Equals(param.BalanceType));

            if ((param.DateKey != null) && (param.DateKey != ""))
            {
                var dateKeyPd = PredicateBuilder.New<MPointBalance>();
                dateKeyPd = dateKeyPd.Or(p => p.BalanceDateKey!.Equals(param.DateKey));

                pd = pd.And(dateKeyPd);
            }

            return pd;
        }

        public async Task<MPointBalance?> GetPointBalanceByWalletId(VMPointBalance param)
        {
            var predicate = PointBalancePredicate(param!);
            var result = await context!.PointBalances!.Where(predicate).FirstOrDefaultAsync();
            return result;
        }
    }
}
