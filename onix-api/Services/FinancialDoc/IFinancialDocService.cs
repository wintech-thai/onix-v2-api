using System.Text.Json;
using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;


namespace Its.Onix.Api.Services
{
    public interface IFinancialDocService
    {
        public Task<MVFinalcialDoc> GetFinancialDocById(string orgId, string docId);
    }
}
