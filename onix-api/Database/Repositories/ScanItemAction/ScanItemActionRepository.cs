using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public class ScanItemActionRepository : BaseRepository, IScanItemActionRepository
    {
        public ScanItemActionRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public MScanItemAction AddScanItemAction(MScanItemAction action)
        {
            action.Id = Guid.NewGuid();
            action.CreatedDate = DateTime.UtcNow;
            action.OrgId = orgId;

            context!.ScanItemActions!.Add(action);
            context.SaveChanges();

            return action;
        }

        private ExpressionStarter<MScanItemAction> ScanItemActionPredicate(VMScanItemAction param)
        {
            var pd = PredicateBuilder.New<MScanItemAction>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));
            return pd;
        }

        public int GetScanItemActionCount(VMScanItemAction param)
        {
            var predicate = ScanItemActionPredicate(param);
            var cnt = context!.ScanItemActions!.Where(predicate).Count();

            return cnt;
        }

        public MScanItemAction GetScanItemActionById(string actionId)
        {
            Guid id = Guid.Parse(actionId);

            var u = context!.ScanItemActions!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public MScanItemAction GetScanItemAction()
        {
            var u = context!.ScanItemActions!.Where(p => p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public MScanItemAction? DeleteScanItemActionById(string ScanItemActionId)
        {
            Guid id = Guid.Parse(ScanItemActionId);

            var r = context!.ScanItemActions!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).FirstOrDefault();
            if (r != null)
            {
                context!.ScanItemActions!.Remove(r);
                context.SaveChanges();
            }

            return r;
        }

        public MScanItemAction? UpdateScanItemActionById(string actionId, MScanItemAction item)
        {
            Guid id = Guid.Parse(actionId);
            var result = context!.ScanItemActions!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.RedirectUrl = item.RedirectUrl;
                result.EncryptionKey = item.EncryptionKey;
                result.EncryptionIV = item.EncryptionIV;
                result.RegisteredAwareFlag = item.RegisteredAwareFlag;
                
                context!.SaveChanges();
            }

            return result!;
        }
    }
}