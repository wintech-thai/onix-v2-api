using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IEntityRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public MEntity AddEntity(MEntity entity);
        public int GetEntityCount(VMEntity param);
        public IEnumerable<MEntity> GetEntities(VMEntity param);
        public MEntity GetEntityById(string entityId);
        public MEntity? DeleteEntityById(string entityId);
        public bool IsEntityCodeExist(string entityCode);
        public MEntity? UpdateEntityById(string entityId, MEntity entity);
    }
}
