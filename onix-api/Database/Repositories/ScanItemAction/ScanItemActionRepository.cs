using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class ScanItemActionRepository : BaseRepository, IScanItemActionRepository
    {
        public ScanItemActionRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public IQueryable<MScanItemAction> GetSelection()
        {
            var query =
                from sca in context!.ScanItemActions
                select new { sca };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MScanItemAction
            {
                Id = x.sca.Id,
                OrgId = x.sca.OrgId,
                ActionName = x.sca.ActionName,
                Description = x.sca.Description,
                RedirectUrl = x.sca.RedirectUrl,
                EncryptionKey = x.sca.EncryptionKey,
                EncryptionIV = x.sca.EncryptionIV,
                ThemeVerify = x.sca.ThemeVerify,
                RegisteredAwareFlag = x.sca.RegisteredAwareFlag,
                Tags = x.sca.Tags,
                IsDefault = x.sca.IsDefault,

                CreatedDate = x.sca.CreatedDate,
            });
        }

        private ExpressionStarter<MScanItemAction> ScanItemActionPredicate(VMScanItemAction param)
        {
            var pd = PredicateBuilder.New<MScanItemAction>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MScanItemAction>();
                fullTextPd = fullTextPd.Or(p => p.ActionName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.RedirectUrl!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<List<MScanItemAction>> GetScanItemActions_V2(VMScanItemAction param)
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

            var predicate = ScanItemActionPredicate(param!);
            var result = await GetSelection()
            .Where(predicate)
            .OrderByDescending(e => e.CreatedDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            foreach (var r in result)
            {
                r.EncryptionKey = "******";
                r.EncryptionIV = "******";
            }

            return result;
        }

        public async Task<MScanItemAction?> GetDefaultScanItemAction_V2()
        {
            var u = await GetSelection().Where(p => p!.IsDefault!.Equals("YES") && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public async Task<int> GetScanItemActionsCount_V2(VMScanItemAction param)
        {
            var predicate = ScanItemActionPredicate(param!);
            var result = await context!.ScanItemActions!.Where(predicate).CountAsync();

            return result;
        }

        public async Task<MScanItemAction?> GetScanItemActionById_V2(string actionId)
        {
            Guid id = Guid.Parse(actionId);
            var u = await GetSelection().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public async Task<MScanItemAction> AddScanItemAction_V2(MScanItemAction action)
        {
            action.OrgId = orgId;

            await context!.ScanItemActions!.AddAsync(action);
            await context.SaveChangesAsync();

            return action;
        }

        public async Task<MScanItemAction?> DeleteScanItemActionById_V2(string actionId)
        {
            Guid id = Guid.Parse(actionId);
            var existing = await context!.ScanItemActions!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                context.ScanItemActions!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<MScanItemAction?> UpdateScanItemActionById_V2(string actionId, MScanItemAction scanItemAction)
        {
            Guid id = Guid.Parse(actionId);
            var existing = await context!.ScanItemActions!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.ActionName = scanItemAction.ActionName;
                existing.Description = scanItemAction.Description;
                existing.Tags = scanItemAction.Tags;
                existing.RedirectUrl = scanItemAction.RedirectUrl;
                existing.EncryptionKey = scanItemAction.EncryptionKey;
                existing.EncryptionIV = scanItemAction.EncryptionIV;
                existing.RegisteredAwareFlag = scanItemAction.RegisteredAwareFlag;
                existing.ThemeVerify = scanItemAction.ThemeVerify;
                //ไม่ต้องมี IsDefault field
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MScanItemAction?> SetScanItemActionDefault_V2(string actionId)
        {
            Guid id = Guid.Parse(actionId);

            var previousDefaults = await context!.ScanItemActions!.Where(p => p!.IsDefault!.Equals("YES") && p!.OrgId!.Equals(orgId)).ToListAsync();
            foreach (var item in previousDefaults)
            {
                item.IsDefault = "NO";
            }

            var existing = await context!.ScanItemActions!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.IsDefault = "YES";
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> IsScanItemActionExist(string actionName)
        {
            var exists = await context!.ScanItemActions!.AnyAsync(p => p!.ActionName!.Equals(actionName) && p!.OrgId!.Equals(orgId));
            return exists;
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
                result.ThemeVerify = item.ThemeVerify;
                
                context!.SaveChanges();
            }

            return result!;
        }
    }
}