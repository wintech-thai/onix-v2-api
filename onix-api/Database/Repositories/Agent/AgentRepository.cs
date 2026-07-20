using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class AgentRepository : BaseRepository, IAgentRepository
    {
        public AgentRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<bool> IsAgentCodeExist(string code)
        {
            var exists = await context!.Agents!.AsExpandable().AnyAsync(p => p!.Code!.Equals(code) && p!.OrgId!.Equals(orgId));
            return exists;
        }

        public async Task<MAgent?> GetAgentByCode(string code)
        {
            var exists = await context!.Agents!.AsExpandable().Where(p => p!.Code!.Equals(code) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return exists;
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

            var predicate = AgentPredicate(param!);
            var result = await GetSelection().AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.LastSeenDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            return result;
        }

        public async Task<int> GetAgentCount(VMAgent param)
        {
            var predicate = AgentPredicate(param!);
            var result = await context!.Agents!.Where(predicate).AsExpandable().CountAsync();

            return result;
        }

        public async Task<MAgent?> GetAgentById(string agentId)
        {
            Guid id = Guid.Parse(agentId);
            var u = await GetSelection().AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
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
                ApiKeyId = x.agent.ApiKeyId,
                CreatedDate = x.agent.CreatedDate,
                LastSeenDate = x.agent.LastSeenDate,
                BankAccountsSelected = x.agent.BankAccountsSelected,
                AgentConfig = x.agent.AgentConfig,
            });
        }

        private ExpressionStarter<MAgent> AgentPredicate(VMAgent param)
        {
            var pd = PredicateBuilder.New<MAgent>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MAgent>();
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Code!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            if ((param.AgentType != "") && (param.AgentType != null))
            {
                var agentTypePd = PredicateBuilder.New<MAgent>();
                agentTypePd = agentTypePd.Or(p => p.AgentType!.Equals(param.AgentType));

                pd = pd.And(agentTypePd);
            }

            return pd;
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
            var existing = await context!.Agents!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
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
            var existing = await context!.Agents!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Code = agent.Code;
                existing.Description = agent.Description;
                existing.Tags = agent.Tags;
                existing.BankAccountsSelected = agent.BankAccountsSelected;
                existing.AgentConfig = agent.AgentConfig;
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
            var existing = await context!.Agents!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
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

        private ExpressionStarter<MAgentEvent> AgentEventPredicate(VMAgentEvent param)
        {
            var pd = PredicateBuilder.New<MAgentEvent>(true);

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.AgentId != null) && (param.AgentId != ""))
            {
                var agentIdPd = PredicateBuilder.New<MAgentEvent>();
                agentIdPd = agentIdPd.Or(p => p.AgentId!.Equals(param.AgentId));

                pd = pd.And(agentIdPd);
            }

            if ((param.EventType != null) && (param.EventType != ""))
            {
                var eventTypePd = PredicateBuilder.New<MAgentEvent>();
                eventTypePd = eventTypePd.Or(p => p.EventType!.Equals(param.EventType));

                pd = pd.And(eventTypePd);
            }

            if ((param.Channel != null) && (param.Channel != ""))
            {
                var channelPd = PredicateBuilder.New<MAgentEvent>();
                channelPd = channelPd.Or(p => p.Channel!.Equals(param.Channel));

                pd = pd.And(channelPd);
            }

            if (param.FromDate.HasValue)
            {
                var fromDatePd = PredicateBuilder.New<MAgentEvent>();
                fromDatePd = fromDatePd.Or(p => p.CreatedDate >= param.FromDate.Value);

                pd = pd.And(fromDatePd);
            }

            // ToDate
            if (param.ToDate.HasValue)
            {
                var toDatePd = PredicateBuilder.New<MAgentEvent>();
                toDatePd = toDatePd.Or(p => p.CreatedDate <= param.ToDate.Value);

                pd = pd.And(toDatePd);
            }

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MAgentEvent>();
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<int> GetAgentEventCount(VMAgentEvent param)
        {
            var predicate = AgentEventPredicate(param!);
            var result = await context!.AgentEvents!.Where(predicate).AsExpandable().CountAsync();

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

            var predicate = AgentEventPredicate(param!);
            var result = await GetSelection2().AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.CreatedDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            return result;
        }

        public async Task<MAgentEvent?> GetAgentEventById(string agentEventId)
        {
            Guid id = Guid.Parse(agentEventId);
            var u = await GetSelection2().AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public async Task<List<MVAgentEventTimeSeries>> GetAgentEventTimeSeries(VMAgentEvent param)
        {
            var predicate = AgentEventPredicate(param!);
            var events = await GetSelection2().AsExpandable()
                .Where(predicate)
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
    }
}