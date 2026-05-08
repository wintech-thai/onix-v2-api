using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class AgentPolicyRepository : BaseRepository, IAgentPolicyRepository
    {
        public AgentPolicyRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<bool> IsAgentPolicyCodeExist(string code)
        {
            var exists = await context!.AgentPolicies!.AsExpandable().AnyAsync(p => p!.Code!.Equals(code) && p!.OrgId!.Equals(orgId));
            return exists;
        }

        public async Task<MAgentPolicy?> GetAgentPolicyByCode(string code)
        {
            var exists = await context!.AgentPolicies!.AsExpandable().Where(p => p!.Code!.Equals(code) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return exists;
        }

        public async Task<List<MAgentPolicy>> GetAgentPolicies(VMAgentPolicy param)
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

            var predicate = AgentPolicyPredicate(param!);
            var result = await GetSelection().AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.CreatedDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            return result;
        }

        public async Task<int> GetAgentPolicyCount(VMAgentPolicy param)
        {
            var predicate = AgentPolicyPredicate(param!);
            var result = await context!.AgentPolicies!.Where(predicate).AsExpandable().CountAsync();

            return result;
        }

        public async Task<MAgentPolicy?> GetAgentPolicyById(string agentId)
        {
            Guid id = Guid.Parse(agentId);
            var u = await GetSelection().AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public IQueryable<MAgentPolicy> GetSelection()
        {
            var query =
                from agentPolicy in context!.AgentPolicies
                select new { agentPolicy };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MAgentPolicy
            {
                Id = x.agentPolicy.Id,
                OrgId = x.agentPolicy.OrgId,
                Code = x.agentPolicy.Code,
                Description = x.agentPolicy.Description,
                PolicyDefinition = x.agentPolicy.PolicyDefinition,
                Tags = x.agentPolicy.Tags,
                CreatedDate = x.agentPolicy.CreatedDate,
            });
        }

        private ExpressionStarter<MAgentPolicy> AgentPolicyPredicate(VMAgentPolicy param)
        {
            var pd = PredicateBuilder.New<MAgentPolicy>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MAgentPolicy>();
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Code!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<MAgentPolicy> AddAgentPolicy(MAgentPolicy agentPolicy)
        {
            agentPolicy.OrgId = orgId;
            agentPolicy.CreatedDate = DateTime.UtcNow;

            await context!.AgentPolicies!.AddAsync(agentPolicy);
            await context.SaveChangesAsync();

            return agentPolicy;
        }

        public async Task<MAgentPolicy?> DeleteAgentPolicyById(string agentId)
        {
            Guid id = Guid.Parse(agentId);
            var existing = await context!.AgentPolicies!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                context.AgentPolicies!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<MAgentPolicy?> UpdateAgentPolicyById(string agentId, MAgentPolicy agentPolicy)
        {
            Guid id = Guid.Parse(agentId);
            var existing = await context!.AgentPolicies!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Code = agentPolicy.Code;
                existing.Description = agentPolicy.Description;
                existing.Tags = agentPolicy.Tags;
                existing.PolicyDefinition = agentPolicy.PolicyDefinition;
            }

            await context.SaveChangesAsync();
            return existing;
        }
    }
}