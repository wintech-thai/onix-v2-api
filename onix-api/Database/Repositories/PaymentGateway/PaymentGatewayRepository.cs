using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using Microsoft.EntityFrameworkCore;
using LinqKit;

namespace Its.Onix.Api.Database.Repositories
{
    public class PaymentGatewayRepository : BaseRepository, IPaymentGatewayRepository
    {
        public PaymentGatewayRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public Task<MPaymentGateway?> GetPaymentGatewayByCode(string code)
        {
            var result = context!.PaymentGateways!.Where(x => x.OrgId!.Equals(orgId) && x.Code!.Equals(code)).FirstOrDefaultAsync();
            return result!;
        }

        public MPaymentGateway AddPaymentGateway(MPaymentGateway paymentGateway)
        {
            paymentGateway.Id = Guid.NewGuid();
            paymentGateway.CreatedDate = DateTime.UtcNow;
            paymentGateway.OrgId = orgId;

            context!.PaymentGateways!.Add(paymentGateway);
            context.SaveChanges();

            return paymentGateway;
        }

        public MPaymentGateway? DeletePaymentGatewayById(string paymentGatewayId)
        {
            Guid id = Guid.Parse(paymentGatewayId);

            var r = context!.PaymentGateways!.Where(x => x.OrgId!.Equals(orgId) && x.Id.Equals(id)).FirstOrDefault();
            if (r != null)
            {
                context!.PaymentGateways!.Remove(r);
                context.SaveChanges();
            }

            return r;
        }

        private ExpressionStarter<MPaymentGateway> PaymentGatewayPredicate(VMPaymentGateway param)
        {
            var pd = PredicateBuilder.New<MPaymentGateway>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.Scheme != null) && (param.Scheme != ""))
            {
                var schemePd = PredicateBuilder.New<MPaymentGateway>();
                schemePd = schemePd.Or(p => p.Scheme!.Equals(param.Scheme));

                pd = pd.And(schemePd);
            }

            if ((param.FullTextSearch != null) && (param.FullTextSearch != ""))
            {
                var fullTextPd = PredicateBuilder.New<MPaymentGateway>();
                fullTextPd = fullTextPd.Or(p => p.Code!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public IEnumerable<MPaymentGateway> GetPaymentGateways(VMPaymentGateway param)
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

            var predicate = PaymentGatewayPredicate(param!);
            var arr = context!.PaymentGateways!.AsExpandable().Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }

        public Task<MPaymentGateway?> GetPaymentGatewayById(string paymentGatewayId)
        {
            Guid id = Guid.Parse(paymentGatewayId);
            var result = context!.PaymentGateways!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefaultAsync();

            return result!;
        }

        public int GetPaymentGatewayCount(VMPaymentGateway param)
        {
            var predicate = PaymentGatewayPredicate(param);
            var cnt = context!.PaymentGateways!.AsExpandable().Where(predicate).Count();

            return cnt;
        }

        public MPaymentGateway? UpdatePaymentGatewayById(string paymentGatewayId, MPaymentGateway paymentGateway)
        {
            Guid id = Guid.Parse(paymentGatewayId);
            var result = context!.PaymentGateways!.Where(x => x.OrgId!.Equals(orgId) && x.Id!.Equals(id)).FirstOrDefault();

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

                context!.SaveChanges();
            }

            return result!;
        }
    }
}
