using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface INotiChannelService
    {
        public Task<MVNotiChannel> GetNotiChannelById(string orgId, string notiChannelId);
        public Task<MVNotiChannel> AddNotiChannel(string orgId, MNotiChannel notiChannel);
        public Task<MVNotiChannel> DeleteNotiChannelById(string orgId, string notiChannelId);
        public Task<List<MNotiChannel>> GetNotiChannels(string orgId, VMNotiChannel param);
        public Task<int> GetNotiChannelCount(string orgId, VMNotiChannel param);
        public Task<MVNotiChannel> UpdateNotiChannelById(string orgId, string notiChannelId, MNotiChannel notiChannel);
        public Task<MVNotiChannel> UpdateNotiChannelStatusById(string orgId, string notiChannelId, string status);
    }
}
