using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public class JobRepository : BaseRepository, IJobRepository
    {
        public JobRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public MJob AddJob(MJob item)
        {
            item.Id = Guid.NewGuid();
            item.CreatedDate = DateTime.UtcNow;
            item.UpdatedDate = DateTime.UtcNow;
            item.OrgId = orgId;

            context!.Jobs!.Add(item);
            context.SaveChanges();

            return item;
        }

        private ExpressionStarter<MJob> IsOrgMatchPredicate(Guid? pmrId)
        {
            var pd = PredicateBuilder.New<MJob>(true);
            if (orgId != "global")
            {
                //ต้องเอา orgId มา where ด้วย
                var orgPd = PredicateBuilder.New<MJob>(true);
                orgPd = orgPd.And(p => p.OrgId!.Equals(orgId));
                pd = pd.And(orgPd);
            }

            if (pmrId != null)
            {
                //ต้องมีการเอา Id ของ payment ไปเช็คด้วย เพื่อดึงเฉพาะตัวนั้น ๆ ออกมา
                var pmrPd = PredicateBuilder.New<MJob>(true);
                pmrPd = pmrPd.And(p => p.Id!.Equals(pmrId));
                pd = pd.And(pmrPd);
            }

            return pd;
        }

        private ExpressionStarter<MJob> JobPredicate(VMJob param)
        {
            //var pd = PredicateBuilder.New<MJob>();
            //pd = pd.And(p => p.OrgId!.Equals(orgId));

            var pd = IsOrgMatchPredicate(null);

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MJob>();
                fullTextPd = fullTextPd.Or(p => p.Name!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Type!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            if ((param.JobType != null) && (param.JobType != ""))
            {
                var itemPd = PredicateBuilder.New<MJob>();
                itemPd = itemPd.Or(p => p.Type!.Equals(param.JobType));

                pd = pd.And(itemPd);
            }

            if ((param.ScanItemTemplateId != null) && (param.ScanItemTemplateId != ""))
            {
                var templatePd = PredicateBuilder.New<MJob>();
                templatePd = templatePd.Or(p => p.ScanItemTemplateId!.Equals(param.ScanItemTemplateId));

                pd = pd.And(templatePd);
            }

            if ((param.EventTypeSet != null) && param.EventTypeSet.Any())
            {
                var eventTypePd = PredicateBuilder.New<MJob>();
                eventTypePd = eventTypePd.Or(p => param.EventTypeSet.Contains(p.Type!));

                pd = pd.And(eventTypePd);
            }


            // FromDate
            if (param.FromDate.HasValue)
            {
                var fromDatePd = PredicateBuilder.New<MJob>();
                fromDatePd = fromDatePd.Or(p => p.CreatedDate >= param.FromDate.Value);

                pd = pd.And(fromDatePd);
            }

            // ToDate
            if (param.ToDate.HasValue)
            {
                var toDatePd = PredicateBuilder.New<MJob>();
                toDatePd = toDatePd.Or(p => p.CreatedDate <= param.ToDate.Value);

                pd = pd.And(toDatePd);
            }

            // Status
            if (!string.IsNullOrEmpty(param.Status))
            {
                var statusPd = PredicateBuilder.New<MJob>();
                statusPd = statusPd.Or(p => p.Status!.Equals(param.Status));

                pd = pd.And(statusPd);
            }

            if (!string.IsNullOrEmpty(param.RefId))
            {
                var refIdPd = PredicateBuilder.New<MJob>();
                refIdPd = refIdPd.Or(p => p.RefId!.Equals(param.RefId));

                pd = pd.And(refIdPd);
            }

            return pd;
        }

        public int GetJobCount(VMJob param)
        {
            var predicate = JobPredicate(param);
            var cnt = context!.Jobs!.Where(predicate).Count();

            return cnt;
        }

        public IEnumerable<MJob> GetJobs(VMJob param)
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

            var predicate = JobPredicate(param!);
            var arr = context!.Jobs!.Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }

        public MJob GetJobById(string itemId)
        {
            Guid id = Guid.Parse(itemId);

            var u = context!.Jobs!.Where(IsOrgMatchPredicate(id)).FirstOrDefault();
            return u!;
        }

        public MJob? DeleteJobById(string jobId)
        {
            Guid id = Guid.Parse(jobId);

            var r = context!.Jobs!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).FirstOrDefault();
            if (r != null)
            {
                context!.Jobs!.Remove(r);
                context.SaveChanges();
            }

            return r;
        }
    }
}