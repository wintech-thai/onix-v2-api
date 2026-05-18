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
                    on pr.MerchantId2 equals mc.Id into merchants
                from merchant in merchants.DefaultIfEmpty()

                select new { pr, merchant };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MPaymentRequest
            {
                Id = x.pr.Id,
                OrgId = x.pr.OrgId,
                RefId = x.pr.RefId,
                RefId1 = x.pr.RefId1,
                RefId2 = x.pr.RefId2,
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
                MerchantId2 = x.pr.MerchantId2,
                PaymentTxId = x.pr.PaymentTxId,
                GeneratedAmount = x.pr.GeneratedAmount,
                ResponseData = x.pr.ResponseData,
                ProcessingMessages = x.pr.ProcessingMessages,
                CreatedDate = x.pr.CreatedDate,

                PayinBankAccountId = x.pr.PayinBankAccountId,
                PayinBankCode = x.pr.PayinBankCode,
                PayinBankAccountNo = x.pr.PayinBankAccountNo,
                PayinBankAccountName = x.pr.PayinBankAccountName,
                PayinPromptPayId = x.pr.PayinPromptPayId,
                PayinAccountType = x.pr.PayinAccountType,
                PayinAccountLevel = x.pr.PayinAccountLevel,

                MerchantName = x.merchant != null ? x.merchant.Name : null,
                MerchantCode = x.merchant != null ? x.merchant.Code : null,
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
            var u = await GetPaymentRequestSelection().AsExpandable().Where(IsOrgMatchPredicate(id)).FirstOrDefaultAsync();
            return u;
        }

        private ExpressionStarter<MPaymentRequest> PaymentRequestPredicate(VMPaymentRequest param)
        {
            var pd = IsOrgMatchPredicate(null);

            if ((param.Direction != null) && (param.Direction != ""))
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

            // FromDate
            if (param.FromDate.HasValue)
            {
                var fromDatePd = PredicateBuilder.New<MPaymentRequest>();
                fromDatePd = fromDatePd.Or(p => p.CreatedDate >= param.FromDate.Value);

                pd = pd.And(fromDatePd);
            }

            // ToDate
            if (param.ToDate.HasValue)
            {
                var toDatePd = PredicateBuilder.New<MPaymentRequest>();
                toDatePd = toDatePd.Or(p => p.CreatedDate <= param.ToDate.Value);

                pd = pd.And(toDatePd);
            }

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MPaymentRequest>();
                fullTextPd = fullTextPd.Or(p => p.MerchantCode!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.MerchantName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.RefId!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.RefId1!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.RefId2!.Contains(param.FullTextSearch));
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

        private ExpressionStarter<MPaymentRequest> IsOrgMatchPredicate(Guid? pmrId)
        {
            var pd = PredicateBuilder.New<MPaymentRequest>(true);
            if (orgId != "global")
            {
                //ต้องเอา orgId มา where ด้วย
                var orgPd = PredicateBuilder.New<MPaymentRequest>(true);
                orgPd = orgPd.And(p => p.OrgId!.Equals(orgId));
                pd = pd.And(orgPd);
            }

            if (pmrId != null)
            {
                //ต้องมีการเอา Id ของ payment ไปเช็คด้วย เพื่อดึงเฉพาะตัวนั้น ๆ ออกมา
                var pmrPd = PredicateBuilder.New<MPaymentRequest>(true);
                pmrPd = pmrPd.And(p => p.Id!.Equals(pmrId));
                pd = pd.And(pmrPd);
            }

            return pd;
        }

        public async Task<MPaymentRequest?> UpdatePaymentRequestById(string paymentRequestId, MPaymentRequest paymentRequest)
        {
            Guid id = Guid.Parse(paymentRequestId);
            var existing = await context!.PaymentRequests!.AsExpandable().Where(IsOrgMatchPredicate(id)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Tags = paymentRequest.Tags;
            }

            await context.SaveChangesAsync();
            return existing;
        }
    }
}