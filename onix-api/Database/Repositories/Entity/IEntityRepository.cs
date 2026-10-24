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

        // V2 — async
        public Task<MEntity> AddEntityV2(MEntity entity);
        public Task<int> GetEntityCountV2(VMEntity param);
        public Task<List<MEntity>> GetEntitiesV2(VMEntity param);
        public Task<MEntity?> GetEntityByIdV2(string entityId);
        public Task<bool> IsEntityCodeExistV2(string entityCode);
        public Task<bool> IsEntityCodeExistExcludingIdV2(string entityCode, string entityId);
        public Task<bool> IsPrimaryEmailExistV2(string email);
        public Task<MEntity?> DeleteEntityByIdV2(string entityId);
        public Task<MEntity?> UpdateEntityByIdV2(string entityId, MEntity entity);
        public Task<List<MEntityAddress>> GetEntityAddressesByEntityIdV2(string entityId);
        public Task<MEntityAddress?> GetEntityAddressByIdV2(string addressId);
        public Task<MEntityAddress> AddEntityAddressV2(MEntityAddress address);
        public Task<MEntityAddress?> UpdateEntityAddressByIdV2(string addressId, MEntityAddress address);
        public Task<MEntityAddress?> DeleteEntityAddressByIdV2(string addressId);
    }
}
