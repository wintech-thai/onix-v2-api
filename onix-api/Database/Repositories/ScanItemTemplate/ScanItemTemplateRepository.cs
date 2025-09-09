using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public class ScanItemTemplateRepository : BaseRepository, IScanItemTemplateRepository
    {
        public ScanItemTemplateRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public MScanItemTemplate AddScanItemTemplate(MScanItemTemplate action)
        {
            action.Id = Guid.NewGuid();
            action.CreatedDate = DateTime.UtcNow;
            action.OrgId = orgId;

            context!.ScanItemTemplates!.Add(action);
            context.SaveChanges();

            return action;
        }

        private ExpressionStarter<MScanItemTemplate> ScanItemTemplatePredicate(VMScanItemTemplate param)
        {
            var pd = PredicateBuilder.New<MScanItemTemplate>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));
            return pd;
        }

        public int GetScanItemTemplateCount(VMScanItemTemplate param)
        {
            var predicate = ScanItemTemplatePredicate(param);
            var cnt = context!.ScanItemTemplates!.Where(predicate).Count();

            return cnt;
        }

        public MScanItemTemplate GetScanItemTemplateById(string actionId)
        {
            Guid id = Guid.Parse(actionId);

            var u = context!.ScanItemTemplates!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public MScanItemTemplate GetScanItemTemplate()
        {
            var u = context!.ScanItemTemplates!.Where(p => p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }

        public MScanItemTemplate? DeleteScanItemTemplateById(string ScanItemTemplateId)
        {
            Guid id = Guid.Parse(ScanItemTemplateId);

            var r = context!.ScanItemTemplates!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).FirstOrDefault();
            if (r != null)
            {
                context!.ScanItemTemplates!.Remove(r);
                context.SaveChanges();
            }

            return r;
        }

        public MScanItemTemplate? UpdateScanItemTemplateById(string actionId, MScanItemTemplate item)
        {
            Guid id = Guid.Parse(actionId);
            var result = context!.ScanItemTemplates!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.SerialPrefixDigit = item.SerialPrefixDigit;
                result.GeneratorCount = item.GeneratorCount;
                result.SerialDigit = item.SerialDigit;
                result.PinDigit = item.PinDigit;
                result.UrlTemplate = item.UrlTemplate;
                result.NotificationEmail = item.NotificationEmail;

                context!.SaveChanges();
            }

            return result!;
        }
    }
}