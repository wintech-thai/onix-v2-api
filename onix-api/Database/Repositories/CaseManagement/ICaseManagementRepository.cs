using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface ICaseManagementRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<MCaseManagement?> GetCaseById(string caseId);
        public Task<List<MCaseManagement>> GetCases(VMCaseManagement param);
        public Task<int> GetCaseCount(VMCaseManagement param);
        public Task<MCaseManagement> AddCase(MCaseManagement caseManagement);
        public Task<MCaseManagement?> UpdateCaseStatusById(string caseId, string status, string updatedBy);
        public Task<MCaseManagementComment> AddComment(MCaseManagementComment comment);
        public Task<List<MCaseManagementComment>> GetComments(Guid caseId);
    }
}
