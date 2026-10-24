using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IEntityService
    {
        public MEntity GetEntityById(string orgId, string cycleId);
        public MEntity GetEntityByEmail(string orgId, string email);
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
        public MVJob? SendCustomerResetPasswordEmail(string orgId, string entityId);
        public MVEntity? UpdateUserStatusById(string orgId, string entityId, string status);
        public MVCustomerUser VerifyUserIsCustomer(string userName);
        public List<MEntityAddress> GetEntityAddressesByEntityId(string orgId, string entityId);
        public MVEntityAddress AddEntityAddress(string orgId, string entityId, MEntityAddress address);
        public MVEntityAddress UpdateEntityAddressById(string orgId, string entityId, string addressId, MEntityAddress address);
        public MVEntityAddress DeleteEntityAddressById(string orgId, string entityId, string addressId);

        // V2 — async
        public Task<MEntity?> GetEntityByIdV2(string orgId, string entityId);
        public Task<MVEntity> AddEntityV2(string orgId, MEntity entity);
        public Task<MVEntity> UpdateEntityByIdV2(string orgId, string entityId, MEntity entity);
        public Task<MVEntity> DeleteEntityByIdV2(string orgId, string entityId);
        public Task<List<MEntity>> GetEntitiesV2(string orgId, VMEntity param);
        public Task<int> GetEntityCountV2(string orgId, VMEntity param);
        public Task<List<MEntityAddress>> GetEntityAddressesByEntityIdV2(string orgId, string entityId);
        public Task<MVEntityAddress> AddEntityAddressV2(string orgId, string entityId, MEntityAddress address);
        public Task<MVEntityAddress> UpdateEntityAddressByIdV2(string orgId, string entityId, string addressId, MEntityAddress address);
        public Task<MVEntityAddress> DeleteEntityAddressByIdV2(string orgId, string entityId, string addressId);
    }
}
