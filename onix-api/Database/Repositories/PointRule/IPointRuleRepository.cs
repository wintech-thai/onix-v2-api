using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IPointRuleRepository
    {
        public void SetCustomOrgId(string customOrgId);
        
        public Task<MPointRule> AddPointRule(MPointRule pr);
        public Task<List<MPointRule>> GetPointRules(VMPointRule param);
        public Task<int> GetPointRuleCount(VMPointRule param);
        public Task<MPointRule?> UpdatePointRuleById(string pointRuleId, MPointRule pr);
        public Task<MPointRule?> GetPointRuleById(string pointRuleId);
        public Task<MPointRule?> DeletePointRuleById(string pointRuleId);
        public Task<MPointRule?> UpdatePointRuleStatusById(string pointRuleId, string status);
        public Task<bool> IsRuleNameExist(string ruleName);
        public Task<MPointRule?> GetPointRuleByName(string ruleName);
    }
}
