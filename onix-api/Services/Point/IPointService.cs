using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public interface IPointService
    {
        public Task<MVPointTx> AddPoint(string orgId, MPointTx tx);
        public Task<MVPointTx> DeductPoint(string orgId, MPointTx tx);

        public Task<List<MPointTx>> GetPointTxsByWalletId(string orgId, VMPointTx param);
        public Task<int> GetPointTxsCountByWalletId(string orgId, VMPointTx param);

        public Task<MVPointBalance?> GetPointBalanceByWalletId(string orgId, VMPointBalance param);
    }
}
