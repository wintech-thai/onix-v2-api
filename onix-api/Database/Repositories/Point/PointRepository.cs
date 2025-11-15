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
            var limit = 0;
            var offset = 0;

            //Param will never be null
            if (param.Offset > 0)
            {
                //Convert to zero base
                offset = param.Offset-1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var predicate = PointTxsPredicate(param!);
            var r = await context!.PointTxs!.Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

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

            await context!.PointTxs!.AddAsync(tx);
            await context.SaveChangesAsync();

            return tx;
        }

        private async Task<MPointBalance> UpsertPointBalance(MPointBalance bal)
        {
            //จะไม่มีการเรียก SaveChange() ในนี้

            //ยังไง item ต้องไม่เป็น null
            var walletId = Guid.Parse(bal.WalletId!);
            var item = context!.Wallets!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(walletId)).FirstOrDefault();
            if (item != null)
            {
                //update ไปที่ Items ด้วย
                item!.PointBalance = bal.BalanceEnd;
            }

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

            await context!.PointTxs!.AddAsync(tx);
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

        public async Task<MWallet> AddWallet(MWallet wallet)
        {
            wallet.Id = Guid.NewGuid();
            wallet.CreatedDate = DateTime.UtcNow;
            wallet.OrgId = orgId;

            await context!.Wallets!.AddAsync(wallet);
            await context.SaveChangesAsync();

            return wallet;
        }

        public async Task<MWallet?> UpdateWalletById(string walletId, MWallet wallet)
        {
            Guid id = Guid.Parse(walletId);
            var result = await context!.Wallets!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                result.Tags = wallet.Tags;
                result.Description = wallet.Description;
                result.Name = wallet.Name;
                result.UpdatedDate = DateTime.UtcNow;

                context!.SaveChanges();
            }

            return result!;
        }

        public async Task<MWallet?> GetWalletById(string walletId)
        {
            Guid id = Guid.Parse(walletId);
            var u = await context!.Wallets!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public async Task<MWallet?> DeleteWalletById(string walletId)
        {
            Guid id = Guid.Parse(walletId);

            var r = await context!.Wallets!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).FirstOrDefaultAsync();
            if (r != null)
            {
                context!.Wallets!.Remove(r);
                await context.SaveChangesAsync();
            }

            return r;
        }

        public async Task<MWallet?> AttachCustomerToWalletById(string walletId, string custId, MEntity customer)
        {
            //TODO : อนาคตเอาข้อมูลจาก customer ไปใส่ใน tag ของ wallet เช่น email=<email_address>
            Guid id = Guid.Parse(walletId);

            var r = await context!.Wallets!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).FirstOrDefaultAsync();
            if (r != null)
            {
                r.CustomerId = custId;
                r.UpdatedDate = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }

            return r;
        }

        private ExpressionStarter<MWallet> WalletPredicate(VMWallet param)
        {
            var pd = PredicateBuilder.New<MWallet>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MWallet>();
                fullTextPd = fullTextPd.Or(p => p.Name!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<List<MWallet>> GetWallets(VMWallet param)
        {
            var limit = 0;
            var offset = 0;

            //Param will never be null
            if (param.Offset > 0)
            {
                //Convert to zero base
                offset = param.Offset-1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var predicate = WalletPredicate(param!);
            var result = await context!.Wallets!.Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return result;
        }

        public async Task<int> GetWalletsCount(VMWallet param)
        {
            var predicate = WalletPredicate(param!);
            var result = await context!.Wallets!.Where(predicate).CountAsync();

            return result;
        }
    }
}
