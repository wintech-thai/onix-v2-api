using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class IocService : BaseService, IIocService
    {
        private readonly IIocRepository? repository = null;

        public IocService(IIocRepository repo) : base()
        {
            repository = repo;
        }

        public async Task<MVIoc> GetIocByIdV2(string orgId, string iocId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVIoc()
            {
                Status = "OK",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(iocId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"IoC ID [{iocId}] format is invalid";

                return r;
            }

            var result = await repository!.GetIocByIdV2(iocId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"IoC ID [{iocId}] not found";

                return r;
            }

            r.Ioc = result;
            return r;
        }

        public async Task<MVIoc> AddIocV2(string orgId, MIoc ioc)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVIoc()
            {
                Status = "OK",
                Description = "Success",
            };

            if (string.IsNullOrEmpty(ioc.IocType))
            {
                r.Status = "IOC_TYPE_MISSING";
                r.Description = "IocType is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(ioc.IocValue))
            {
                r.Status = "IOC_VALUE_MISSING";
                r.Description = "IocValue is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(ioc.Status))
            {
                ioc.Status = "Active";
            }

            var result = await repository!.AddIocV2(ioc);

            r.Ioc = result;
            return r;
        }

        public async Task<MVIoc> UpdateIocByIdV2(string orgId, string iocId, MIoc ioc)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVIoc()
            {
                Status = "OK",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(iocId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"IoC ID [{iocId}] format is invalid";

                return r;
            }

            var result = await repository!.UpdateIocByIdV2(iocId, ioc);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"IoC ID [{iocId}] not found";

                return r;
            }

            r.Ioc = result;
            return r;
        }

        public async Task<MVIoc> UpdateIocStatusByIdV2(string orgId, string iocId, string status)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVIoc()
            {
                Status = "OK",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(iocId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"IoC ID [{iocId}] format is invalid";

                return r;
            }

            var result = await repository!.UpdateIocStatusByIdV2(iocId, status);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"IoC ID [{iocId}] not found";

                return r;
            }

            r.Ioc = result;
            return r;
        }

        public async Task<MVIoc> DeleteIocByIdV2(string orgId, string iocId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVIoc()
            {
                Status = "OK",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(iocId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"IoC ID [{iocId}] format is invalid";

                return r;
            }

            var m = await repository!.DeleteIocByIdV2(iocId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"IoC ID [{iocId}] not found";
            }

            r.Ioc = m;
            return r;
        }

        public async Task<List<MIoc>> GetIocsV2(string orgId, VMIoc param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetIocsV2(param);

            return result;
        }

        public async Task<int> GetIocCountV2(string orgId, VMIoc param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetIocCountV2(param);

            return result;
        }
    }
}
