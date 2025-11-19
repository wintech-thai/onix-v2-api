using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using System.Threading.Tasks;
using Its.Onix.Api.Utils;
using System.Text.Json;

namespace Its.Onix.Api.Services
{
    public class PointTriggerService : BaseService, IPointTriggerService
    {
        private readonly IPointTriggerRepository repository = null!;
        private readonly IPointService _pointService;
        private readonly IPointRuleService _pointRuleService;

        public PointTriggerService(
            IPointTriggerRepository repo,
            IPointService pointService,
            IPointRuleService pointRuleService) : base()
        {
            repository = repo;
            _pointService = pointService;
            _pointRuleService = pointRuleService;
        }

        public async Task<MVPointTrigger> AddPointTrigger(string orgId, PointTriggerInput pt)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPointTrigger()
            {
                Status = "OK",
                Description = "Success",
            };

            if (string.IsNullOrEmpty(pt.WalletId))
            {
                r.Status = "WALLET_MISSING";
                r.Description = $"Wallet ID is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(pt.EventTriggered))
            {
                r.Status = "TRIGGER_EVENT_MISSING";
                r.Description = $"EventTriggered is missing!!!";

                return r;
            }

            var pi = pt.PointRuleInput;
            if (pi == null)
            {
                r.Status = "POINT_INPUT_MISSING";
                r.Description = $"Point input is missing!!!";

                return r;
            }

            var wallet = await _pointService.GetWalletById(orgId, pt.WalletId);
            if (wallet == null)
            {
                r.Status = "WALLET_NOT_FOUND";
                r.Description = $"Wallet ID [{pt.WalletId}] not found!!!";

                return r;
            }

            var (isMatch, point) = await GetPoint(orgId, pt.EventTriggered, pi);
            var triggerId = Guid.NewGuid().ToString();

            var pointTrigger = new MPointTrigger
            {
                TriggerParams = JsonSerializer.Serialize(pt),
                TriggeredEvent = pt.EventTriggered,
                WalletId = pt.WalletId,
                TriggerName = $"{pt.EventTriggered}:{triggerId}",
                Description = $"Point trigger for wallet ID {pt.WalletId}",
                IsRuleMatch = "False",
                Points = point,
            };

            if (isMatch)
            {
                pointTrigger.IsRuleMatch = "True";
            }

            var tx = new MPointTx
            {
                WalletId = pt.WalletId,
                Tags = $"TriggerId={triggerId},Event={pt.EventTriggered}",
                Description = $"Trigger by event [{pt.EventTriggered}]",
                TxAmount = point,
                TxType = 1, /* IN */
            };

            var _ = await _pointService.AddPoint(orgId, tx);

            var result = await repository!.AddPointTrigger(pointTrigger);
            r.PointTrigger = result;

            return r;
        }

        private async Task<(bool isMatch, int piont)> GetPoint(string orgId, string triggerEvent, PointRuleInput pri)
        {
            var result = await _pointRuleService.EvaluatePointRules(orgId, triggerEvent, pri);
            
            var point = 0;
            if (result.IsMatch)
            {
                point = Convert.ToInt32(result.ExecutionResult);
            }

            return (result.IsMatch, point);
        }

        public async Task<MVPointTrigger?> GetPointTriggerById(string orgId, string pointTriggerId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPointTrigger()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(pointTriggerId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Point trigger ID [{pointTriggerId}] format is invalid";

                return r;
            }

            var result = await repository!.GetPointTriggerById(pointTriggerId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Point trigger ID [{pointTriggerId}] not found for the organization [{orgId}]";
            }

            r.PointTrigger = result;
            return r;
        }

        public async Task<List<MPointTrigger>> GetPointTriggers(string orgId, VMPointTrigger param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPointTriggers(param);

            return result;
        }

        public async Task<int> GetPointTriggersCount(string orgId, VMPointTrigger param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPointTriggerCount(param);

            return result;
        }
    }
}
