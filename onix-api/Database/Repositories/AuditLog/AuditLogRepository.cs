using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public class AuditLogRepository : BaseRepository, IAuditLogRepository
    {
        public AuditLogRepository(IDataContext ctx)
        {
            context = ctx;
        }

        private ExpressionStarter<MAuditLog> AuditLogPredicate(VMAuditLog param)
        {
            var pd = PredicateBuilder.New<MAuditLog>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MAuditLog>();
                fullTextPd = fullTextPd.Or(p => p.ClientIp!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Role!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.UserAgent!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.CustomDesc!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.CustomStatus!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.HttpMethod!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Host!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Path!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.QueryString!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.IdentityType!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.UserName!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public int GetAuditLogCount(VMAuditLog param)
        {
            var predicate = AuditLogPredicate(param);
            var cnt = context!.AuditLogs!.Where(predicate).Count();

            return cnt;
        }

        public IEnumerable<MAuditLog> GetAuditLogs(VMAuditLog param)
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

            var predicate = AuditLogPredicate(param!);
            var arr = context!.AuditLogs!.Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }

        public MAuditLog GetAuditLogById(string auditLogId)
        {
            Guid id = Guid.Parse(auditLogId);

            var u = context!.AuditLogs!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefault();
            return u!;
        }
    }
}