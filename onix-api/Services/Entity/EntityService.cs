using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class EntityService : BaseService, IEntityService
    {
        private readonly IEntityRepository? repository = null;

        public EntityService(IEntityRepository repo) : base()
        {
            repository = repo;
        }

        public MEntity GetEntityById(string orgId, string entityId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetEntityById(entityId);

            return result;
        }

        public MVEntity? AddEntity(string orgId, MEntity cycle)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVEntity();

            var isExist = repository!.IsEntityCodeExist(cycle.Code!);

            if (isExist)
            {
                r.Status = "DUPLICATE";
                r.Description = $"Entity code [{cycle.Code}] is duplicate";

                return r;
            }

            var result = repository!.AddEntity(cycle);

            r.Status = "OK";
            r.Description = "Success";
            r.Entity = result;

            return r;
        }

        public MVEntity? UpdateEntityEmailById(string orgId, string entityId, string email, bool sendVerification)
        {
            return null;
        }

        public MVEntity? UpdateEntityById(string orgId, string entityId, MEntity cycle)
        {
            var r = new MVEntity()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);
            var result = repository!.UpdateEntityById(entityId, cycle);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Entity ID [{entityId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Entity = result;
            return r;
        }

        public MVEntity? DeleteEntityById(string orgId, string entityId)
        {
            var r = new MVEntity()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(entityId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Entity ID [{entityId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = repository!.DeleteEntityById(entityId);

            r.Entity = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Entity ID [{entityId}] not found for the organization [{orgId}]";
            }

            return r;
        }

        public IEnumerable<MEntity> GetEntities(string orgId, VMEntity param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetEntities(param);

            foreach (var entity in result)
            {
                //เพื่อไม่ให้ข้อมูลที่ response กลับไปใหญ่จนเกินไป
                entity.Content = "";
            }

            return result;
        }

        public int GetEntityCount(string orgId, VMEntity param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetEntityCount(param);

            return result;
        }
    }
}
