using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class ScanItemFolderRepository : BaseRepository, IScanItemFolderRepository
    {
        public ScanItemFolderRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<bool> IsScanItemFolderExist(string folderName)
        {
            var exists = await context!.ScanItemFolders!.AnyAsync(p => p!.FolderName!.Equals(folderName) && p!.OrgId!.Equals(orgId));
            return exists;
        }

        public IQueryable<MScanItemFolder> GetSelection()
        {
            var query =
                from scf in context!.ScanItemFolders
                join sca in context.ScanItemActions!
                    on scf.ScanItemActionId equals sca.Id.ToString() into sciAction
                from action in sciAction.DefaultIfEmpty()
                select new { scf, action };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MScanItemFolder
            {
                Id = x.scf.Id,
                OrgId = x.scf.OrgId,
                FolderName = x.scf.FolderName,
                Description = x.scf.Description,
                Tags = x.scf.Tags,
                ScanItemCount = x.scf.ScanItemCount,
                ScanItemActionName = x.action.ActionName,

                CreatedDate = x.scf.CreatedDate,
            });
        }

        public async Task<MScanItemFolder> GetScanItemFolder()
        {
            var u = await GetSelection().AsExpandable().Where(p => p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        private ExpressionStarter<MScanItemFolder> ScanItemFolderPredicate(VMScanItemFolder param)
        {
            var pd = PredicateBuilder.New<MScanItemFolder>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MScanItemFolder>();
                fullTextPd = fullTextPd.Or(p => p.FolderName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.ScanItemActionName!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<List<MScanItemFolder>> GetScanItemFolders(VMScanItemFolder param)
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

            var predicate = ScanItemFolderPredicate(param!);
            var result = await GetSelection().AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.CreatedDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            return result;
        }

        public async Task<int> GetScanItemFolderCount(VMScanItemFolder param)
        {
            var predicate = ScanItemFolderPredicate(param!);
            var result = await context!.ScanItemFolders!.Where(predicate).AsExpandable().CountAsync();

            return result;
        }

        public async Task<MScanItemFolder?> GetScanItemFolderById(string folderId)
        {
            Guid id = Guid.Parse(folderId);
            var u = await GetSelection().AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public async Task<MScanItemFolder> AddScanItemFolder(MScanItemFolder folder)
        {
            folder.OrgId = orgId;

            await context!.ScanItemFolders!.AddAsync(folder);
            await context.SaveChangesAsync();

            return folder;
        }

        public async Task<MScanItemFolder?> DeleteScanItemFolderById(string folderId)
        {
            Guid id = Guid.Parse(folderId);
            var existing = await context!.ScanItemFolders!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                context.ScanItemFolders!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<MScanItemFolder?> UpdateScanItemFolderById(string folderId, MScanItemFolder folder)
        {
            Guid id = Guid.Parse(folderId);
            var existing = await context!.ScanItemFolders!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.FolderName = folder.FolderName;
                existing.Description = folder.Description;
                existing.Tags = folder.Tags;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MScanItemFolder?> AttachScanItemFolderToAction(string folderId, string actionId)
        {
            Guid id = Guid.Parse(folderId);
            var existing = await context!.ScanItemFolders!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.ScanItemActionId = actionId;
            }

            await context.SaveChangesAsync();
            return existing;
        }
    }
}