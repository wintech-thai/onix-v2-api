using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;
using System.Text.Json;

namespace Its.Onix.Api.Services
{
    public class FinancialDocService : BaseService, IFinancialDocService
    {
        private readonly IFinancialDocRepository? repository = null;

        public FinancialDocService(IFinancialDocRepository repo) : base()
        {
            repository = repo;
        }

        public async Task<MVFinalcialDoc> GetFinancialDocById(string orgId, string docId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVFinalcialDoc()
            {
                Status = "OK",
                Description = "Success",
            };

            //Implement here ....
            //เรียก Await function ในนี้ด้วย

            return r;
        }
    }
}
