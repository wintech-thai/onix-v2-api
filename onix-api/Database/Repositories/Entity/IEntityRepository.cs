using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IEntityRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public MEntity AddEntity(MEntity entity);
        public MEntity GetOrCreateEntityByEmail(MEntity entity);
        public int GetEntityCount(VMEntity param);
        public IEnumerable<MEntity> GetEntities(VMEntity param);
        public MEntity GetEntityById(string entityId);
        public MEntity GetEntityByEmail(string email);
        public MEntity? DeleteEntityById(string entityId);
        public bool IsEntityCodeExist(string entityCode);
        public bool IsEntityCodeExistExcludingId(string entityCode, string entityId);
        public bool IsPrimaryEmailExist(string email);
        public List<MEntityAddress> GetEntityAddressesByEntityId(string entityId);
        public MEntityAddress? GetEntityAddressById(string addressId);
        public MEntityAddress AddEntityAddress(MEntityAddress address);
        public MEntityAddress? UpdateEntityAddressById(string addressId, MEntityAddress address);
        public MEntityAddress? DeleteEntityAddressById(string addressId);
        public MEntity? UpdateEntityById(string entityId, MEntity entity);
        public MEntity? UpdateEntityEmailById(string entityId, string email);
        public MEntity? UpdateEntityEmailStatusById(string entityId, string status);
        public MEntity? UpdateEntityUserNameById(string entityId, string userName);
        public MEntity? UpdateEntityUserStatusById(string entityId, string status);
        public MEntity? UpdateUserStatusById(string entityId, string status);
    }
}
