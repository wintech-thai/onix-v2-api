using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface IIocRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<MIoc> AddIocV2(MIoc ioc);
        public Task<int> GetIocCountV2(VMIoc param);
        public Task<List<MIoc>> GetIocsV2(VMIoc param);
        public Task<MIoc?> GetIocByIdV2(string iocId);
        public Task<MIoc?> GetIocByTypeAndValueV2(string iocType, string iocValue);
        public Task<MIoc?> DeleteIocByIdV2(string iocId);
        public Task<MIoc?> UpdateIocByIdV2(string iocId, MIoc ioc);
        public Task<MIoc?> UpdateIocStatusByIdV2(string iocId, string status);
    }
}
