using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Services
{
    public class PointService : BaseService, IPointService
    {
        private readonly IPointRepository repository = null!;
        private readonly IEntityRepository _entityRepo;

        public PointService(
            IPointRepository repo,
            IEntityRepository entityRepo) : base()
        {
            repository = repo;
            _entityRepo = entityRepo;
        }

        private async Task<MPointBalance> GetCurrentBalance(string walletId)
        {
            var param = new VMPointBalance()
            {
                WalletId = walletId,
                BalanceType = "PointBalanceCurrent",
            };

            var bal = await repository.GetPointBalanceByWalletId(param);

            if (bal == null)
            {
                bal = new MPointBalance()
                {
                    StatCode = param.BalanceType,
                    WalletId = walletId,
                    BalanceDate = DateTime.UtcNow,
                    BalanceDateKey = "000000",
                    TxIn = 0,
                    TxOut = 0,
                    BalanceBegin = 0,
                    BalanceEnd = 0,
                    IsNew = true,
                };
            }
            else
            {
                bal.IsNew = false;
            }

            return bal;
        }

        private async Task<MPointBalance> GetDailyBalance(string walletId, long? currentAmt)
        {
            var amt = currentAmt;
            if (amt == null)
            {
                amt = 0;
            }

            var dateString = DateTime.UtcNow.ToString("yyyyMMdd");
            var param = new VMPointBalance()
            {
                WalletId = walletId,
                BalanceType = "PointBalanceDaily",
                DateKey = dateString,
            };

            var bal = await repository.GetPointBalanceByWalletId(param);

            if (bal == null)
            {
                bal = new MPointBalance()
                {
                    StatCode = param.BalanceType,
                    WalletId = walletId,
                    BalanceDate = DateTime.UtcNow,
                    BalanceDateKey = param.DateKey,
                    TxIn = 0,
                    TxOut = 0,
                    BalanceBegin = amt,
                    BalanceEnd = amt,
                    IsNew = true,
                };
            }
            else
            {
                bal.IsNew = false;    
            }

            return bal;
        }

        public async Task<MVPointTx> AddPoint(string orgId, MPointTx tx)
        {
            //ใช้วิธี : Optimistic Concurrency ในการแก้ race condition
            repository!.SetCustomOrgId(orgId);

            var r = new MVPointTx()
            {
                Status = "OK",
                Description = "Success",
            };

            if (tx.TxAmount == null || tx.TxAmount < 0)
            {
                r.Status = "INVALID_TX_AMOUNT";
                r.Description = "TxAmount must be greater than 0 for point addition!!!";

                return r;
            }

            var txAmt = tx.TxAmount;
            var currBal = await GetCurrentBalance(tx.WalletId!);

            //เอา balance ปัจจุบันมาก่อน

            var previousBal = currBal.BalanceEnd;
            tx.PreviousBalance = previousBal;

            currBal.TxIn = currBal.TxIn + txAmt;
            currBal.BalanceEnd = currBal.BalanceEnd + txAmt;
            currBal.BalanceDate = DateTime.UtcNow;

            var dailyBal = await GetDailyBalance(tx.WalletId!, previousBal);
            dailyBal.TxIn = dailyBal.TxIn + txAmt;
            dailyBal.BalanceEnd = dailyBal.BalanceEnd + txAmt;
            dailyBal.BalanceDate = DateTime.UtcNow;

            tx.TxType = 1;

            //ตรงนี้จะได้ balance ที่ลดหรือเพิ่มแล้ว
            tx.CurrentBalance = currBal.BalanceEnd;
            var result = await repository!.AddPointTxWithBalance(tx, currBal, dailyBal);

            r.Status = "OK";
            r.Description = "Success";
            r.PointTx = result;

            return r;
        }

        public async Task<MVPointTx> DeductPoint(string orgId, MPointTx tx)
        {
            //ใช้วิธี : Optimistic Concurrency ในการแก้ race condition
            repository.SetCustomOrgId(orgId);

            var r = new MVPointTx()
            {
                Status = "OK",
                Description = "Success",
            };

            if (tx.TxAmount == null || tx.TxAmount < 0)
            {
                r.Status = "INVALID_TX_AMOUNT";
                r.Description = "TxAmount must be greater than 0 for point deduction!!!";

                return r;
            }

            var txAmt = tx.TxAmount;
            var currBal = await GetCurrentBalance(tx.WalletId!);

            //เอา balance ปัจจุบันมาก่อน
            var previousBal = currBal.BalanceEnd;
            tx.PreviousBalance = previousBal;

            currBal.TxOut = currBal.TxOut + txAmt;
            currBal.BalanceEnd = currBal.BalanceEnd - txAmt;
            currBal.BalanceDate = DateTime.UtcNow;

            if (currBal.BalanceEnd < 0)
            {
                r.Status = "INVALID_BALANCE_LESS_THAN_ZERO";
                r.Description = "Balance cannot be less than 0!!!";
                return r;
            }

            var dailyBal = await GetDailyBalance(tx.WalletId!, previousBal);
            dailyBal.TxOut = dailyBal.TxOut + txAmt;
            dailyBal.BalanceEnd = dailyBal.BalanceEnd - txAmt;
            dailyBal.BalanceDate = DateTime.UtcNow;

            //ตรงนี้จะได้ balance ที่ลดหรือเพิ่มแล้ว
            tx.CurrentBalance = currBal.BalanceEnd;
            tx.TxType = -1;
            var result = await repository!.AddPointTxWithBalance(tx, currBal, dailyBal);

            r.Status = "OK";
            r.Description = "Success";
            r.PointTx = result;

            return r;
        }

        public async Task<List<MPointTx>> GetPointTxsByCustomerId(string orgId, string custId, VMPointTx param)
        {
            repository.SetCustomOrgId(orgId);

            var wallet = await repository!.GetWalletByCustomerId(custId);
            if (wallet == null)
            {
                return [];
            }

            param.WalletId = wallet.Id.ToString();
            var pointTxs = await repository.GetPointTxsByWalletId(param);

            return pointTxs;
        }

        public async Task<List<MPointTx>> GetPointTxsByWalletId(string orgId, VMPointTx param)
        {
            repository.SetCustomOrgId(orgId);
            var result = await repository.GetPointTxsByWalletId(param);
            return result;
        }

        public async Task<int> GetPointTxsCountByWalletId(string orgId, VMPointTx param)
        {
            repository.SetCustomOrgId(orgId);
            var result = await repository.GetPointTxsCountByWalletId(param);
            return result;
        }

        public async Task<MVPointBalance?> GetPointBalanceByWalletId(string orgId, VMPointBalance param)
        {
            repository.SetCustomOrgId(orgId);

            var r = new MVPointBalance()
            {
                Status = "OK",
                Description = "Success",
            };

            var balanceType = param.BalanceType;
            var dateKey = param.DateKey;

            if (string.IsNullOrEmpty(balanceType))
            {
                r.Status = "BALANCE_TYPE_MISSING";
                r.Description = "BalanceType need to be PointBalanceDaily or PointBalanceCurrent";

                return r;
            }

            if (balanceType == "PointBalanceDaily")
            {
                if (string.IsNullOrEmpty(dateKey))
                {
                    r.Status = "BALANCE_DATEKEY_MISSING";
                    r.Description = "DateKey is empty!!!";

                    return r;
                }
            }

            var result = await repository.GetPointBalanceByWalletId(param);
            r.PointBalance = result;

            return r;
        }

        public async Task<MVWallet> AddWallet(string orgId, MWallet wallet)
        {
            var r = new MVWallet();
            r.Status = "OK";
            r.Description = "Success";

            repository!.SetCustomOrgId(orgId);

            if (string.IsNullOrEmpty(wallet.Name))
            {
                r.Status = "INVALID_WALLET_NAME";
                r.Description = "Wallet name must not be blank!!!";

                return r;
            }

            var result = await repository!.AddWallet(wallet);
            r.Wallet = result;

            return r;
        }

        public async Task<List<MWallet>> GetWallets(string orgId, VMWallet param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetWallets(param);

            return result;
        }

        public async Task<int> GetWalletsCount(string orgId, VMWallet param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetWalletsCount(param);

            return result;
        }

        public async Task<MVWallet?> GetWalletByCustomerId(string orgId, string customerId)
        {
            var r = new MVWallet()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(customerId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Customer ID [{customerId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetWalletByCustomerId(customerId);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Wallet not found for customer ID [{customerId}], organization [{orgId}]";
            }

            r.Wallet = result;
            return r;
        }

        public async Task<MVWallet?> GetWalletById(string orgId, string walletId)
        {
            var r = new MVWallet()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(walletId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Wallet ID [{walletId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetWalletById(walletId);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Wallet ID [{walletId}] not found for the organization [{orgId}]";
            }

            r.Wallet = result;

            return r;
        }

        public async Task<MVWallet?> UpdateWalletById(string orgId, string walletId, MWallet wallet)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVWallet()
            {
                Status = "SUCCESS",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(walletId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Wallet ID [{walletId}] format is invalid";

                return r;
            }

            if (string.IsNullOrEmpty(wallet.Name))
            {
                r.Status = "INVALID_WALLET_NAME";
                r.Description = "Wallet name must not be blank!!!";

                return r;
            }

            var result = await repository!.UpdateWalletById(walletId, wallet);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Wallet ID [{walletId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Wallet = result;
            return r;
        }

        public async Task<MVWallet?> AttachCustomerToWalletById(string orgId, string walletId, string custId)
        {
            repository!.SetCustomOrgId(orgId);
            _entityRepo.SetCustomOrgId(orgId);

            var r = new MVWallet()
            {
                Status = "SUCCESS",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(walletId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Wallet ID [{walletId}] format is invalid";

                return r;
            }

            if (!ServiceUtils.IsGuidValid(custId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Customer ID [{custId}] format is invalid";

                return r;
            }

            var customer = _entityRepo.GetEntityById(custId!);
            if (customer == null)
            {
                r.Status = "CUSTOMER_NOTFOUND";
                r.Description = $"Customer ID [{custId}] not found!!!";

                return r;
            }

            var result = await repository!.AttachCustomerToWalletById(walletId, custId, customer);
            r.Wallet = result;

            return r;
        }

        public async Task<MVWallet?> DeleteWalletById(string orgId, string walletId)
        {
            var r = new MVWallet()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(walletId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Wallet ID [{walletId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = await repository!.DeleteWalletById(walletId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Wallet ID [{walletId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Wallet = m;

            return r;
        }

        public MVWallet ValidateResponseData(string orgId, string customerId, List<MPointTx> items)
        {
            var r = new MVWallet()
            {
                Status = "OK",
                Description = "Success"
            };

            var wallet = repository!.GetWalletByCustomerId(customerId).Result;

            foreach (var pointTx in items)
            {
                var responseOrgId = pointTx.OrgId;
                var responseWalletId = pointTx.WalletId;

                if (responseOrgId != orgId)
                {
                    r.Status = "ERROR_ORG_ID_MISMATCH";
                    r.Description = "Organization ID of data is different from requested!!!";
                    return r;
                }

                if (responseWalletId != wallet!.Id.ToString())
                {
                    r.Status = "ERROR_CUST_ID_MISMATCH";
                    r.Description = "Customer ID of data is different from requested!!!";
                    return r;
                }
            }

            return r;
        }

        public MVWallet ValidateResponseData(string orgId, string customerId, MVWallet responseData)
        {
            var r = new MVWallet()
            {
                Status = "OK",
                Description = "Success"
            };

            var responseWallet = responseData.Wallet!;
            var responseOrgId = responseWallet.OrgId;
            var responseCustomerId = responseWallet.CustomerId;

            if (responseOrgId != orgId)
            {
                r.Status = "ERROR_ORG_ID_MISMATCH";
                r.Description = "Organization ID of data is different from requested!!!";
                return r;
            }

            if (responseCustomerId != customerId)
            {
                r.Status = "ERROR_CUST_ID_MISMATCH";
                r.Description = "Customer ID of data is different from requested!!!";
                return r;
            }

            return r;
        }
    }
}
