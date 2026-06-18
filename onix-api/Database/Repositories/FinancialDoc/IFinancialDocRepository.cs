using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IFinancialDocRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<bool> IsDocNoExist(string docNo);
    }
}
