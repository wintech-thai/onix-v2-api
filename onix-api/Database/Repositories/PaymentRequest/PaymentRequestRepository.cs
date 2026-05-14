using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class PaymentRequestRepository : BaseRepository, IPaymentRequestRepository
    {
        public PaymentRequestRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<bool> IsRefIdExist(string refId)
        {
            var exists = await context!.PaymentRequests!.AsExpandable().AnyAsync(p => p!.RefId!.Equals(refId) && p!.OrgId!.Equals(orgId));
            return exists;
        }

        //=== Start V2 ===
        public IQueryable<MPaymentRequest> GetPaymentRequestSelection()
        {
            var query =
                from pr in context!.PaymentRequests

                join mc in context.Merchants!
                    on pr.MerchantId equals mc.Id.ToString() into merchants
                from merchant in merchants.DefaultIfEmpty()

                select new { pr, merchant };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MPaymentRequest
            {
                Id = x.pr.Id,
                OrgId = x.pr.OrgId,
                RefId = x.pr.RefId,
                Description = x.pr.Description,
                CustomerEmail = x.pr.CustomerEmail,
                CustomerPhone = x.pr.CustomerPhone,
                Currency = x.pr.Currency,
                BankCode = x.pr.BankCode,
                BankAccountNo = x.pr.BankAccountNo,
                BankAccountName = x.pr.BankAccountName,
                RequestedAmount = x.pr.RequestedAmount,
                Tags = x.pr.Tags,
                Status = x.pr.Status,
                Direction = x.pr.Direction,
                MerchantId = x.pr.MerchantId,
                PaymentTxId = x.pr.PaymentTxId,
                GeneratedAmount = x.pr.GeneratedAmount,
                ResponseData = x.pr.ResponseData,
                CreatedDate = x.pr.CreatedDate,

                MerchantName = x.merchant.Name,
                MerchantCode = x.merchant.Code,
            });
        }

        public async Task<List<MPaymentRequest>> GetPaymentRequests(VMPaymentRequest param)
        {
            var limit = 0;
            var offset = 0;

            //Param will never be null
            if (param.Offset > 0)
            {
                //Convert to zero base
                offset = param.Offset-1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var predicate = PaymentRequestPredicate(param!);
            var result = await GetPaymentRequestSelection().AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.CreatedDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            return result;
        }

        public async Task<int> GetPaymentRequestCount(VMPaymentRequest param)
        {
            var predicate = PaymentRequestPredicate(param!);
            var result = await context!.PaymentRequests!.Where(predicate).AsExpandable().CountAsync();

            return result;
        }

        public async Task<MPaymentRequest?> GetPaymentRequestById(string paymentRequestId)
        {
            Guid id = Guid.Parse(paymentRequestId);
            var u = await GetPaymentRequestSelection().AsExpandable().Where(p => p!.Id!.Equals(id) && IsOrgMatch(p)).FirstOrDefaultAsync();
            return u;
        }

        private ExpressionStarter<MPaymentRequest> PaymentRequestPredicate(VMPaymentRequest param)
        {
            var pd = PredicateBuilder.New<MPaymentRequest>();

            pd = pd.And(p => IsOrgMatch(p));

            if ((param.Status != null) && (param.Status != ""))
            {
                var directionPd = PredicateBuilder.New<MPaymentRequest>();
                directionPd = directionPd.Or(p => p.Direction!.Equals(param.Direction));

                pd = pd.And(directionPd);
            }

            if ((param.Status != null) && (param.Status != ""))
            {
                var statusPd = PredicateBuilder.New<MPaymentRequest>();
                statusPd = statusPd.Or(p => p.Status!.Equals(param.Status));

                pd = pd.And(statusPd);
            }

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MPaymentRequest>();
                fullTextPd = fullTextPd.Or(p => p.MerchantCode!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.MerchantName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.RefId!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<MPaymentRequest> AddPaymentRequest(MPaymentRequest paymentRequest)
        {
            paymentRequest.OrgId = orgId;
            paymentRequest.CreatedDate = DateTime.UtcNow;

            await context!.PaymentRequests!.AddAsync(paymentRequest);
            await context.SaveChangesAsync();

            return paymentRequest;
        }

        private bool IsOrgMatch(MPaymentRequest param)
        {
            if (orgId == "global")
            {
                return true;
            }

            var result = param.OrgId!.Equals(orgId);
            return result;
        }

        public async Task<MPaymentRequest?> UpdatePaymentRequestById(string paymentRequestId, MPaymentRequest paymentRequest)
        {
            Guid id = Guid.Parse(paymentRequestId);
            var existing = await context!.PaymentRequests!.AsExpandable().Where(p => p!.Id!.Equals(id) && IsOrgMatch(p)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Tags = paymentRequest.Tags;
            }

            await context.SaveChangesAsync();
            return existing;
        }
    }
}