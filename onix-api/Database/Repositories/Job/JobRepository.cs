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

        private ExpressionStarter<MJob> JobPredicate(VMJob param)
        {
            var pd = PredicateBuilder.New<MJob>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

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

            var u = context!.Jobs!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }
    }
}