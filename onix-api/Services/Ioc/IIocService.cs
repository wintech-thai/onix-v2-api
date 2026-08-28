using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IIocService
    {
        public Task<MVIoc> GetIocByIdV2(string orgId, string iocId);
        public Task<MVIoc> AddIocV2(string orgId, MIoc ioc);
        public Task<MVIoc> UpdateIocByIdV2(string orgId, string iocId, MIoc ioc);
        public Task<MVIoc> UpdateIocStatusByIdV2(string orgId, string iocId, string status);
        public Task<MVIoc> DeleteIocByIdV2(string orgId, string iocId);
        public Task<List<MIoc>> GetIocsV2(string orgId, VMIoc param);
        public Task<int> GetIocCountV2(string orgId, VMIoc param);
    }
}
