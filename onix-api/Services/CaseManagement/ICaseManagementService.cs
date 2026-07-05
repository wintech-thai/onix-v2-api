using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface ICaseManagementService
    {
        public Task<MVCaseManagement> GetCaseById(string orgId, string caseId);
        public Task<List<MCaseManagement>> GetCases(string orgId, VMCaseManagement param);
        public Task<int> GetCaseCount(string orgId, VMCaseManagement param);
        public Task<MVCaseManagement> AddCase(string orgId, MCaseManagement caseManagement);
        public Task<MVCaseManagement> UpdateCaseStatus(string orgId, string caseId, string status, string updatedBy);
        // admin: query across all orgs
        public Task<List<MCaseManagement>> GetAllCases(VMCaseManagement param);
        public Task<int> GetAllCaseCount(VMCaseManagement param);
        public Task<MVCaseManagementComment> AddComment(string orgId, string caseId, string content, string createdBy, string authorType);
        public Task<MVCaseManagementComment> GetComments(string orgId, string caseId);
    }
}
