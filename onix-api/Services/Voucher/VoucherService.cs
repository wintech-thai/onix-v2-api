using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Services
{
    public class VoucherService : BaseService, IVoucherService
    {
        private readonly IVoucherRepository repository = null!;
        private readonly IItemService _itemService;
        private readonly IRedisHelper _redis;

        public VoucherService(
            IVoucherRepository repo,
            IItemService itemService,
            IPointRuleService pointRuleService,
            IRedisHelper redis) : base()
        {
            repository = repo;
            _itemService = itemService;
            _redis = redis;
        }

        public async Task<MVVoucher> AddVoucher(string orgId, MVoucher vc)
        {
            repository!.SetCustomOrgId(orgId);
            vc.Status = "Active";
            vc.IsUsed = "NO";

            var r = new MVVoucher()
            {
                Status = "OK",
                Description = "Success",
            };

            if (string.IsNullOrEmpty(vc.VoucherNo))
            {
                r.Status = "VOUCHER_NO_MISSING";
                r.Description = $"Voucher number is missing!!!";

                return r;
            }

            var isDocNoExist = await repository.IsVoucherNoExist(vc.VoucherNo!);
            if (isDocNoExist)
            {
                r.Status = "DOC_NO_DUPLICATE";
                r.Description = $"Voucher number [{vc.VoucherNo}] is duplicate!!!";

                return r;
            }

            //TODO : Add point & privilege deduction logic here

            var result = await repository!.AddVoucher(vc);
            r.Voucher = result;

            return r;
        }

        public async Task<List<MVoucher>> GetVouchers(string orgId, VMVoucher param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetVouchers(param);

            return result;
        }

        public async Task<int> GetVoucherCount(string orgId, VMVoucher param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetVoucherCount(param);

            return result;
        }

        public async Task<MVVoucher> DeleteVoucherById(string orgId, string voucherId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVVoucher()
            {
                Status = "SUCCESS",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(voucherId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Voucher ID [{voucherId}] format is invalid";

                return r;
            }

            var result = await repository!.DeleteVoucherById(voucherId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Voucher ID [{voucherId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Voucher = result;
            return r;
        }

        public async Task<MVVoucher> UpdateVoucherStatusById(string orgId, string voucherId, string status)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVVoucher()
            {
                Status = "SUCCESS",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(voucherId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Voucher ID [{voucherId}] format is invalid";

                return r;
            }

            var result = await repository!.UpdateVoucherStatusById(voucherId, status);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Voucher ID [{voucherId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Voucher = result;
            return r;
        }

        public async Task<MVVoucher> GetVoucherById(string orgId, string voucherId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVVoucher()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(voucherId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Voucher ID [{voucherId}] format is invalid";

                return r;
            }

            var result = await repository!.GetVoucherById(voucherId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Voucher ID [{voucherId}] not found for the organization [{orgId}]";
            }

            r.Voucher = result;

            return r;
        }
    }
}
