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
        private readonly IPointService _pointService;
        private readonly IRedisHelper _redis;

        public VoucherService(
            IVoucherRepository repo,
            IItemService itemService,
            IPointService pointService,
            IRedisHelper redis) : base()
        {
            repository = repo;
            _itemService = itemService;
            _pointService = pointService;
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

            var walletResult = await _pointService.GetWalletByCustomerId(orgId, vc.CustomerId!);
            if (walletResult!.Status != "OK")
            {
                r.Status = walletResult.Status;
                r.Description = walletResult.Description;

                return r;
            }

            //เปลี่ยนให้เป็น await จะดีขึ้น
            var product = _itemService.GetItemById(orgId, vc.PrivilegeId!);
            if (product == null)
            {
                r.Status = "PRIVILEGE_NOTFOUND";
                r.Description = $"Privilege ID [{vc.PrivilegeId}] not found for the organization [{orgId}]";

                return r;
            }

            //ให้ตระหนักไว้ว่าจะเกิด race condition ได้นะ ถ้ามีการ redeem พร้อมๆ กันหลายๆ request
            var privilegeQty = 1;
            if (product.CurrentBalance < privilegeQty)
            {
                r.Status = "PRIVILEGE_QTY_NOT_ENOUGH";
                r.Description = $"Privilege ID [{vc.PrivilegeId}] quantity is not enough for the organization [{orgId}]";

                return r;
            }

            if (walletResult.Wallet!.PointBalance < product.PointRedeem)
            {
                r.Status = "WALLET_BALANCE_NOT_ENOUGH";
                r.Description = $"Wallet ID [{vc.WalletId}] balance is not enough for the organization [{orgId}]";

                return r;
            }

            var walletId = walletResult.Wallet.Id.ToString();
            var privilegeId = vc.PrivilegeId!;

            //Acquire wallet lock here to prevent race condition
            using var redWalletLock = await _redis.AcquireRedLockAsync(
                $"lock:wallet:{walletId}",  // resource
                TimeSpan.FromSeconds(2)   // lock expiry
            );
            if (!redWalletLock.IsAcquired)
            {
                r.Status = "WALLET_LOCK_NOT_ACQUIRED";
                r.Description = $"Could not acquire lock for wallet ID [{walletId}] to process voucher redemption. Please try again.";
                return r;
            }

            //Acquire privilege lock here to prevent race condition
            using var redPrivilegeLock = await _redis.AcquireRedLockAsync(
                $"lock:privilege:{privilegeId}",  // resource
                TimeSpan.FromSeconds(2)   // lock expiry
            );
            if (!redPrivilegeLock.IsAcquired)
            {
                r.Status = "PRIVILEGE_LOCK_NOT_ACQUIRED";
                r.Description = $"Could not acquire lock for privilege ID [{privilegeId}] to process voucher redemption. Please try again.";
                return r;
            }

            //เอาราคามาจาก privilege เองเลย
            var prefix = ServiceUtils.GenerateSecureRandomString(2).ToUpper();
            var vcNo = ServiceUtils.CreateOTP(5);
            
            vc.RedeemPrice = product.PointRedeem;
            vc.WalletId = walletResult.Wallet!.Id.ToString();
            vc.VoucherNo = $"{prefix}-{vcNo}";
            vc.Pin = ServiceUtils.CreateOTP(6);

            var privilegeTx = new MItemTx()
            {
                OrgId = orgId,
                ItemId = vc.PrivilegeId,
                TxType = -1, //Deduct
                TxAmount = privilegeQty,
                Description = $"Deduct privilege quantity for voucher [{vc.VoucherNo}]",
                Tags = $"voucher={vc.VoucherNo}",
            };

            //เปลี่ยนให้เป็น await จะดีขึ้น
            var privilegeDeductStatus = _itemService.DeductItemQuantity(orgId, privilegeTx);
            if (privilegeDeductStatus.Status != "OK")
            {
                r.Status = privilegeDeductStatus.Status;
                r.Description = privilegeDeductStatus.Description;

                return r;
            }

            var pointTx = new MPointTx()
            {
                OrgId = orgId,
                TxType = 2, //Out
                TxAmount = product.PointRedeem,
                Description = $"Deduct point(s) for voucher [{vc.VoucherNo}]",
                Tags = $"voucher={vc.VoucherNo}",
            };

            var pointDeductStatus = await _pointService.DeductPoint(orgId, pointTx);
            if (pointDeductStatus.Status != "OK")
            {
                r.Status = pointDeductStatus.Status;
                r.Description = pointDeductStatus.Description;

                return r;
            }

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
