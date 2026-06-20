using System.Text.Json;
using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;


namespace Its.Onix.Api.Services
{
    public interface IFinancialDocService
    {
        public Task<MVFinalcialDoc> GetFinancialDocById(string orgId, string docId);
        public Task<MVFinalcialDoc> AddFinancialDoc(string orgId, MFinancialDoc financialDoc);
        public Task<MVFinalcialDoc> UpdateFinancialDocById(string orgId, string docId, MFinancialDoc financialDoc);
        public Task<MVFinalcialDoc> DeleteFinancialDocById(string orgId, string docId);
        public Task<List<MFinancialDoc>> GetFinancialDocs(string orgId, VMFinancialDoc param);
        public Task<int> GetFinancialDocCount(string orgId, VMFinancialDoc param);
        public Task<RevenueSummary> CalculateRevenue(string orgId, DateTime fromDate, DateTime toDate);
    }
}
