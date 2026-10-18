using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using Microsoft.EntityFrameworkCore;

namespace Its.Onix.Api.Database.Repositories
{
    public class PaymentGatewayRepository : BaseRepository, IPaymentGatewayRepository
    {
        public PaymentGatewayRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<MPaymentGateway?> GetPaymentGatewayByCode(string code)
        {
            var result = await context!.PaymentGateways!.Where(x => x.OrgId!.Equals(orgId) && x.Code!.Equals(code)).FirstOrDefaultAsync();
            return result;
        }

        public async Task<MPaymentGateway> AddPaymentGateway(MPaymentGateway paymentGateway)
        {
            paymentGateway.Id = Guid.NewGuid();
            paymentGateway.CreatedDate = DateTime.UtcNow;
            paymentGateway.OrgId = orgId;

            context!.PaymentGateways!.Add(paymentGateway);
            await context.SaveChangesAsync();

            return paymentGateway;
        }

        public async Task<MPaymentGateway?> DeletePaymentGatewayById(string paymentGatewayId)
        {
            Guid id = Guid.Parse(paymentGatewayId);

            var r = await context!.PaymentGateways!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).FirstOrDefaultAsync();
            if (r != null)
            {
                context!.PaymentGateways!.Remove(r);
                await context.SaveChangesAsync();
            }

            return r;
        }

        private IQueryable<MPaymentGateway> PaymentGatewayQuery(VMPaymentGateway param)
        {
            IQueryable<MPaymentGateway> query = context!.PaymentGateways!.Where(p => p.OrgId!.Equals(orgId));

            if ((param.Scheme != null) && (param.Scheme != ""))
            {
                query = query.Where(p => p.Scheme!.Equals(param.Scheme));
            }

            if ((param.FullTextSearch != null) && (param.FullTextSearch != ""))
            {
                query = query.Where(p =>
                    p.Code!.Contains(param.FullTextSearch) ||
                    p.Description!.Contains(param.FullTextSearch) ||
                    p.Tags!.Contains(param.FullTextSearch));
            }

            return query;
        }

        public async Task<IEnumerable<MPaymentGateway>> GetPaymentGateways(VMPaymentGateway param)
        {
            var limit = 0;
            var offset = 0;

            if (param.Offset > 0)
            {
                offset = param.Offset - 1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var arr = await PaymentGatewayQuery(param)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return arr;
        }

        public async Task<MPaymentGateway?> GetPaymentGatewayById(string paymentGatewayId)
        {
            Guid id = Guid.Parse(paymentGatewayId);
            var result = await context!.PaymentGateways!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefaultAsync();

            return result;
        }

        public async Task<int> GetPaymentGatewayCount(VMPaymentGateway param)
        {
            var cnt = await PaymentGatewayQuery(param).CountAsync();

            return cnt;
        }

        public async Task<MPaymentGateway?> UpdatePaymentGatewayById(string paymentGatewayId, MPaymentGateway paymentGateway)
        {
            Guid id = Guid.Parse(paymentGatewayId);
            var result = await context!.PaymentGateways!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefaultAsync();

            if (result != null)
            {
                result.Code = paymentGateway.Code;
                result.Description = paymentGateway.Description;
                result.EndpointBaseUrl = paymentGateway.EndpointBaseUrl;
                result.EndpointOrgId = paymentGateway.EndpointOrgId;
                result.EndpointMerchantId = paymentGateway.EndpointMerchantId;
                result.ApiKey = paymentGateway.ApiKey;
                result.Tags = paymentGateway.Tags;
                //Scheme กำหนดได้แค่ตอน Add เท่านั้น แก้ไขทีหลังไม่ได้ จึงไม่แก้ตรงนี้

                await context!.SaveChangesAsync();
            }

            return result;
        }
    }
}
