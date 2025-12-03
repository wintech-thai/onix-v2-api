using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace Its.Onix.Api.Database.Repositories
{
    public class PointRuleRepository : BaseRepository, IPointRuleRepository
    {
        public PointRuleRepository(IDataContext ctx)
        {
            context = ctx;
        }

        private ExpressionStarter<MPointRule> PointRulePredicate(VMPointRule param)
        {
            var pd = PredicateBuilder.New<MPointRule>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MPointRule>();
                fullTextPd = fullTextPd.Or(p => p.RuleName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            if ((param.RuleType != "") && (param.RuleType != null))
            {
                var ruleTypePd = PredicateBuilder.New<MPointRule>();
                ruleTypePd = ruleTypePd.Or(p => p.RuleType!.Equals(param.RuleType));

                pd = pd.And(ruleTypePd);
            }

            return pd;
        }

        public async Task<MPointRule> AddPointRule(MPointRule pr)
        {
            pr.OrgId = orgId;

            await context!.PointRules!.AddAsync(pr);
            await context.SaveChangesAsync();

            return pr;
        }

        public async Task<List<MPointRule>> GetPointRulesByTriggerEvent(string triggerEvent)
        {
            var result = await context!.PointRules!.Where(x => x.OrgId!.Equals(orgId) && x.TriggeredEvent!.Equals(triggerEvent))
                .OrderBy(e => e.Priority)
                .ThenBy(e => e.CreatedDate)
                .ToListAsync();

            return result;
        }

        public async Task<List<MPointRule>> GetPointRules(VMPointRule param)
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

            if (string.IsNullOrEmpty(param.RuleType))
            {
                param.RuleType = "PointRule";
            }

            var predicate = PointRulePredicate(param!);
            var result = await context!.PointRules!.Where(predicate)
                .OrderBy(e => e.Priority)
                .ThenBy(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            //มันใหญ่มากเลยไม่อยากให้ return RuleDefinition ออกไป
            foreach (var r in result)
            {
                r.RuleDefinition = "";
            }

            return result;
        }

        public async Task<int> GetPointRuleCount(VMPointRule param)
        {
            var predicate = PointRulePredicate(param!);
            var result = await context!.PointRules!.Where(predicate).CountAsync();

            return result;
        }

        public async Task<MPointRule?> UpdatePointRuleById(string pointRuleId, MPointRule pr)
        {
            Guid id = Guid.Parse(pointRuleId);
            var result = await context!.PointRules!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                result.Tags = pr.Tags;
                result.Description = pr.Description;
                result.RuleName = pr.RuleName;
                result.Priority = pr.Priority;
                result.StartDate = pr.StartDate;
                result.EndDate = pr.EndDate;
                result.RuleDefinition = pr.RuleDefinition;
                result.RuleType = pr.RuleType; //ตรงนี้ controller จะ set ค่าเข้ามาให้ว่าเป็น PointRule หรือ PriceRule
                result.UpdatedDate = DateTime.UtcNow;

                context!.SaveChanges();
            }

            return result!;
        }

        public async Task<MPointRule?> GetPointRuleById(string pointRuleId)
        {
            Guid id = Guid.Parse(pointRuleId);
            var u = await context!.PointRules!.Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public async Task<MPointRule?> DeletePointRuleById(string pointRuleId)
        {
            Guid id = Guid.Parse(pointRuleId);

            var r = await context!.PointRules!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).FirstOrDefaultAsync();
            if (r != null)
            {
                context!.PointRules!.Remove(r);
                await context.SaveChangesAsync();
            }

            return r;
        }

        public async Task<MPointRule?> UpdatePointRuleStatusById(string pointRuleId, string status)
        {
            Guid id = Guid.Parse(pointRuleId);
            var result = await context!.PointRules!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                result.Status = status;
                result.UpdatedDate = DateTime.UtcNow;

                context!.SaveChanges();
            }

            return result!;
        }

        public async Task<bool> IsRuleNameExist(string ruleName)
        {
            var result = await context!.PointRules!.Where(x => x.OrgId!.Equals(orgId) && x.RuleName!.Equals(ruleName)).FirstOrDefaultAsync();
            if (result != null)
            {
                //Exist
                return true;
            }

            return false;
        }

        public async Task<MPointRule?> GetPointRuleByName(string ruleName)
        {
            var result = await context!.PointRules!.Where(x => x.OrgId!.Equals(orgId) && x.RuleName!.Equals(ruleName)).FirstOrDefaultAsync();
            return result;
        }
    }
}
