using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;
using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Services
{
    public class CaseManagementService : BaseService, ICaseManagementService
    {
        private readonly ICaseManagementRepository repository;

        public CaseManagementService(ICaseManagementRepository repo) : base()
        {
            repository = repo;
        }

        public async Task<MVCaseManagement> GetCaseById(string orgId, string caseId)
        {
            repository.SetCustomOrgId(orgId);
            var r = new MVCaseManagement { Status = "OK", Description = "Success" };

            if (!ServiceUtils.IsGuidValid(caseId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Case ID [{caseId}] format is invalid";
                return r;
            }

            var result = await repository.GetCaseById(caseId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Case ID [{caseId}] not found";
                return r;
            }

            r.CaseManagement = result;
            return r;
        }

        public async Task<List<MCaseManagement>> GetCases(string orgId, VMCaseManagement param)
        {
            repository.SetCustomOrgId(orgId);
            var result = await repository.GetCases(param);
            result.ForEach(p => p.Description = "");
            return result;
        }

        public async Task<int> GetCaseCount(string orgId, VMCaseManagement param)
        {
            repository.SetCustomOrgId(orgId);
            return await repository.GetCaseCount(param);
        }

        public async Task<MVCaseManagement> AddCase(string orgId, MCaseManagement caseManagement)
        {
            repository.SetCustomOrgId(orgId);
            var r = new MVCaseManagement { Status = "OK", Description = "Success" };

            if (string.IsNullOrEmpty(caseManagement.Subject))
            {
                r.Status = "SUBJECT_MISSING";
                r.Description = "Subject is required";
                return r;
            }

            if (string.IsNullOrEmpty(caseManagement.Description))
            {
                r.Status = "DESCRIPTION_MISSING";
                r.Description = "Description is required";
                return r;
            }

            // Generate ref: <orgid>-YYYYMMDDHHmmss
            var now = DateTime.UtcNow;
            caseManagement.Ref = $"{orgId}-{now:yyyyMMddHHmmss}";
            caseManagement.Status = "New";

            var result = await repository.AddCase(caseManagement);
            r.CaseManagement = result;
            return r;
        }

        public async Task<MVCaseManagement> UpdateCaseStatus(string orgId, string caseId, string status, string updatedBy)
        {
            repository.SetCustomOrgId(orgId);
            var r = new MVCaseManagement { Status = "OK", Description = "Success" };

            if (!ServiceUtils.IsGuidValid(caseId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Case ID [{caseId}] format is invalid";
                return r;
            }

            var validStatuses = new[] { "New", "Open", "In Progress", "Waiting for Customer", "Resolved", "Closed", "Cancelled" };
            if (!validStatuses.Contains(status))
            {
                r.Status = "STATUS_INVALID";
                r.Description = $"Status [{status}] is not valid";
                return r;
            }

            var result = await repository.UpdateCaseStatusById(caseId, status, updatedBy);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Case ID [{caseId}] not found";
                return r;
            }

            r.CaseManagement = result;
            return r;
        }

        public async Task<List<MCaseManagement>> GetAllCases(VMCaseManagement param)
        {
            var result = await repository.GetCases(param);
            result.ForEach(p => p.Description = "");
            return result;
        }

        public async Task<int> GetAllCaseCount(VMCaseManagement param)
        {
            return await repository.GetCaseCount(param);
        }

        public async Task<MVCaseManagementComment> AddComment(string orgId, string caseId, string content, string createdBy, string authorType)
        {
            repository.SetCustomOrgId(orgId);
            var r = new MVCaseManagementComment { Status = "OK", Description = "Success" };

            if (!ServiceUtils.IsGuidValid(caseId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Case ID [{caseId}] format is invalid";
                return r;
            }

            if (string.IsNullOrEmpty(content))
            {
                r.Status = "CONTENT_MISSING";
                r.Description = "Content is required";
                return r;
            }

            var caseEntity = await repository.GetCaseById(caseId);
            if (caseEntity == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Case ID [{caseId}] not found";
                return r;
            }

            if (caseEntity.Status == "Closed" || caseEntity.Status == "Resolved")
            {
                r.Status = "CASE_CLOSED";
                r.Description = "Cannot add comment to a Closed or Resolved case";
                return r;
            }

            var comment = new MCaseManagementComment
            {
                CaseId = caseEntity.Id,
                OrgId = caseEntity.OrgId,
                Content = content,
                AuthorType = authorType,
                CreatedBy = createdBy,
            };

            var result = await repository.AddComment(comment);
            r.Comment = result;
            return r;
        }

        public async Task<MVCaseManagementComment> GetComments(string orgId, string caseId)
        {
            repository.SetCustomOrgId(orgId);
            var r = new MVCaseManagementComment { Status = "OK", Description = "Success" };

            if (!ServiceUtils.IsGuidValid(caseId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Case ID [{caseId}] format is invalid";
                return r;
            }

            var caseEntity = await repository.GetCaseById(caseId);
            if (caseEntity == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Case ID [{caseId}] not found";
                return r;
            }

            r.Comments = await repository.GetComments(caseEntity.Id!.Value);
            return r;
        }
    }
}
