using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class MerchantSummary
    {
        public int MerchantCount { get; set; }
        public List<MAggregateData> MerchantCountByStatus { get; set; }

        public MerchantSummary()
        {
            MerchantCountByStatus = [];
        }
    }
}
