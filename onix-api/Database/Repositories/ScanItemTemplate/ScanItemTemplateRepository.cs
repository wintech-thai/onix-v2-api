using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class ScanItemTemplateRepository : BaseRepository, IScanItemTemplateRepository
    {
        public ScanItemTemplateRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<bool> IsScanItemTemplateExist(string templateName)
        {
            var exists = await context!.ScanItemTemplates!.AnyAsync(p => p!.TemplateName!.Equals(templateName) && p!.OrgId!.Equals(orgId));
            return exists;
        }

        public IQueryable<MScanItemTemplate> GetSelection()
        {
            var query =
                from sca in context!.ScanItemTemplates
                select new { sca };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MScanItemTemplate
            {
                Id = x.sca.Id,
                OrgId = x.sca.OrgId,
                TemplateName = x.sca.TemplateName,
                Description = x.sca.Description,
                SerialPrefixDigit = x.sca.SerialPrefixDigit,
                GeneratorCount = x.sca.GeneratorCount,
                SerialDigit = x.sca.SerialDigit,
                PinDigit = x.sca.PinDigit,
                UrlTemplate = x.sca.UrlTemplate,
                NotificationEmail = x.sca.NotificationEmail,
                Tags = x.sca.Tags,

                CreatedDate = x.sca.CreatedDate,
            });
        }

        public async Task<MScanItemTemplate> GetScanItemTemplate_V2()
        {
            var u = await GetSelection().AsExpandable().Where(p => p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }


        private ExpressionStarter<MScanItemTemplate> ScanItemTemplatePredicate(VMScanItemTemplate param)
        {
            var pd = PredicateBuilder.New<MScanItemTemplate>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MScanItemTemplate>();
                fullTextPd = fullTextPd.Or(p => p.TemplateName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.UrlTemplate!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<List<MScanItemTemplate>> GetScanItemTemplates_V2(VMScanItemTemplate param)
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

            var predicate = ScanItemTemplatePredicate(param!);
            var result = await GetSelection().AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.CreatedDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            return result;
        }

        public async Task<int> GetScanItemTemplateCount_V2(VMScanItemTemplate param)
        {
            var predicate = ScanItemTemplatePredicate(param!);
            var result = await context!.ScanItemTemplates!.Where(predicate).AsExpandable().CountAsync();

            return result;
        }

        public async Task<MScanItemTemplate?> GetScanItemTemplateById_V2(string templateId)
        {
            Guid id = Guid.Parse(templateId);
            var u = await GetSelection().AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public async Task<MScanItemTemplate> AddScanItemTemplate_V2(MScanItemTemplate template)
        {
            template.OrgId = orgId;

            await context!.ScanItemTemplates!.AddAsync(template);
            await context.SaveChangesAsync();

            return template;
        }

        public async Task<MScanItemTemplate?> DeleteScanItemTemplateById_V2(string templateId)
        {
            Guid id = Guid.Parse(templateId);
            var existing = await context!.ScanItemTemplates!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                context.ScanItemTemplates!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<MScanItemTemplate?> UpdateScanItemTemplateById_V2(string templateId, MScanItemTemplate template)
        {
            Guid id = Guid.Parse(templateId);
            var existing = await context!.ScanItemTemplates!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.TemplateName = template.TemplateName;
                existing.Description = template.Description;
                existing.Tags = template.Tags;
                existing.UrlTemplate = template.UrlTemplate;
                existing.SerialPrefixDigit = template.SerialPrefixDigit;
                existing.GeneratorCount = template.GeneratorCount;
                existing.SerialDigit = template.SerialDigit;
                existing.PinDigit = template.PinDigit;
                existing.NotificationEmail = template.NotificationEmail;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MScanItemTemplate?> SetScanItemTemplateDefault_V2(string templateId)
        {
            Guid id = Guid.Parse(templateId);

            var previousDefaults = await context!.ScanItemTemplates!.AsExpandable().Where(p => p!.IsDefault!.Equals("YES") && p!.OrgId!.Equals(orgId)).ToListAsync();
            foreach (var item in previousDefaults)
            {
                item.IsDefault = "NO";
            }

            var existing = await context!.ScanItemTemplates!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.IsDefault = "YES";
            }

            await context.SaveChangesAsync();
            return existing;
        }
    }
}