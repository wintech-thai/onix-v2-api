using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class DocumentNumberRepository : BaseRepository, IDocumentNumberRepository
    {
        public DocumentNumberRepository(IDataContext ctx)
        {
            context = ctx;
        }

        private ExpressionStarter<MDocumentNumberConfig> BuildPredicate(VMDocumentNumberConfig param)
        {
            var pd = PredicateBuilder.New<MDocumentNumberConfig>();
            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if (!string.IsNullOrEmpty(param.FullTextSearch))
            {
                var fts = PredicateBuilder.New<MDocumentNumberConfig>();
                fts = fts.Or(p => p.DocumentType!.Contains(param.FullTextSearch));
                fts = fts.Or(p => p.DocumentFormat!.Contains(param.FullTextSearch));
                pd = pd.And(fts);
            }

            if (!string.IsNullOrEmpty(param.DocumentType))
                pd = pd.And(p => p.DocumentType!.Equals(param.DocumentType));

            return pd;
        }

        public async Task<MDocumentNumberConfig> AddDocumentNumberConfigAsync(MDocumentNumberConfig config)
        {
            config.OrgId = orgId;
            await context!.DocumentNumberConfigs!.AddAsync(config);
            await context.SaveChangesAsync();
            return config;
        }

        public async Task<MDocumentNumberConfig?> GetDocumentNumberConfigByIdAsync(string id)
        {
            Guid guid = Guid.Parse(id);
            return await context!.DocumentNumberConfigs!
                .AsExpandable()
                .Where(p => p.Id!.Equals(guid) && p.OrgId!.Equals(orgId))
                .FirstOrDefaultAsync();
        }

        public async Task<List<MDocumentNumberConfig>> GetDocumentNumberConfigsAsync(VMDocumentNumberConfig param)
        {
            var offset = param.Offset > 0 ? param.Offset - 1 : 0;
            var limit = param.Limit > 0 ? param.Limit : 100;

            var predicate = BuildPredicate(param);
            return await context!.DocumentNumberConfigs!
                .AsExpandable()
                .Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetDocumentNumberConfigCountAsync(VMDocumentNumberConfig param)
        {
            var predicate = BuildPredicate(param);
            return await context!.DocumentNumberConfigs!.AsExpandable().Where(predicate).CountAsync();
        }

        public async Task<MDocumentNumberConfig?> UpdateDocumentNumberConfigByIdAsync(string id, MDocumentNumberConfig config)
        {
            Guid guid = Guid.Parse(id);
            var existing = await context!.DocumentNumberConfigs!
                .AsExpandable()
                .Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(guid))
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.DocumentFormat = config.DocumentFormat;
                existing.SeqDigit = config.SeqDigit;
                existing.ResetType = config.ResetType;
                existing.YearOffset = config.YearOffset;
                existing.Tags = config.Tags;
                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<MDocumentNumberConfig?> DeleteDocumentNumberConfigByIdAsync(string id)
        {
            Guid guid = Guid.Parse(id);
            var existing = await context!.DocumentNumberConfigs!
                .AsExpandable()
                .Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(guid))
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                context!.DocumentNumberConfigs!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<bool> IsDocumentTypeExistAsync(string documentType)
        {
            return await context!.DocumentNumberConfigs!
                .AsExpandable()
                .AnyAsync(p => p.DocumentType!.Equals(documentType) && p.OrgId!.Equals(orgId));
        }

        public async Task<MDocumentNumberConfig?> GetDocumentNumberConfigByTypeAsync(string documentType)
        {
            return await context!.DocumentNumberConfigs!
                .AsExpandable()
                .Where(p => p.DocumentType!.Equals(documentType) && p.OrgId!.Equals(orgId))
                .FirstOrDefaultAsync();
        }

        public async Task<MDocumentNumberConfig?> UpdateDocumentNumberSequenceAsync(string id, int sequenceNo, string sequenceKey)
        {
            Guid guid = Guid.Parse(id);
            var existing = await context!.DocumentNumberConfigs!
                .AsExpandable()
                .Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(guid))
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.CurrentSequenceNo = sequenceNo;
                existing.CurrentSequenceKey = sequenceKey;
                await context.SaveChangesAsync();
            }

            return existing;
        }
    }
}
