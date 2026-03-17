using LinqKit;
using Its.Onix.Api.Models;
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
                CreatedDate = x.agent.CreatedDate,
                LastSeenDate = x.agent.LastSeenDate,
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
            }

            await context.SaveChangesAsync();
            return existing;
        }
    }
}