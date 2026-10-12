using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;
using Microsoft.EntityFrameworkCore;

namespace Its.Onix.Api.Database.Repositories
{
    // NOTE: deliberately avoids LinqKit's .AsExpandable()/PredicateBuilder throughout this file.
    // In this project's LinqKit/EF Core combo, .AsExpandable() intermittently produces a query
    // whose .Provider does NOT implement IAsyncQueryProvider — confirmed broken even with a
    // single combined PredicateBuilder predicate (not just when chaining a second .Where()).
    // Plain sequential .Where() calls on a vanilla IQueryable<T> never leave EF Core's own
    // provider, so they're always safe for async. None of the queries here actually need
    // LinqKit's Expression-combining power (just a couple of ANDed/ORed conditions) — use plain
    // LINQ instead of reaching for PredicateBuilder again in this file.
    public class AgentRepository : BaseRepository, IAgentRepository
    {
        public AgentRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<bool> IsAgentCodeExist(string code)
        {
            IQueryable<MAgent> query = context!.Agents!.Where(p => p.Code!.Equals(code));
            if (orgId != "global")
            {
                query = query.Where(p => p.OrgId!.Equals(orgId));
            }
            return await query.AnyAsync();
        }

        public async Task<MAgent?> GetAgentByCode(string code)
        {
            IQueryable<MAgent> query = context!.Agents!.Where(p => p.Code!.Equals(code));
            if (orgId != "global")
            {
                query = query.Where(p => p.OrgId!.Equals(orgId));
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<MAgent>> GetAgents(VMAgent param)
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

            var query = ApplyAgentFilters(GetSelection(), param);
            var result = await query
            .OrderByDescending(e => e.LastSeenDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            return result;
        }

        public async Task<int> GetAgentCount(VMAgent param)
        {
            var query = ApplyAgentFilters(context!.Agents!, param);
            var result = await query.CountAsync();

            return result;
        }

        public async Task<MAgent?> GetAgentById(string agentId)
        {
            Guid id = Guid.Parse(agentId);
            IQueryable<MAgent> query = GetSelection().Where(p => p.Id!.Equals(id));
            if (orgId != "global")
            {
                query = query.Where(p => p.OrgId!.Equals(orgId));
            }
            var u = await query.FirstOrDefaultAsync();
            return u;
        }

        public IQueryable<MAgent> GetSelection()
        {
            var query =
                from agent in context!.Agents
                select new { agent };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MAgent
            {
                Id = x.agent.Id,
                OrgId = x.agent.OrgId,
                Code = x.agent.Code,
                Description = x.agent.Description,
                Tags = x.agent.Tags,
                AgentType = x.agent.AgentType,
                ApiKeyId = x.agent.ApiKeyId,
                CreatedDate = x.agent.CreatedDate,
                LastSeenDate = x.agent.LastSeenDate,
                BankAccountsSelected = x.agent.BankAccountsSelected,
                AgentConfig = x.agent.AgentConfig,
                AgentStatus = x.agent.AgentStatus,
            });
        }

        private IQueryable<MAgent> ApplyAgentFilters(IQueryable<MAgent> query, VMAgent param)
        {
            if (orgId != "global")
            {
                query = query.Where(p => p.OrgId!.Equals(orgId));
            }

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fts = param.FullTextSearch;
                query = query.Where(p => p.Description!.Contains(fts) || p.Code!.Contains(fts) || p.Tags!.Contains(fts));
            }

            if ((param.AgentType != "") && (param.AgentType != null))
            {
                query = query.Where(p => p.AgentType!.Equals(param.AgentType));
            }

            return query;
        }

        public async Task<MAgent> AddAgent(MAgent agent)
        {
            agent.OrgId = orgId;
            agent.CreatedDate = DateTime.UtcNow;

            await context!.Agents!.AddAsync(agent);
            await context.SaveChangesAsync();

            return agent;
        }

        public async Task<MAgent?> DeleteAgentById(string agentId)
        {
            Guid id = Guid.Parse(agentId);
            IQueryable<MAgent> query = context!.Agents!.Where(p => p.Id!.Equals(id));
            if (orgId != "global")
            {
                query = query.Where(p => p.OrgId!.Equals(orgId));
            }
            var existing = await query.FirstOrDefaultAsync();
            if (existing != null)
            {
                context.Agents!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<MAgent?> UpdateAgentById(string agentId, MAgent agent)
        {
            Guid id = Guid.Parse(agentId);
            IQueryable<MAgent> query = context!.Agents!.Where(p => p.Id!.Equals(id));
            if (orgId != "global")
            {
                query = query.Where(p => p.OrgId!.Equals(orgId));
            }
            var existing = await query.FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Code = agent.Code;
                existing.Description = agent.Description;
                existing.Tags = agent.Tags;
                existing.BankAccountsSelected = agent.BankAccountsSelected;
                existing.AgentConfig = agent.AgentConfig;
                //existing.AgentStatus = agent.AgentStatus;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        private async Task<MAgent?> UpdateAgentLastSeenById(string agentId, int? errorCcount)
        {
            if (errorCcount == null)
            {
                errorCcount = 0;
            }

            Guid id = Guid.Parse(agentId);
            IQueryable<MAgent> query = context!.Agents!.Where(p => p.Id!.Equals(id));
            if (orgId != "global")
            {
                query = query.Where(p => p.OrgId!.Equals(orgId));
            }
            var existing = await query.FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.LastSeenDate = DateTime.UtcNow;
                if (errorCcount > 0)
                {
                    existing.LastSeenErrorDate = existing.LastSeenDate;
                }
                else
                {
                    existing.LastSeenErrorDate = null;
                }
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MAgentEvent> AddAgentEvent(MAgentEvent agentEvent)
        {
            agentEvent.OrgId = orgId;
            agentEvent.CreatedDate = DateTime.UtcNow;

            await context!.AgentEvents!.AddAsync(agentEvent);
            await UpdateAgentLastSeenById(agentEvent.AgentId!, agentEvent.ErrorCount);

            await context.SaveChangesAsync();

            return agentEvent;
        }

        private IQueryable<MAgentEvent> ApplyAgentEventFilters(IQueryable<MAgentEvent> query, VMAgentEvent param)
        {
            if (orgId != "global")
            {
                query = query.Where(p => p.OrgId!.Equals(orgId));
            }

            if ((param.AgentId != null) && (param.AgentId != ""))
            {
                query = query.Where(p => p.AgentId!.Equals(param.AgentId));
            }

            if ((param.EventType != null) && (param.EventType != ""))
            {
                query = query.Where(p => p.EventType!.Equals(param.EventType));
            }

            if ((param.Channel != null) && (param.Channel != ""))
            {
                query = query.Where(p => p.Channel!.Equals(param.Channel));
            }

            if (param.FromDate.HasValue)
            {
                var fromDate = param.FromDate.Value;
                query = query.Where(p => p.CreatedDate >= fromDate);
            }

            if (param.ToDate.HasValue)
            {
                var toDate = param.ToDate.Value;
                query = query.Where(p => p.CreatedDate <= toDate);
            }

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fts = param.FullTextSearch;
                query = query.Where(p => p.Tags!.Contains(fts));
            }

            return query;
        }

        public async Task<int> GetAgentEventCount(VMAgentEvent param)
        {
            var query = ApplyAgentEventFilters(context!.AgentEvents!, param);
            var result = await query.CountAsync();

            return result;
        }

        public IQueryable<MAgentEvent> GetSelection2()
        {
            var query =
                from agent in context!.AgentEvents
                select new { agent };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MAgentEvent
            {
                Id = x.agent.Id,
                OrgId = x.agent.OrgId,
                AgentId = x.agent.AgentId,
                RawData = x.agent.RawData,
                Tags = x.agent.Tags,
                EventType = x.agent.EventType,
                Channel = x.agent.Channel,
                CreatedDate = x.agent.CreatedDate,
                Status = x.agent.Status,
                StatusDesc = x.agent.StatusDesc,
            });
        }

        public async Task<List<MAgentEvent>> GetAgentEvents(VMAgentEvent param)
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

            var query = ApplyAgentEventFilters(GetSelection2(), param);
            var result = await query
            .OrderByDescending(e => e.CreatedDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            return result;
        }

        public async Task<MAgentEvent?> GetAgentEventById(string agentEventId)
        {
            Guid id = Guid.Parse(agentEventId);
            IQueryable<MAgentEvent> query = GetSelection2().Where(p => p.Id!.Equals(id));
            if (orgId != "global")
            {
                query = query.Where(p => p.OrgId!.Equals(orgId));
            }
            var u = await query.FirstOrDefaultAsync();
            return u;
        }

        public async Task<List<MVAgentEventTimeSeries>> GetAgentEventTimeSeries(VMAgentEvent param)
        {
            var query = ApplyAgentEventFilters(GetSelection2(), param);
            var events = await query
                .OrderBy(e => e.CreatedDate)
                .ToListAsync();

            var rangeHours = 24.0;
            if (param.FromDate.HasValue && param.ToDate.HasValue)
            {
                rangeHours = (param.ToDate.Value - param.FromDate.Value).TotalHours;
            }

            string BucketKey(DateTime dt)
            {
                if (rangeHours <= 2)
                    return dt.ToString("yyyy-MM-ddTHH:mm:00");
                if (rangeHours <= 48)
                    return dt.ToString("yyyy-MM-ddTHH:00:00");
                return dt.ToString("yyyy-MM-ddT00:00:00");
            }

            var grouped = events
                .Where(e => e.CreatedDate.HasValue)
                .GroupBy(e => new {
                    Time = BucketKey(e.CreatedDate!.Value),
                    EventType = e.EventType ?? ""
                })
                .Select(g => new MVAgentEventTimeSeries
                {
                    Time = g.Key.Time,
                    EventType = g.Key.EventType,
                    Count = g.Count(),
                })
                .OrderBy(x => x.Time)
                .ToList();

            return grouped;
        }

        public async Task<MAgent?> UpdateAgentStatusById(string agentId, string status)
        {
            Guid id = Guid.Parse(agentId);
            IQueryable<MAgent> query = context!.Agents!.Where(p => p.Id!.Equals(id));
            if (orgId != "global")
            {
                query = query.Where(p => p.OrgId!.Equals(orgId));
            }
            var result = await query.FirstOrDefaultAsync();

            if (result != null)
            {
                result.AgentStatus = status;
                await context!.SaveChangesAsync();
            }

            return result!;
        }
    }
}
