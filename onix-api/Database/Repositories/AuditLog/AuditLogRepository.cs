using LinqKit;
using Microsoft.EntityFrameworkCore;
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

            if (param.fromDate.HasValue)
                pd = pd.And(p => p.CreatedDate >= param.fromDate);
            if (param.toDate.HasValue)
                pd = pd.And(p => p.CreatedDate <= param.toDate);

            if (!string.IsNullOrEmpty(param.FullTextSearch))
            {
                var fts = PredicateBuilder.New<MAuditLog>();
                fts = fts.Or(p => p.ClientIp!.Contains(param.FullTextSearch));
                fts = fts.Or(p => p.Role!.Contains(param.FullTextSearch));
                fts = fts.Or(p => p.UserAgent!.Contains(param.FullTextSearch));
                fts = fts.Or(p => p.CustomDesc!.Contains(param.FullTextSearch));
                fts = fts.Or(p => p.CustomStatus!.Contains(param.FullTextSearch));
                fts = fts.Or(p => p.HttpMethod!.Contains(param.FullTextSearch));
                fts = fts.Or(p => p.Host!.Contains(param.FullTextSearch));
                fts = fts.Or(p => p.Path!.Contains(param.FullTextSearch));
                fts = fts.Or(p => p.QueryString!.Contains(param.FullTextSearch));
                fts = fts.Or(p => p.IdentityType!.Contains(param.FullTextSearch));
                fts = fts.Or(p => p.UserName!.Contains(param.FullTextSearch));
                pd = pd.And(fts);
            }

            return pd;
        }

        private ExpressionStarter<MAuditLog> AllAuditLogPredicate(VMAuditLog param)
        {
            var pd = PredicateBuilder.New<MAuditLog>(true);

            if (param.OrgIds != null && param.OrgIds.Count > 0)
                pd = pd.And(p => param.OrgIds.Contains(p.OrgId!));

            if (param.fromDate.HasValue)
                pd = pd.And(p => p.CreatedDate >= param.fromDate);
            if (param.toDate.HasValue)
                pd = pd.And(p => p.CreatedDate <= param.toDate);

            if (!string.IsNullOrEmpty(param.FullTextSearch))
            {
                var fts = PredicateBuilder.New<MAuditLog>();
                fts = fts.Or(p => p.ClientIp!.Contains(param.FullTextSearch));
                fts = fts.Or(p => p.Role!.Contains(param.FullTextSearch));
                fts = fts.Or(p => p.UserAgent!.Contains(param.FullTextSearch));
                fts = fts.Or(p => p.CustomDesc!.Contains(param.FullTextSearch));
                fts = fts.Or(p => p.CustomStatus!.Contains(param.FullTextSearch));
                fts = fts.Or(p => p.HttpMethod!.Contains(param.FullTextSearch));
                fts = fts.Or(p => p.Host!.Contains(param.FullTextSearch));
                fts = fts.Or(p => p.Path!.Contains(param.FullTextSearch));
                fts = fts.Or(p => p.QueryString!.Contains(param.FullTextSearch));
                fts = fts.Or(p => p.IdentityType!.Contains(param.FullTextSearch));
                fts = fts.Or(p => p.UserName!.Contains(param.FullTextSearch));
                pd = pd.And(fts);
            }

            return pd;
        }

        // ── Org-scoped (merchant) ─────────────────────────────────────────────

        public async Task<int> GetAuditLogCount(VMAuditLog param)
        {
            return await context!.AuditLogs!.Where(AuditLogPredicate(param)).CountAsync();
        }

        public async Task<IEnumerable<MAuditLog>> GetAuditLogs(VMAuditLog param)
        {
            var offset = param.Offset > 0 ? param.Offset - 1 : 0;
            var limit = param.Limit > 0 ? param.Limit : 100;

            return await context!.AuditLogs!
                .Where(AuditLogPredicate(param))
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<MAuditLog> GetAuditLogById(string auditLogId)
        {
            var id = Guid.Parse(auditLogId);
            return (await context!.AuditLogs!
                .Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId))
                .FirstOrDefaultAsync())!;
        }

        // ── Admin (all orgs) ──────────────────────────────────────────────────

        public async Task<int> GetAllAuditLogCount(VMAuditLog param)
        {
            return await context!.AuditLogs!.Where(AllAuditLogPredicate(param)).CountAsync();
        }

        public async Task<IEnumerable<MAuditLog>> GetAllAuditLogs(VMAuditLog param)
        {
            var offset = param.Offset > 0 ? param.Offset - 1 : 0;
            var limit = param.Limit > 0 ? param.Limit : 100;

            return await context!.AuditLogs!
                .Where(AllAuditLogPredicate(param))
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<MAuditLog> GetAllAuditLogById(string auditLogId)
        {
            var id = Guid.Parse(auditLogId);
            return (await context!.AuditLogs!
                .Where(p => p!.Id!.Equals(id))
                .FirstOrDefaultAsync())!;
        }

        public async Task<VMAuditLogAggregations> GetAllAuditLogAggregations(VMAuditLog param)
        {
            var pd = AllAuditLogPredicate(param);
            var q = context!.AuditLogs!.Where(pd);

            // Timeline: group by hour
            var timelineRaw = await q
                .GroupBy(e => new
                {
                    e.CreatedDate!.Value.Year,
                    e.CreatedDate.Value.Month,
                    e.CreatedDate.Value.Day,
                    e.CreatedDate.Value.Hour,
                })
                .Select(g => new { g.Key, Count = g.Count() })
                .OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month)
                    .ThenBy(x => x.Key.Day).ThenBy(x => x.Key.Hour)
                .ToListAsync();

            var timeline = timelineRaw.Select(b =>
            {
                var dt = new DateTime(b.Key.Year, b.Key.Month, b.Key.Day, b.Key.Hour, 0, 0, DateTimeKind.Utc);
                return new VMAggBucket
                {
                    Key = new DateTimeOffset(dt).ToUnixTimeMilliseconds(),
                    KeyAsString = dt.ToString("O"),
                    DocCount = b.Count,
                };
            }).ToList();

            // Top-N by API name
            var byApi = (await q
                .Where(e => e.ApiName != null && e.ApiName != "")
                .GroupBy(e => e.ApiName)
                .Select(g => new { Key = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync())
                .Select(x => new VMAggBucket { Key = x.Key, DocCount = x.Count })
                .ToList();

            // Top-N by user
            var byUser = (await q
                .Where(e => e.UserName != null && e.UserName != "")
                .GroupBy(e => e.UserName)
                .Select(g => new { Key = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync())
                .Select(x => new VMAggBucket { Key = x.Key, DocCount = x.Count })
                .ToList();

            // Top-N by IP
            var byIp = (await q
                .Where(e => e.ClientIp != null && e.ClientIp != "")
                .GroupBy(e => e.ClientIp)
                .Select(g => new { Key = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync())
                .Select(x => new VMAggBucket { Key = x.Key, DocCount = x.Count })
                .ToList();

            // Top-N by status code
            var byStatus = (await q
                .Where(e => e.StatusCode.HasValue)
                .GroupBy(e => e.StatusCode)
                .Select(g => new { Key = g.Key!.Value, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync())
                .Select(x => new VMAggBucket { Key = x.Key.ToString(), DocCount = x.Count })
                .ToList();

            // Bruteforce: 401 by IP
            var bruteforce = (await q
                .Where(e => e.StatusCode == 401 && e.ClientIp != null && e.ClientIp != "")
                .GroupBy(e => e.ClientIp)
                .Select(g => new { Key = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync())
                .Select(x => new VMAggBucket { Key = x.Key, DocCount = x.Count })
                .ToList();

            return new VMAuditLogAggregations
            {
                Timeline = new VMAggBuckets { Buckets = timeline },
                ByApi = new VMAggBuckets { Buckets = byApi },
                ByUser = new VMAggBuckets { Buckets = byUser },
                ByIp = new VMAggBuckets { Buckets = byIp },
                ByStatus = new VMAggBuckets { Buckets = byStatus },
                Bruteforce = new VMBruteforceAgg { ByIp = new VMAggBuckets { Buckets = bruteforce } },
            };
        }
    }
}
