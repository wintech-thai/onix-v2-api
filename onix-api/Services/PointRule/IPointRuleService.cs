using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IPointRuleService
    {
        public Task<MVPointRule> AddPointRule(string orgId, MPointRule pr);
        public Task<MVPointRule?> UpdatePointRuleById(string orgId, string pointRuleId, MPointRule pr);
        public Task<MVPointRule?> GetPointRuleById(string orgId, string pointRuleId);
        public Task<MVPointRule?> DeletePointRuleById(string orgId, string pointRuleId);
        public Task<List<MPointRule>> GetPointRules(string orgId, VMPointRule param);
        public Task<int> GetPointRulesCount(string orgId, VMPointRule param);
        public Task<MVPointRule?> UpdatePointRuleStatusById(string orgId, string pointRuleId, string status);
        public Task<PointRuleExecutionResult> EvaluatePointRuleById(string orgId, string pointRuleId, PointRuleInput ruleInput);
        public Task<PointRuleExecutionResult> EvaluatePointRules(string orgId, PointRuleInput ruleInput);
    }
}
