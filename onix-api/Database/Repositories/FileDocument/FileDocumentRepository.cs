using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class FileDocumentRepository : BaseRepository, IFileDocumentRepository
    {
        public FileDocumentRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<bool> IsStoragePathExist(string storagePath)
        {
            var exists = await context!.FileDocuments!.AsExpandable().AnyAsync(p => p!.ObjectStoragePath!.Equals(storagePath) && p!.OrgId!.Equals(orgId));
            return exists;
        }

        public async Task<MFileDocument?> GetFileDocumentByStoragePath(string storagePath)
        {
            var fileDocument = await context!.FileDocuments!.AsExpandable().FirstOrDefaultAsync(p => p!.ObjectStoragePath!.Equals(storagePath) && p!.OrgId!.Equals(orgId));
            return fileDocument;
        }

        public async Task<List<MFileDocument>> GetFileDocuments(VMFileDocument param)
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

            var predicate = FileDocumentPredicate(param!);
            var result = await GetSelection().AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.CreatedDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            return result;
        }

        public async Task<int> GetFileDocumentCount(VMFileDocument param)
        {
            var predicate = FileDocumentPredicate(param!);
            var result = await context!.FileDocuments!.Where(predicate).AsExpandable().CountAsync();

            return result;
        }

        public async Task<MFileDocument?> GetFileDocumentById(string fileDocumentId)
        {
            Guid id = Guid.Parse(fileDocumentId);
            var u = await GetSelection().AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public IQueryable<MFileDocument> GetSelection()
        {
            var query =
                from fd in context!.FileDocuments
                select new { fd };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MFileDocument
            {
                Id = x.fd.Id,
                OrgId = x.fd.OrgId,
                ObjectStoragePath = x.fd.ObjectStoragePath,
                Tags = x.fd.Tags,
                DocumentType = x.fd.DocumentType,
                MimeType = x.fd.MimeType,
                PublicDocumentUrl = x.fd.PublicDocumentUrl,
                IsPublic = x.fd.IsPublic,
                CreatedDate = x.fd.CreatedDate,
            });
        }

        private ExpressionStarter<MFileDocument> FileDocumentPredicate(VMFileDocument param)
        {
            var pd = PredicateBuilder.New<MFileDocument>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MFileDocument>();
                fullTextPd = fullTextPd.Or(p => p.ObjectStoragePath!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            if ((param.DocumentType != "") && (param.DocumentType != null))
            {
                var docTypePd = PredicateBuilder.New<MFileDocument>();
                docTypePd = docTypePd.Or(p => p.DocumentType!.Equals(param.DocumentType));

                pd = pd.And(docTypePd);
            }

            return pd;
        }

        public async Task<MFileDocument> AddFileDocument(MFileDocument fileDocument)
        {
            fileDocument.OrgId = orgId;
            fileDocument.CreatedDate = DateTime.UtcNow;

            await context!.FileDocuments!.AddAsync(fileDocument);
            await context.SaveChangesAsync();

            return fileDocument;
        }

        public async Task<MFileDocument?> DeleteFileDocumentById(string fileDocumentId)
        {
            Guid id = Guid.Parse(fileDocumentId);
            var existing = await context!.FileDocuments!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                context.FileDocuments!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<MFileDocument?> UpdateFileDocumentById(string fileDocumentId, MFileDocument fileDocument)
        {
            Guid id = Guid.Parse(fileDocumentId);
            var existing = await context!.FileDocuments!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.ObjectStoragePath = fileDocument.ObjectStoragePath;
                existing.Tags = fileDocument.Tags;
                existing.Description = fileDocument.Description;
            }

            await context.SaveChangesAsync();
            return existing;
        }
    }
}