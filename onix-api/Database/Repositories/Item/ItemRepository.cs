using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public class ItemRepository : BaseRepository, IItemRepository
    {
        public ItemRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public MItem AddItem(MItem item)
        {
            item.Id = Guid.NewGuid();
            item.CreatedDate = DateTime.UtcNow;
            item.UpdatedDate = DateTime.UtcNow;
            item.OrgId = orgId;

            context!.Items!.Add(item);
            context.SaveChanges();

            return item;
        }

        private ExpressionStarter<MItem> ItemPredicate(VMItem param)
        {
            var pd = PredicateBuilder.New<MItem>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MItem>();
                fullTextPd = fullTextPd.Or(p => p.Code!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            if ((param.ItemType != null) && (param.ItemType > 0))
            {
                var itemPd = PredicateBuilder.New<MItem>();
                itemPd = itemPd.Or(p => p.ItemType!.Equals(param.ItemType));

                pd = pd.And(itemPd);
            }

            if ((param.Status != null) && (param.Status != ""))
            {
                var statusPd = PredicateBuilder.New<MItem>();
                statusPd = statusPd.Or(p => p.Status!.Equals(param.Status));

                pd = pd.And(statusPd);
            }

            return pd;
        }

        public int GetItemCount(VMItem param)
        {
            var predicate = ItemPredicate(param);
            var cnt = context!.Items!.Where(predicate).Count();

            return cnt;
        }

        public IEnumerable<MItem> GetItems(VMItem param)
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

            var predicate = ItemPredicate(param!);
            var arr = context!.Items!.Where(predicate)
                .OrderByDescending(e => e.Code)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }

        public MItem GetItemById(string itemId)
        {
            Guid id = Guid.Parse(itemId);

            var u = context!.Items!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public MItem GetItemByName(string code)
        {
            var u = context!.Items!.Where(p => p!.Code!.Equals(code) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public bool IsItemCodeExist(string code)
        {
            var cnt = context!.Items!.Where(p => p!.Code!.Equals(code)
                && p!.OrgId!.Equals(orgId)).Count();

            return cnt >= 1;
        }

        public MItem? DeleteItemById(string ItemId)
        {
            Guid id = Guid.Parse(ItemId);

            var r = context!.Items!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).FirstOrDefault();
            if (r != null)
            {
                context!.Items!.Remove(r);
                context.SaveChanges();
            }

            return r;
        }

        public MItem? UpdateItemById(string itemId, MItem item)
        {
            Guid id = Guid.Parse(itemId);
            var result = context!.Items!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                //ไม่ต้องมี ItemType
                result.Properties = item.Properties;
                result.Description = item.Description;
                result.Narrative = item.Narrative;
                result.Content = item.Content;
                result.Tags = item.Tags;
                result.UpdatedDate = DateTime.UtcNow;
                result.EffectiveDate = item.EffectiveDate;
                result.ExpireDate = item.ExpireDate;
                result.Status = item.Status;
                
                context!.SaveChanges();
            }

            return result!;
        }

        public MItem? ApproveItemById(string itemId)
        {
            Guid id = Guid.Parse(itemId);
            var result = context!.Items!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.Status = "Approved";
                context!.SaveChanges();
            }

            return result!;
        }

        public MItem? DisableItemById(string itemId)
        {
            Guid id = Guid.Parse(itemId);
            var result = context!.Items!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.Status = "Disabled";
                context!.SaveChanges();
            }

            return result!;
        }

        private ExpressionStarter<MItemBalance> ItemBalancePredicate(VMItemBalance param)
        {
            var pd = PredicateBuilder.New<MItemBalance>();

            pd = pd.And(p => p.OrgId!.Equals(orgId) && p.ItemId!.Equals(param.ItemId) && p.StatCode!.Equals(param.BalanceType));
/*
            if ((param.DateKey != null) && (param.DateKey != ""))
            {
                var dateKeyPd = PredicateBuilder.New<MItemBalance>();
                dateKeyPd = dateKeyPd.Or(p => p.BalanceDateKey!.Equals(param.DateKey));

                pd = pd.And(dateKeyPd);
            }
*/
            return pd;
        }

        public MItemBalance? GetItemBalanceByItemId(VMItemBalance param)
        {
            var predicate = ItemBalancePredicate(param!);
            var result = context!.ItemBalances!.Where(predicate).FirstOrDefault();
            return result;
        }

        private MItemBalance UpsertItemBalance(MItemBalance bal)
        {
            //จะไม่มีการเรียก SaveChange() ในนี้

            //ยังไง item ต้องไม่เป็น null
            var itemId = Guid.Parse(bal.ItemId!);
            var item = context!.Items!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(itemId)).FirstOrDefault();
            if (item != null)
            {
                //update ไปที่ Items ด้วย
                item!.CurrentBalance = bal.BalanceEnd;
            }

            if (bal.IsNew)
            {
                context!.ItemBalances!.Add(bal);
            }
            else
            {
                var result = context!.ItemBalances!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(bal.Id)).FirstOrDefault();
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

        public MItemTx AddItemTxWithBalance(MItemTx tx, MItemBalance currBal)
        {
            tx.OrgId = orgId;
            currBal.OrgId = orgId;

            //Transaction control จะถูกจัดการเองในนี้เลย

            context!.ItemTxs!.Add(tx);
            UpsertItemBalance(currBal);

            context.SaveChanges();
            return tx;
        }

        private ExpressionStarter<MItemTx> PointTxsPredicate(VMItemTx param)
        {
            var pd = PredicateBuilder.New<MItemTx>();

            pd = pd.And(p => p.OrgId!.Equals(orgId) && p.ItemId!.Equals(param.ItemId));

            if (param.FromDate != null)
            {
                var fromDatePd = PredicateBuilder.New<MItemTx>();
                fromDatePd = fromDatePd.Or(p => p.CreatedDate! >= param.FromDate);
                pd = pd.And(fromDatePd);
            }

            if (param.ToDate != null)
            {
                var toDatePd = PredicateBuilder.New<MItemTx>();
                toDatePd = toDatePd.Or(p => p.CreatedDate! <= param.ToDate);
                pd = pd.And(toDatePd);
            }

            return pd;
        }

        public List<MItemTx> GetItemTxsByItemId(VMItemTx param)
        {
            var predicate = PointTxsPredicate(param!);
            var r = context!.ItemTxs!.Where(predicate).ToList();

            return r;
        }

        public int GetItemTxsCountByItemId(VMItemTx param)
        {
            var predicate = PointTxsPredicate(param!);
            var r = context!.ItemTxs!.Where(predicate).Count();

            return r;
        }
    }
}