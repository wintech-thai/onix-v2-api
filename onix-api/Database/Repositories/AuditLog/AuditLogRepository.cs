using LinqKit;
using System.Data.Entity;
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

            if (param.FromDate.HasValue)
                pd = pd.And(p => p.CreatedDate >= param.FromDate);
            if (param.ToDate.HasValue)
                pd = pd.And(p => p.CreatedDate <= param.ToDate);

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

            if (param.FromDate.HasValue)
                pd = pd.And(p => p.CreatedDate >= param.FromDate);
            if (param.ToDate.HasValue)
                pd = pd.And(p => p.CreatedDate <= param.ToDate);

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

            if (!string.IsNullOrEmpty(param.FilterApi))
                pd = pd.And(p => p.ApiName!.Equals(param.FilterApi));
            if (!string.IsNullOrEmpty(param.FilterUser))
                pd = pd.And(p => p.UserName!.Equals(param.FilterUser));
            if (!string.IsNullOrEmpty(param.FilterIp))
                pd = pd.And(p => p.ClientIp!.Equals(param.FilterIp));
            if (param.FilterStatus.HasValue)
                pd = pd.And(p => p.StatusCode == param.FilterStatus);

            return pd;
        }

        // ── Org-scoped (merchant) ─────────────────────────────────────────────

        public async Task<int> GetAuditLogCount(VMAuditLog param)
        {
            return await context!.AuditLogs!.AsExpandable().Where(AuditLogPredicate(param)).CountAsync();
        }

        public async Task<IEnumerable<MAuditLog>> GetAuditLogs(VMAuditLog param)
        {
            var offset = param.Offset > 0 ? param.Offset - 1 : 0;
            var limit = param.Limit > 0 ? param.Limit : 100;

            return await context!.AuditLogs!
                .AsExpandable()
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

        public async Task<VMAuditLogAggregations> GetAuditLogAggregations(VMAuditLog param)
        {
            var pd = AuditLogPredicate(param);

            var items = await context!.AuditLogs!
                .AsExpandable()
                .Where(pd)
                .Select(e => new
                {
                    e.CreatedDate,
                    e.ApiName,
                    e.UserName,
                    e.ClientIp,
                    e.StatusCode,
                })
                .ToListAsync();

            var timeline = items
                .Where(e => e.CreatedDate.HasValue)
                .GroupBy(e => TruncateToInterval(e.CreatedDate!.Value, param.Interval))
                .OrderBy(g => g.Key)
                .Select(g => new VMAggBucket
                {
                    Key = new DateTimeOffset(g.Key).ToUnixTimeMilliseconds(),
                    KeyAsString = g.Key.ToString("O"),
                    DocCount = g.Count(),
                    GroupByApi = new VMAggBuckets
                    {
                        Buckets = (param.GroupBy?.ToLower() switch
                        {
                            "user" => g.Where(e => !string.IsNullOrEmpty(e.UserName))
                                       .GroupBy(e => e.UserName!),
                            "ip"   => g.Where(e => !string.IsNullOrEmpty(e.ClientIp))
                                       .GroupBy(e => e.ClientIp!),
                            "status" => g.Where(e => e.StatusCode.HasValue)
                                         .GroupBy(e => e.StatusCode!.Value.ToString()),
                            _      => g.Where(e => !string.IsNullOrEmpty(e.ApiName))
                                       .GroupBy(e => e.ApiName!),
                        })
                        .Select(ag => new { Key = ag.Key, Count = ag.Count() })
                        .OrderByDescending(x => x.Count)
                        .Take(10)
                        .Select(x => new VMAggBucket { Key = x.Key, DocCount = x.Count })
                        .ToList(),
                    },
                })
                .ToList();

            var byApi = items
                .Where(e => !string.IsNullOrEmpty(e.ApiName))
                .GroupBy(e => e.ApiName!)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).Take(10)
                .Select(x => new VMAggBucket { Key = x.Key, DocCount = x.Count })
                .ToList();

            var byUser = items
                .Where(e => !string.IsNullOrEmpty(e.UserName))
                .GroupBy(e => e.UserName!)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).Take(10)
                .Select(x => new VMAggBucket { Key = x.Key, DocCount = x.Count })
                .ToList();

            var byIp = items
                .Where(e => !string.IsNullOrEmpty(e.ClientIp))
                .GroupBy(e => e.ClientIp!)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).Take(10)
                .Select(x => new VMAggBucket { Key = x.Key, DocCount = x.Count })
                .ToList();

            var byStatus = items
                .Where(e => e.StatusCode.HasValue)
                .GroupBy(e => e.StatusCode!.Value)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).Take(10)
                .Select(x => new VMAggBucket { Key = x.Key.ToString(), DocCount = x.Count })
                .ToList();

            var bruteforce = items
                .Where(e => e.StatusCode == 401 && !string.IsNullOrEmpty(e.ClientIp))
                .GroupBy(e => e.ClientIp!)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).Take(10)
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

        // ── Admin (all orgs) ──────────────────────────────────────────────────

        public async Task<int> GetAllAuditLogCount(VMAuditLog param)
        {
            return await context!.AuditLogs!.AsExpandable().Where(AllAuditLogPredicate(param)).CountAsync();
        }

        public async Task<IEnumerable<MAuditLog>> GetAllAuditLogs(VMAuditLog param)
        {
            var offset = param.Offset > 0 ? param.Offset - 1 : 0;
            var limit = param.Limit > 0 ? param.Limit : 100;

            return await context!.AuditLogs!
                .AsExpandable()
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

        private static DateTime TruncateToInterval(DateTime dt, string? interval)
        {
            var match = System.Text.RegularExpressions.Regex.Match(interval ?? "1h", @"(\d+)([smhd])");
            if (!match.Success)
                return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, DateTimeKind.Utc);

            var val = int.Parse(match.Groups[1].Value);
            var unit = match.Groups[2].Value;

            return unit switch
            {
                "s" => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, (dt.Second / val) * val, DateTimeKind.Utc),
                "m" => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / val) * val, 0, DateTimeKind.Utc),
                "h" => new DateTime(dt.Year, dt.Month, dt.Day, (dt.Hour / val) * val, 0, 0, DateTimeKind.Utc),
                "d" => new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Utc),
                _ => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, DateTimeKind.Utc),
            };
        }

        public async Task<VMAuditLogAggregations> GetAllAuditLogAggregations(VMAuditLog param)
        {
            var pd = AllAuditLogPredicate(param);

            var items = await context!.AuditLogs!
                .AsExpandable()
                .Where(pd)
                .Select(e => new
                {
                    e.CreatedDate,
                    e.ApiName,
                    e.UserName,
                    e.ClientIp,
                    e.StatusCode,
                })
                .ToListAsync();

            var timeline = items
                .Where(e => e.CreatedDate.HasValue)
                .GroupBy(e => TruncateToInterval(e.CreatedDate!.Value, param.Interval))
                .OrderBy(g => g.Key)
                .Select(g => new VMAggBucket
                {
                    Key = new DateTimeOffset(g.Key).ToUnixTimeMilliseconds(),
                    KeyAsString = g.Key.ToString("O"),
                    DocCount = g.Count(),
                    GroupByApi = new VMAggBuckets
                    {
                        Buckets = (param.GroupBy?.ToLower() switch
                        {
                            "user" => g.Where(e => !string.IsNullOrEmpty(e.UserName))
                                       .GroupBy(e => e.UserName!),
                            "ip"   => g.Where(e => !string.IsNullOrEmpty(e.ClientIp))
                                       .GroupBy(e => e.ClientIp!),
                            "status" => g.Where(e => e.StatusCode.HasValue)
                                         .GroupBy(e => e.StatusCode!.Value.ToString()),
                            _      => g.Where(e => !string.IsNullOrEmpty(e.ApiName))
                                       .GroupBy(e => e.ApiName!),
                        })
                        .Select(ag => new { Key = ag.Key, Count = ag.Count() })
                        .OrderByDescending(x => x.Count)
                        .Take(10)
                        .Select(x => new VMAggBucket { Key = x.Key, DocCount = x.Count })
                        .ToList(),
                    },
                })
                .ToList();

            var byApi = items
                .Where(e => !string.IsNullOrEmpty(e.ApiName))
                .GroupBy(e => e.ApiName!)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).Take(10)
                .Select(x => new VMAggBucket { Key = x.Key, DocCount = x.Count })
                .ToList();

            var byUser = items
                .Where(e => !string.IsNullOrEmpty(e.UserName))
                .GroupBy(e => e.UserName!)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).Take(10)
                .Select(x => new VMAggBucket { Key = x.Key, DocCount = x.Count })
                .ToList();

            var byIp = items
                .Where(e => !string.IsNullOrEmpty(e.ClientIp))
                .GroupBy(e => e.ClientIp!)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).Take(10)
                .Select(x => new VMAggBucket { Key = x.Key, DocCount = x.Count })
                .ToList();

            var byStatus = items
                .Where(e => e.StatusCode.HasValue)
                .GroupBy(e => e.StatusCode!.Value)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).Take(10)
                .Select(x => new VMAggBucket { Key = x.Key.ToString(), DocCount = x.Count })
                .ToList();

            var bruteforce = items
                .Where(e => e.StatusCode == 401 && !string.IsNullOrEmpty(e.ClientIp))
                .GroupBy(e => e.ClientIp!)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).Take(10)
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
