using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class RiskPolicyRepository : BaseRepository, IRiskPolicyRepository
    {
        public RiskPolicyRepository(IDataContext ctx)
        {
            context = ctx;
        }

        private ExpressionStarter<MRiskPolicy> RiskPolicyPredicate(VMRiskPolicy param)
        {
            var pd = PredicateBuilder.New<MRiskPolicy>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MRiskPolicy>();
                fullTextPd = fullTextPd.Or(p => p.Name!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            if (!string.IsNullOrEmpty(param.Status))
            {
                pd = pd.And(p => p.Status!.Equals(param.Status));
            }

            if (param.FromDate.HasValue)
            {
                pd = pd.And(p => p.CreatedDate >= param.FromDate);
            }

            if (param.ToDate.HasValue)
            {
                pd = pd.And(p => p.CreatedDate <= param.ToDate);
            }

            return pd;
        }

        public async Task<MRiskPolicy> AddRiskPolicyV2(MRiskPolicy riskPolicy)
        {
            riskPolicy.Id = Guid.NewGuid();
            riskPolicy.OrgId = orgId;
            riskPolicy.CreatedDate = DateTime.UtcNow;

            await context!.RiskPolicies!.AddAsync(riskPolicy);
            await context.SaveChangesAsync();

            return riskPolicy;
        }

        public async Task<int> GetRiskPolicyCountV2(VMRiskPolicy param)
        {
            var predicate = RiskPolicyPredicate(param);
            var cnt = await context!.RiskPolicies!.Where(predicate).AsExpandable().CountAsync();

            return cnt;
        }

        public async Task<List<MRiskPolicy>> GetRiskPoliciesV2(VMRiskPolicy param)
        {
            var limit = 0;
            var offset = 0;

            if (param.Offset > 0)
            {
                offset = param.Offset - 1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var predicate = RiskPolicyPredicate(param);
            var arr = await context!.RiskPolicies!.AsExpandable().Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return arr;
        }

        public async Task<MRiskPolicy?> GetRiskPolicyByIdV2(string riskPolicyId)
        {
            Guid id = Guid.Parse(riskPolicyId);

            var u = await context!.RiskPolicies!.AsExpandable().Where(p => p.Id!.Equals(id) && p.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public async Task<MRiskPolicy?> DeleteRiskPolicyByIdV2(string riskPolicyId)
        {
            Guid id = Guid.Parse(riskPolicyId);

            var r = await context!.RiskPolicies!.AsExpandable().Where(p => p.OrgId!.Equals(orgId) && p.Id!.Equals(id)).FirstOrDefaultAsync();
            if (r != null)
            {
                context!.RiskPolicies!.Remove(r);
                await context.SaveChangesAsync();
            }

            return r;
        }

        public async Task<MRiskPolicy?> UpdateRiskPolicyByIdV2(string riskPolicyId, MRiskPolicy riskPolicy)
        {
            Guid id = Guid.Parse(riskPolicyId);
            var result = await context!.RiskPolicies!.AsExpandable().Where(p => p.OrgId!.Equals(orgId) && p.Id!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                result.Name = riskPolicy.Name;
                result.Description = riskPolicy.Description;
                result.Tags = riskPolicy.Tags;
                result.AllowBlankPayerName = riskPolicy.AllowBlankPayerName;
                result.AllowUnknownPayerName = riskPolicy.AllowUnknownPayerName;
                result.AllowSuspiciousPayerName = riskPolicy.AllowSuspiciousPayerName;
                result.AllowMaliciousPayerName = riskPolicy.AllowMaliciousPayerName;
                await context.SaveChangesAsync();
            }

            return result;
        }

        public async Task<MRiskPolicy?> UpdateRiskPolicyStatusByIdV2(string riskPolicyId, string status)
        {
            Guid id = Guid.Parse(riskPolicyId);
            var result = await context!.RiskPolicies!.AsExpandable().Where(p => p.OrgId!.Equals(orgId) && p.Id!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                result.Status = status;
                await context.SaveChangesAsync();
            }

            return result;
        }
    }
}
