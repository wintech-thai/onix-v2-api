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
        private readonly IRedisHelper _redis;

        public PointTriggerService(
            IPointTriggerRepository repo,
            IPointService pointService,
            IPointRuleService pointRuleService,
            IRedisHelper redis) : base()
        {
            repository = repo;
            _pointService = pointService;
            _pointRuleService = pointRuleService;
            _redis = redis;
        }

        public async Task<MVPointTrigger> AddPointTrigger(string orgId, string token, PointTriggerInput pt)
        {
            var r = new MVPointTrigger()
            {
                Status = "OK",
                Description = "Success",
            };

            var cacheKey = CacheHelper.CreatePointTriggerCustRegisterKey(orgId);
            var otpObj = _redis.GetObjectAsync<MJob>($"{cacheKey}:{token}").Result;
            if (otpObj == null)
            {
                r.Status = "TOKEN_NOTFOUND_OR_EXPIRE";
                r.Description = $"Token [{token}] not found or expire!!!";

                return r;
            }

            var t = await AddPointTrigger(orgId, pt);
            return t;
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

            var t = await _pointService.GetWalletById(orgId, pt.WalletId);
            if (t!.Status != "OK")
            {
                r.Status = t.Status;
                r.Description = t.Description;

                return r;
            }

            var (isMatch, exeResult) = await GetPoint(orgId, pt.EventTriggered, pi);
            pi.RuleDefinition = ""; //เคลียร์ไปไม่ต้องเก็บใน TriggerParams

            var triggerId = Guid.NewGuid().ToString();
            var point = Convert.ToInt32(exeResult.ExecutionResult);

            var ptp = new PointTriggerParam()
            {
                TriggerInput = pt,
                TriggerResult = exeResult,
            };

            var pointTrigger = new MPointTrigger
            {
                TriggerParams = JsonSerializer.Serialize(ptp),
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

        private async Task<(bool isMatch, PointRuleExecutionResult exeResult)> GetPoint(string orgId, string triggerEvent, PointRuleInput pri)
        {
            var result = await _pointRuleService.EvaluatePointRules(orgId, triggerEvent, pri);
            return (result.IsMatch, result);
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
