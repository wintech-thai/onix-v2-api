using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IEntityService
    {
        public MEntity GetEntityById(string orgId, string cycleId);
        public MVEntity? AddEntity(string orgId, MEntity entity);
        public MVEntity? DeleteEntityById(string orgId, string cycleId);
        public IEnumerable<MEntity> GetEntities(string orgId, VMEntity param);
        public int GetEntityCount(string orgId, VMEntity param);
        public MVEntity? UpdateEntityById(string orgId, string cycleId, MEntity systemVariable);
        public MVEntity? UpdateEntityEmailStatusById(string orgId, string entityId, string status);
        public MVEntity? UpdateEntityEmailById(string orgId, string entityId, string email, bool sendVerification);
        public MVEntity? UpdateEntityUserNameById(string orgId, string entityId, string userName);
        public MVEntity? UpdateEntityUserStatusById(string orgId, string entityId, string status);
        public MVEntity? SendCustomerUserCreationEmail(string orgId, string entityId);
    }
}
