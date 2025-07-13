using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class MasterRefService : BaseService, IMasterRefService
    {
        private readonly IMasterRefRepository? repository = null;

        public MasterRefService(IMasterRefRepository repo) : base()
        {
            repository = repo;
        }

        public MMasterRef GetMasterRefById(string orgId, string iocHostId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetMasterRefById(iocHostId);

            return result;
        }

        public MVMasterRef? AddMasterRef(string orgId, MMasterRef masterRef)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVMasterRef();

            var isExist = repository!.IsMasterRefCodeExist(masterRef.Code!);

            if (isExist)
            {
                r.Status = "DUPLICATE";
                r.Description = $"Master code [{masterRef.Code}] is duplicate";

                return r;
            }

            var result = repository!.AddMasterRef(masterRef);

            r.Status = "OK";
            r.Description = "Success";
            r.MasterRef = result;

            return r;
        }

        public MVMasterRef? UpdateMasterRefById(string orgId, string masterRefId, MMasterRef masterRef)
        {
            var r = new MVMasterRef()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);
            var result = repository!.UpdateMasterRefById(masterRefId, masterRef);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Master Ref ID [{masterRefId}] not found for the organization [{orgId}]";

                return r;
            }

            r.MasterRef = result;
            return r;
        }

        public MVMasterRef? DeleteMasterRefById(string orgId, string masterRefId)
        {
            var r = new MVMasterRef()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(masterRefId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Master Ref ID [{masterRefId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = repository!.DeleteMasterRefById(masterRefId);

            r.MasterRef = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Master Ref ID [{masterRefId}] not found for the organization [{orgId}]";
            }

            return r;
        }

        public IEnumerable<MMasterRef> GetMasterRefs(string orgId, VMMasterRef param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetMasterRefs(param);

            return result;
        }

        public int GetMasterRefCount(string orgId, VMMasterRef param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetMasterRefCount(param);

            return result;
        }
    }
}
