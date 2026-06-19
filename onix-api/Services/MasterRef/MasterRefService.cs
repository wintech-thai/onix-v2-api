using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;
using System.Text.Json;

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

        // ──────────────────────────── V2 — async ────────────────────────────

        private string SerializeDefinition(MMasterRef masterRef)
        {
            var obj = masterRef.DefinitionObj;
            if (obj == null)
            {
                return "[]";
            }

            return JsonSerializer.Serialize(obj);
        }

        private void DeserializeDefinition(MMasterRef masterRef)
        {
            var jsonStr = masterRef.Definition;
            if (string.IsNullOrEmpty(jsonStr))
            {
                jsonStr = "[]";
            }

            try
            {
                masterRef.DefinitionObj = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonStr);
            }
            catch
            {
                masterRef.DefinitionObj = [];
            }
        }

        // เคลียร์ raw JSON string ทิ้งก่อนส่งกลับ ไม่ให้ response มีทั้ง raw string และ parsed object ซ้ำซ้อนกัน
        // (pattern เดียวกับ AgentService.GetAgentById() ที่เคลียร์ BankAccountsSelected ทิ้งหลัง deserialize)
        private static void ClearDefinition(MMasterRef masterRef)
        {
            masterRef.Definition = "";
        }

        public async Task<MVMasterRef> GetMasterRefByIdV2(string orgId, string masterRefId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVMasterRef()
            {
                Status = "OK",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(masterRefId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Master Ref ID [{masterRefId}] format is invalid";

                return r;
            }

            var result = await repository!.GetMasterRefByIdV2(masterRefId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Master Ref ID [{masterRefId}] not found for the organization [{orgId}]";

                return r;
            }

            DeserializeDefinition(result);
            ClearDefinition(result);

            r.MasterRef = result;
            return r;
        }

        public async Task<MVMasterRef> AddMasterRefV2(string orgId, MMasterRef masterRef)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVMasterRef()
            {
                Status = "OK",
                Description = "Success",
            };

            if (string.IsNullOrEmpty(masterRef.Code))
            {
                r.Status = "CODE_MISSING";
                r.Description = "Master Ref code is missing!!!";

                return r;
            }

            var isExist = await repository!.IsMasterRefCodeExistV2(masterRef.Code);
            if (isExist)
            {
                r.Status = "DUPLICATE";
                r.Description = $"Master code [{masterRef.Code}] is duplicate";

                return r;
            }

            masterRef.Definition = SerializeDefinition(masterRef);

            var result = await repository!.AddMasterRefV2(masterRef);
            ClearDefinition(result);

            r.MasterRef = result;
            return r;
        }

        public async Task<MVMasterRef> UpdateMasterRefByIdV2(string orgId, string masterRefId, MMasterRef masterRef)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVMasterRef()
            {
                Status = "OK",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(masterRefId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Master Ref ID [{masterRefId}] format is invalid";

                return r;
            }

            masterRef.Definition = SerializeDefinition(masterRef);

            var result = await repository!.UpdateMasterRefByIdV2(masterRefId, masterRef);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Master Ref ID [{masterRefId}] not found for the organization [{orgId}]";

                return r;
            }

            DeserializeDefinition(result);
            ClearDefinition(result);

            r.MasterRef = result;
            return r;
        }

        public async Task<MVMasterRef> DeleteMasterRefByIdV2(string orgId, string masterRefId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVMasterRef()
            {
                Status = "OK",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(masterRefId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Master Ref ID [{masterRefId}] format is invalid";

                return r;
            }

            var m = await repository!.DeleteMasterRefByIdV2(masterRefId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Master Ref ID [{masterRefId}] not found for the organization [{orgId}]";
            }

            r.MasterRef = m;
            return r;
        }

        public async Task<List<MMasterRef>> GetMasterRefsV2(string orgId, VMMasterRef param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetMasterRefsV2(param);

            result.ForEach(p => { DeserializeDefinition(p); ClearDefinition(p); });

            return result;
        }

        public async Task<int> GetMasterRefCountV2(string orgId, VMMasterRef param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetMasterRefCountV2(param);

            return result;
        }
    }
}
