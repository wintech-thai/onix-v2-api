using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class CycleService : BaseService, ICycleService
    {
        private readonly ICycleRepository? repository = null;

        public CycleService(ICycleRepository repo) : base()
        {
            repository = repo;
        }

        public MCycle GetCycleById(string orgId, string cycleId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetCycleById(cycleId);

            return result;
        }

        public MVCycle? AddCycle(string orgId, MCycle cycle)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVCycle();

            var isExist = repository!.IsCycleCodeExist(cycle.Code!);

            if (isExist)
            {
                r.Status = "DUPLICATE";
                r.Description = $"Cycle code [{cycle.Code}] is duplicate";

                return r;
            }

            var result = repository!.AddCycle(cycle);

            r.Status = "OK";
            r.Description = "Success";
            r.Cycle = result;

            return r;
        }

        public MVCycle? UpdateCycleById(string orgId, string cycleId, MCycle cycle)
        {
            var r = new MVCycle()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);
            var result = repository!.UpdateCycleById(cycleId, cycle);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Cycle ID [{cycleId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Cycle = result;
            return r;
        }

        public MVCycle? DeleteCycleById(string orgId, string cycleId)
        {
            var r = new MVCycle()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(cycleId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Cycle ID [{cycleId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = repository!.DeleteCycleById(cycleId);

            r.Cycle = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Cycle ID [{cycleId}] not found for the organization [{orgId}]";
            }

            return r;
        }

        public IEnumerable<MCycle> GetCycles(string orgId, VMCycle param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetCycles(param);

            return result;
        }

        public int GetCycleCount(string orgId, VMCycle param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetCycleCount(param);

            return result;
        }
    }
}
