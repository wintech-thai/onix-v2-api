using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Services
{
    public interface IPointTopUpService
    {
        Task<MVAccountDoc> CreatePointTopUp(string orgId, decimal amount);
    }
}
