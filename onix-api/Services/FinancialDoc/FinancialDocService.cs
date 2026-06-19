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
        private readonly ISummaryService _summarySvc;

        public FinancialDocService(IFinancialDocRepository repo, ISummaryService summarySvc) : base()
        {
            repository = repo;
            _summarySvc = summarySvc;
        }

        private static string SerializeItems(List<MFinancialDocItem>? items)
        {
            if (items == null)
            {
                return "[]";
            }

            return JsonSerializer.Serialize(items);
        }

        private static List<MFinancialDocItem> DeserializeItems(string? json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<List<MFinancialDocItem>>(json) ?? [];
            }
            catch
            {
                return [];
            }
        }

        private static void DeserializeAllItems(MFinancialDoc doc)
        {
            doc.ExpenseItemsArr = DeserializeItems(doc.ExpenseItemsDefinition);
            doc.RevenueItemsArr = DeserializeItems(doc.RevenueItemsDefinition);
            doc.SharingItemsArr = DeserializeItems(doc.SharingItemsDefinition);
        }

        // เคลียร์ raw JSON string ทิ้งก่อนส่งกลับ ไม่ให้ response มีทั้ง raw string และ parsed array ซ้ำซ้อนกัน
        // (pattern เดียวกับ AgentService.GetAgentById() ที่เคลียร์ BankAccountsSelected ทิ้งหลัง deserialize)
        private static void ClearDefinitionFields(MFinancialDoc doc)
        {
            doc.ExpenseItemsDefinition = "";
            doc.RevenueItemsDefinition = "";
            doc.SharingItemsDefinition = "";
        }

        private static void RecalculateTotals(MFinancialDoc doc)
        {
            var totalRevenue = doc.RevenueItemsArr?.Sum(x => x.Amount ?? 0) ?? 0;
            var totalExpense = doc.ExpenseItemsArr?.Sum(x => x.Amount ?? 0) ?? 0;

            doc.TotalRevenue = totalRevenue;
            doc.TotalExpense = totalExpense;
            doc.ProfitLoss = totalRevenue - totalExpense;
        }

        private static string GenerateDocumentNo()
        {
            var now = DateTime.UtcNow;
            return $"FD-{now:yyyyMMdd}-{now:HHmm}";
        }

        public async Task<MVFinalcialDoc> GetFinancialDocById(string orgId, string docId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVFinalcialDoc()
            {
                Status = "OK",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(docId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Financial Doc ID [{docId}] format is invalid";

                return r;
            }

            var result = await repository!.GetFinancialDocById(docId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Financial Doc ID [{docId}] not found for the organization [{orgId}]";

                return r;
            }

            DeserializeAllItems(result);
            ClearDefinitionFields(result);

            r.FinancialDoc = result;
            return r;
        }

        public async Task<MVFinalcialDoc> AddFinancialDoc(string orgId, MFinancialDoc financialDoc)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVFinalcialDoc()
            {
                Status = "OK",
                Description = "Success",
            };

            //Auto-generate DocumentNo ถ้ายังไม่ได้ระบุมาจาก request
            var docNo = financialDoc.DocumentNo;
            if (string.IsNullOrEmpty(docNo))
            {
                docNo = GenerateDocumentNo();
            }

            if (await repository!.IsDocNoExist(docNo))
            {
                r.Status = "DOCUMENT_NO_DUPLICATE";
                r.Description = $"Document No [{docNo}] already exists";

                return r;
            }

            financialDoc.DocumentNo = docNo;
            financialDoc.ExpenseItemsDefinition = SerializeItems(financialDoc.ExpenseItemsArr);
            financialDoc.RevenueItemsDefinition = SerializeItems(financialDoc.RevenueItemsArr);
            financialDoc.SharingItemsDefinition = SerializeItems(financialDoc.SharingItemsArr);

            RecalculateTotals(financialDoc);

            var result = await repository!.AddFinancialDoc(financialDoc);
            ClearDefinitionFields(result);

            r.FinancialDoc = result;
            return r;
        }

        public async Task<MVFinalcialDoc> UpdateFinancialDocById(string orgId, string docId, MFinancialDoc financialDoc)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVFinalcialDoc()
            {
                Status = "OK",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(docId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Financial Doc ID [{docId}] format is invalid";

                return r;
            }

            //Not allow to update DocumentNo
            financialDoc.ExpenseItemsDefinition = SerializeItems(financialDoc.ExpenseItemsArr);
            financialDoc.RevenueItemsDefinition = SerializeItems(financialDoc.RevenueItemsArr);
            financialDoc.SharingItemsDefinition = SerializeItems(financialDoc.SharingItemsArr);

            RecalculateTotals(financialDoc);

            var result = await repository!.UpdateFinancialDocById(docId, financialDoc);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Financial Doc ID [{docId}] not found for the organization [{orgId}]";

                return r;
            }

            DeserializeAllItems(result);
            ClearDefinitionFields(result);

            r.FinancialDoc = result;
            return r;
        }

        public async Task<MVFinalcialDoc> DeleteFinancialDocById(string orgId, string docId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVFinalcialDoc()
            {
                Status = "OK",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(docId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Financial Doc ID [{docId}] format is invalid";

                return r;
            }

            var m = await repository!.DeleteFinancialDocById(docId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Financial Doc ID [{docId}] not found for the organization [{orgId}]";
            }

            r.FinancialDoc = m;
            return r;
        }

        public async Task<List<MFinancialDoc>> GetFinancialDocs(string orgId, VMFinancialDoc param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetFinancialDocs(param);

            result.ForEach(p => { DeserializeAllItems(p); ClearDefinitionFields(p); });

            return result;
        }

        public async Task<int> GetFinancialDocCount(string orgId, VMFinancialDoc param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetFinancialDocCount(param);

            return result;
        }

        public async Task<RevenueSummary> CalculateRevenue(string orgId, DateTime fromDate, DateTime toDate)
        {
            var param = new VMSummary()
            {
                FromDate = fromDate,
                ToDate = toDate,
            };

            var result = await _summarySvc.GetRevenueSummary(orgId, param);
            return result;
        }
    }
}
