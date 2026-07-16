using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;
using LinqKit;

namespace Its.Onix.Api.Services
{
    public class NotiChannelService : BaseService, INotiChannelService
    {
        private readonly INotiChannelRepository? repository = null;
        private readonly IJobService _jobService;


        public NotiChannelService(INotiChannelRepository repo, IJobService jobService) : base()
        {
            repository = repo;
            _jobService = jobService;
        }

        public List<NameValue> GetEventTypes(string orgId)
        {
            var list = new List<NameValue>
            {
                new() { Name = "Payment.Success", Value = "Payment success event" },
                new() { Name = "Payment.Unidentified", Value = "Payment unidentified event" },
                new() { Name = "Payment.DailyTxAmountLimitExceeded", Value = "Payment daily transaction amount limit exceeded event" },
            };

            return list;
        }

        public List<NameValue> GetChannelTypes(string orgId)
        {
            var list = new List<NameValue>
            {
                new() { Name = "Discord", Value = "Discord" },
                new() { Name = "Telegram", Value = "Telegram" },
            };

            return list;
        }

        public async Task<MVNotiChannel> GetNotiChannelById(string orgId, string notiChannelId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVNotiChannel()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(notiChannelId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Notification channel ID [{notiChannelId}] format is invalid";

                return r;
            }   

            var result = await repository!.GetNotiChannelById(notiChannelId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Notification channel ID [{notiChannelId}] not found for the organization [{orgId}]";

                return r;
            }

            var evtTypes = result.EventsMatched != null ? result.EventsMatched.Split('|') : [];
            result.EventTypes = evtTypes;
            result.EventsMatched = "";

            r.NotiChannel = result;
            return r;
        }

        public async Task<MVNotiChannel> AddNotiChannel(string orgId, MNotiChannel notiChannel)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVNotiChannel();
            r.Status = "OK";
            r.Description = "Success";

            if (string.IsNullOrEmpty(notiChannel.ChannelName))
            {
                r.Status = "NAME_MISSING";
                r.Description = $"Notification channel name is missing!!!";

                return r;
            }

            var isExist = await repository!.IsChannelNameExist(notiChannel.ChannelName);
            if (isExist)
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"Notification channel name [{notiChannel.ChannelName}] already exist!!!";

                return r;
            }

            var evtTypeStr = notiChannel.EventTypes != null ? string.Join("|", notiChannel.EventTypes) : null;
            notiChannel.EventsMatched = evtTypeStr;

            var result = await repository!.AddNotiChannel(notiChannel);
            if (result == null)
            {
                r.Status = "FAILED";
                r.Description = $"Failed to add notification channel [{notiChannel.ChannelName}]";

                return r;
            }

            r.NotiChannel = result;

            return r;
        }

        public async Task<MVNotiChannel> DeleteNotiChannelById(string orgId, string notiChannelId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVNotiChannel()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(notiChannelId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Notification channel ID [{notiChannelId}] format is invalid";

                return r;
            }

            var currentCr = await GetNotiChannelById(orgId, notiChannelId);
            if (currentCr.NotiChannel == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Notification channel ID [{notiChannelId}] not found for the organization [{orgId}]";

                return r;
            }

            var m = await repository!.DeleteNotiChannelById(notiChannelId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Notification channel ID [{notiChannelId}] not found for the organization [{orgId}]";

                return r;
            }

            r.NotiChannel = m;
            return r;
        }

        public async Task<List<MNotiChannel>> GetNotiChannels(string orgId, VMNotiChannel param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetNotiChannels(param);

            result.ForEach(m => 
            {
                var evtTypes = m.EventsMatched != null ? m.EventsMatched.Split('|') : [];
                m.EventTypes = evtTypes;
                m.EventsMatched = "";

                m.MessageTemplate = ""; // do not return message template in the list API
            });

            return result;
        }

        public async Task<int> GetNotiChannelCount(string orgId, VMNotiChannel param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetNotiChannelCount(param);

            return result;
        }

        public async Task<MVNotiChannel> UpdateNotiChannelById(string orgId, string notiChannelId, MNotiChannel notiChannel)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVNotiChannel()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(notiChannelId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Notification channel ID [{notiChannelId}] format is invalid";

                return r;
            }

            var channelName = notiChannel.ChannelName;
            if (string.IsNullOrEmpty(channelName))
            {
                r.Status = "NAME_MISSING";
                r.Description = $"Notification channel name is missing!!!";

                return r;
            }
            
            var m = await repository!.GetNotiChannelByName(channelName);
            if (m != null)
            {
                if (m.Id.ToString() != notiChannelId) 
                {
                    r.Status = "NAME_DUPLICATE";
                    r.Description = $"Notification channel name [{channelName}] already exist!!!";

                    return r;
                }
            }

            var evtTypeStr = notiChannel.EventTypes != null ? string.Join("|", notiChannel.EventTypes) : null;
            notiChannel.EventsMatched = evtTypeStr;

            var result = await repository!.UpdateNotiChannelById(notiChannelId, notiChannel);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Notification channel ID [{notiChannelId}] not found for the organization [{orgId}]";

                return r;
            }

            r.NotiChannel = result;
            return r;
        }

        public async Task<MVNotiChannel> UpdateNotiChannelStatusById(string orgId, string notiChannelId, string status)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVNotiChannel()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(notiChannelId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Notification channel ID [{notiChannelId}] format is invalid";

                return r;
            }

            var result = await repository!.UpdateNotiChannelStatusById(notiChannelId, status);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Notification channel ID [{notiChannelId}] not found for the organization [{orgId}]";

                return r;
            }

            r.NotiChannel = result;
            return r;
        }

        private IEnumerable<string> GetEventTypeSet()
        {
            var types = GetEventTypes("global");

            var eventTypeSet = new List<string>();
            types.ForEach(t => eventTypeSet.Add(t.Name!));

            return eventTypeSet;
        }

        public MVJob GetNotiEventById(string orgId, string notiEventId)
        {
            var r = new MVJob()
            {
                Status = "OK",
                Description = "Success"
            };

            var result = _jobService.GetJobById(orgId, notiEventId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Notification event ID [{notiEventId}] not found for the organization [{orgId}]";
            }

            r.Job = result;

            return r;
        }

        public IEnumerable<MJob> GetNotiEvents(string orgId, VMJob param)
        {
            param.EventTypeSet = GetEventTypeSet();

            var result = _jobService.GetJobs(orgId, param);

            result.ForEach(r => 
            {
                r.Configuration = ""; // do not return configuration in the event API
                r.JobMessage = ""; // do not return job message in the event API
                r.JobMessage2 = ""; // do not return notify job message in the event API
            });

            return result;
        }

        public int GetNotiEventCount(string orgId, VMJob param)
        {
            param.EventTypeSet = GetEventTypeSet();

            var result = _jobService.GetJobCount(orgId, param);
            return result;
        }
    }
}
