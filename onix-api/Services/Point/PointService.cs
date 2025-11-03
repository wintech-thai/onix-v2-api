using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using System.Threading.Tasks;

namespace Its.Onix.Api.Services
{
    public class PointService : BaseService, IPointService
    {
        private readonly IPointRepository repository = null!;

        public PointService(IPointRepository repo) : base()
        {
            repository = repo;
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
    }
}
