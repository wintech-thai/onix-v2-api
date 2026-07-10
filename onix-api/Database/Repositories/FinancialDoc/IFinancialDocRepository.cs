using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IFinancialDocRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<bool> IsDocNoExist(string docNo);
        public Task<MFinancialDoc> AddFinancialDoc(MFinancialDoc financialDoc, List<MFinancialDocItemExpense> expenseItems);
        public Task<MFinancialDoc?> GetFinancialDocById(string financialDocId);
        public Task<MFinancialDoc?> UpdateFinancialDocById(string financialDocId, MFinancialDoc financialDoc, List<MFinancialDocItemExpense> expenseItems);
        public Task<MFinancialDoc?> DeleteFinancialDocById(string financialDocId);
        public Task<List<MFinancialDoc>> GetFinancialDocs(VMFinancialDoc param);
        public Task<int> GetFinancialDocCount(VMFinancialDoc param);
    }
}
