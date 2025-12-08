using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Utils;
using System.Text.Json;
using System.Text;
using System.Web;

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

            var productPoint = product.PointRedeem;
            if (productPoint == null)
            {
                //ถ้าเป็น null ให้ตั้งเป็น 0, ไม่จะบวกลบ point ไม่ได้
                productPoint = 0;
            }

            if (walletResult.Wallet!.PointBalance < productPoint)
            {
                r.Status = "WALLET_BALANCE_NOT_ENOUGH";
                r.Description = $"Wallet ID [{vc.WalletId}] balance is not enough for the organization [{orgId}]";

                return r;
            }

            var walletId = walletResult.Wallet.Id.ToString();
            var privilegeId = vc.PrivilegeId!;

            var vp = new VoucherParam()
            {
                CustomerId = vc.CustomerId!,
                PrivilegeId = privilegeId,
                WalletId = walletId!,
            };

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
            
            vc.RedeemPrice = productPoint;
            vc.WalletId = walletResult.Wallet!.Id.ToString();
            vc.VoucherNo = $"{prefix}-{vcNo}";
            vc.Pin = ServiceUtils.CreateOTP(6);
            vc.StartDate = product.EffectiveDate;
            vc.EndDate = product.ExpireDate;
            vc.Barcode = $"{vc.VoucherNo}-{vc.Pin}";

            var privilegeTx = new MItemTx()
            {
                ItemId = vc.PrivilegeId,
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
                WalletId = walletId,
                TxAmount = productPoint,
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

            vp.ItemTransaction = privilegeDeductStatus.ItemTx;
            vp.PointTransaction = pointDeductStatus.PointTx;
            var vpJson = JsonSerializer.Serialize(vp);

            vc.VoucherParams = vpJson;
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

        public async Task<MVVoucher> VerifyVoucherByBarcode(string orgId, string barcode)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVVoucher()
            {
                Status = "OK",
                Description = "Success"
            };

            var result = await repository!.VerifyVoucherByBarcode(barcode);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Voucher with barcode [{barcode}] not found for the organization [{orgId}]";
            }

            r.Voucher = result;

            return r;
        }

        public async Task<MVVoucher> VerifyVoucherByPin(string orgId, string voucherNo, string pin)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVVoucher()
            {
                Status = "OK",
                Description = "Success"
            };

            var result = await repository!.VerifyVoucherByPin(voucherNo, pin);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Voucher with voucher number [{voucherNo}] and pin [{pin}] not found for the organization [{orgId}]";
            }

            r.Voucher = result;

            return r;
        }

        public async Task<MVVoucher> UpdateVoucherUsedFlagById(string orgId, string voucherId, string isUsed)
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

            var vc = await repository!.GetVoucherById(voucherId);
            if (vc == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Voucher ID [{voucherId}] not found for the organization [{orgId}]";

                return r;
            }

            if (vc.Status == "Disable")
            {
                r.Status = "VOUCHER_DISABLED";
                r.Description = $"Voucher ID [{voucherId}] is disable!!!";

                return r;
            }

            var result = await repository!.UpdateVoucherUsedFlagById(voucherId, isUsed);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Voucher ID [{voucherId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Voucher = result;
            return r;
        }

        public async Task<MVVoucher> UpdateVoucherUsedFlagById(string orgId, string voucherId, string pin, string isUsed)
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

            var vc = await repository!.GetVoucherById(voucherId);
            if (vc == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Voucher ID [{voucherId}] with pin [{pin}] not found for the organization [{orgId}]";

                return r;
            }

            if (vc.Status == "Disable")
            {
                r.Status = "VOUCHER_DISABLED";
                r.Description = $"Voucher ID [{voucherId}] is disable!!!";

                return r;
            }

            var result = await repository!.UpdateVoucherUsedFlagById(voucherId, pin, isUsed);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Voucher ID [{voucherId}] with pin [{pin}] not found for the organization [{orgId}]";

                return r;
            }

            r.Voucher = result;
            return r;
        }

        public async Task<MVVoucher> GetVoucherVerifyQrUrl(string id, string voucherId)
        {
            repository.SetCustomOrgId(id);

            var r = new MVVoucher()
            {
                Status = "OK",
                Description = "Success",
                Voucher = new MVoucher() {}
            };

            if (!ServiceUtils.IsGuidValid(voucherId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Voucher ID [{voucherId}] format is invalid";

                return r;
            }

            var voucher = await repository!.GetVoucherById(voucherId);
            if (voucher == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Voucher ID [{voucherId}] not found for the organization [{id}]";

                return r;
            }

            var apiDomain = "api";
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";
            if (environment != "Production")
            {
                apiDomain = "api-dev";
            }

            var verifyUrl = $"https://{apiDomain}.please-scan.com/api/Voucher/org/{id}/action/ScanVoucherByPin/{voucherId}";
            r.Voucher.VoucherVerifyUrl = verifyUrl;

            return r;
        }

        public async Task<MVVoucher> GetVoucherVerifyUrl(string id, string voucherId, bool withData)
        {
            repository.SetCustomOrgId(id);

            var r = new MVVoucher()
            {
                Status = "OK",
                Description = "Success",
                Voucher = new MVoucher() {}
            };

            var dataUrlSafe = "";
            if (withData)
            {
                if (!ServiceUtils.IsGuidValid(voucherId))
                {
                    r.Status = "UUID_INVALID";
                    r.Description = $"Voucher ID [{voucherId}] format is invalid";

                    return r;
                }

                var voucher = await repository!.GetVoucherById(voucherId);
                if (voucher == null)
                {
                    r.Status = "NOTFOUND";
                    r.Description = $"Voucher ID [{voucherId}] not found for the organization [{id}]";

                    return r;
                }

                var jsonString = JsonSerializer.Serialize(voucher);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
                string jsonStringB64 = Convert.ToBase64String(jsonBytes);

                dataUrlSafe = HttpUtility.UrlEncode(jsonStringB64);
            }

            var verifyDomain = "verify";
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";
            if (environment != "Production")
            {
                verifyDomain = "verify-dev";
            }

            var verifyUrl = $"https://{verifyDomain}.please-scan.com/voucher?org={id}&theme=default&data={dataUrlSafe}";
            r.Voucher.VoucherVerifyUrl = verifyUrl;

            return r;
        }
    }
}
