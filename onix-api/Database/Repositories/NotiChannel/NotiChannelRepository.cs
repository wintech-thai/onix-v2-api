using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class NotiChannelRepository : BaseRepository, INotiChannelRepository
    {
        public NotiChannelRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<MNotiChannel> AddNotiChannel(MNotiChannel alertChannel)
        {
            alertChannel.OrgId = orgId;
            
            alertChannel.Id = Guid.NewGuid();
            alertChannel.CreatedDate = DateTime.UtcNow;

            context!.NotiChannels!.Add(alertChannel);
            await context.SaveChangesAsync();

            return alertChannel;
        }

        public async Task<List<MNotiChannel>> GetNotiChannels(VMNotiChannel param)
        {
            var limit = 0;
            var offset = 0;

            //Param will never be null
            if (param.Offset > 0)
            {
                //Convert to zero base
                offset = param.Offset-1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var predicate = NotiChannelPredicate(param!);
            var result = await GetSelection().AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.CreatedDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            foreach (var r in result)
            {
                //ไม่ต้อง masking เพราะว่าต้องเอาไปใช้ตอนวนลูปส่งต่อ
                //r.DiscordWebhookUrl = "";
            }

            return result;
        }

        public async Task<int> GetNotiChannelCount(VMNotiChannel param)
        {
            var predicate = NotiChannelPredicate(param!);
            var result = await context!.NotiChannels!.Where(predicate).AsExpandable().CountAsync();

            return result;
        }

        public async Task<MNotiChannel?> GetNotiChannelById(string notiChannelId)
        {
            Guid id = Guid.Parse(notiChannelId);
            var u = await context!.NotiChannels!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public IQueryable<MNotiChannel> GetSelection()
        {
            var query =
                from channel in context!.NotiChannels!
                select new { channel };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MNotiChannel
            {
                Id = x.channel.Id,
                OrgId = x.channel.OrgId,
                ChannelName = x.channel.ChannelName,
                Description = x.channel.Description,
                Tags = x.channel.Tags,
                Type = x.channel.Type,
                Status = x.channel.Status,
                DiscordWebhookUrl = x.channel.DiscordWebhookUrl,
                TelegramWebhookUrl = x.channel.TelegramWebhookUrl,
                TelegramChatId = x.channel.TelegramChatId,
                CreatedDate = x.channel.CreatedDate,
                EventsMatched = x.channel.EventsMatched,
                MessageTemplate = x.channel.MessageTemplate
            });
        }

        private ExpressionStarter<MNotiChannel> NotiChannelPredicate(VMNotiChannel param)
        {
            var pd = PredicateBuilder.New<MNotiChannel>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MNotiChannel>();
                fullTextPd = fullTextPd.Or(p => p.ChannelName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Type!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Status!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.EventsMatched!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            if ((param.Status != "") && (param.Status != null))
            {
                var statusPd = PredicateBuilder.New<MNotiChannel>();
                statusPd = statusPd.Or(p => p.Status!.Equals(param.Status));

                pd = pd.And(statusPd);
            }

            return pd;
        }

        public async Task<MNotiChannel?> DeleteNotiChannelById(string notiChannelId)
        {
            Guid id = Guid.Parse(notiChannelId);
            var existing = await context!.NotiChannels!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                context.NotiChannels!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<MNotiChannel?> UpdateNotiChannelById(string notiChannelId, MNotiChannel notiChannel)
        {
            Guid id = Guid.Parse(notiChannelId);
            var existing = await context!.NotiChannels!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                // ไม่ให้ update Type
                existing.ChannelName = notiChannel.ChannelName;
                existing.Description = notiChannel.Description;
                existing.Tags = notiChannel.Tags;
                existing.DiscordWebhookUrl = notiChannel.DiscordWebhookUrl;
                existing.TelegramWebhookUrl = notiChannel.TelegramWebhookUrl;
                existing.TelegramChatId = notiChannel.TelegramChatId;
                existing.MessageTemplate = notiChannel.MessageTemplate;
                existing.EventsMatched = notiChannel.EventsMatched;

                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<MNotiChannel?> UpdateNotiChannelStatusById(string notiChannelId, string status)
        {
            Guid id = Guid.Parse(notiChannelId);
            var existing = await context!.NotiChannels!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Status = status;
                await context.SaveChangesAsync();
            }
            
            return existing;
        }

        public async Task<bool> IsChannelNameExist(string channelName)
        {
            var exists = await context!.NotiChannels!.AsExpandable().AnyAsync(p => p!.ChannelName!.Equals(channelName) && p!.OrgId!.Equals(orgId));
            return exists;
        }

        public async Task<MNotiChannel?> GetNotiChannelByName(string channelName)
        {
            var exists = await context!.NotiChannels!.AsExpandable().Where(p => p!.ChannelName!.Equals(channelName) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return exists;
        }
    }
}