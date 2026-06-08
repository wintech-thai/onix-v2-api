using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Database.Repositories
{
    public interface INotiChannelRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<List<MNotiChannel>> GetNotiChannels(VMNotiChannel param);
        public Task<int> GetNotiChannelCount(VMNotiChannel param);
        public Task<MNotiChannel?> GetNotiChannelById(string notiChannelId);
        public Task<MNotiChannel> AddNotiChannel(MNotiChannel notiChannel);
        public Task<MNotiChannel?> DeleteNotiChannelById(string notiChannelId);
        public Task<MNotiChannel?> UpdateNotiChannelById(string notiChannelId, MNotiChannel notiChannel);
        public Task<MNotiChannel?> UpdateNotiChannelStatusById(string notiChannelId, string status);
        public Task<MNotiChannel?> GetNotiChannelByName(string channelName);
        public Task<bool> IsChannelNameExist(string channelName);
    }
}
