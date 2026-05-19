using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class PaymentTransactionRepository : BaseRepository, IPaymentTransactionRepository
    {
        public PaymentTransactionRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<bool> IsPaymentRequestIdExist(string paymentTransactionId)
        {
            var exists = await context!.PaymentTransactions!.AsExpandable().AnyAsync(p => p!.PaymentRequestId!.Equals(paymentTransactionId));
            return exists;
        }

        //=== Start V2 ===
        public IQueryable<MPaymentTransaction> GetPaymentTransactionSelection()
        {
            var query =
                from pt in context!.PaymentTransactions
                select new { pt };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MPaymentTransaction
            {
                Id = x.pt.Id,
                OrgId = x.pt.OrgId,
                MerchantId = x.pt.MerchantId,
                PaymentRequestId = x.pt.PaymentRequestId,
                Description = x.pt.Description,
                Currency = x.pt.Currency,
                Tags = x.pt.Tags,
                Status = x.pt.Status,
                Direction = x.pt.Direction,
                TxAmount = x.pt.TxAmount,

                PayInFeePct = x.pt.PayInFeePct,
                PayInFee = x.pt.PayInFee,
                PayOutFeePct = x.pt.PayOutFeePct,
                PayOutFee = x.pt.PayOutFee,
                PayInTotalAmount = x.pt.PayInTotalAmount,
                PayOutTotalAmount = x.pt.PayOutTotalAmount,
                PayInBankAccountId = x.pt.PayInBankAccountId,
                PayInBankCode = x.pt.PayInBankCode,
                PayInBankAccountNo = x.pt.PayInBankAccountNo,
                PayInBankAccountName = x.pt.PayInBankAccountName,
                PayOutBankCode = x.pt.PayOutBankCode,
                PayOutBankAccountNo = x.pt.PayOutBankAccountNo,
                PayOutBankAccountName = x.pt.PayOutBankAccountName,
                
                FromBankCode = x.pt.FromBankCode,
                FromBankAccountNo = x.pt.FromBankAccountNo,
                FromBankAccountName = x.pt.FromBankAccountName,
                
                ProcessingMessages = x.pt.ProcessingMessages,
                RawInput = x.pt.RawInput,
                
                CreatedDate = x.pt.CreatedDate,
                MerchantName = x.pt.MerchantName,
                MerchantCode = x.pt.MerchantCode,
            });
        }

        public async Task<List<MPaymentTransaction>> GetPaymentTransactions(VMPaymentTransaction param)
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

            var predicate = PaymentTransactionPredicate(param!);
            var result = await GetPaymentTransactionSelection().AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.CreatedDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            return result;
        }

        public async Task<int> GetPaymentTransactionCount(VMPaymentTransaction param)
        {
            var predicate = PaymentTransactionPredicate(param!);
            var result = await context!.PaymentTransactions!.Where(predicate).AsExpandable().CountAsync();

            return result;
        }

        public async Task<MPaymentTransaction?> GetPaymentTransactionById(string paymentTxId)
        {
            Guid id = Guid.Parse(paymentTxId);
            var u = await GetPaymentTransactionSelection().AsExpandable().Where(IsOrgMatchPredicate(id)).FirstOrDefaultAsync();
            return u;
        }

        private ExpressionStarter<MPaymentTransaction> PaymentTransactionPredicate(VMPaymentTransaction param)
        {
            var pd = IsOrgMatchPredicate(null);

            if ((param.Direction != null) && (param.Direction != ""))
            {
                var directionPd = PredicateBuilder.New<MPaymentTransaction>();
                directionPd = directionPd.Or(p => p.Direction!.Equals(param.Direction));

                pd = pd.And(directionPd);
            }

            if ((param.Status != null) && (param.Status != ""))
            {
                var statusPd = PredicateBuilder.New<MPaymentTransaction>();
                statusPd = statusPd.Or(p => p.Status!.Equals(param.Status));

                pd = pd.And(statusPd);
            }

            // FromDate
            if (param.FromDate.HasValue)
            {
                var fromDatePd = PredicateBuilder.New<MPaymentTransaction>();
                fromDatePd = fromDatePd.Or(p => p.CreatedDate >= param.FromDate.Value);

                pd = pd.And(fromDatePd);
            }

            // ToDate
            if (param.ToDate.HasValue)
            {
                var toDatePd = PredicateBuilder.New<MPaymentTransaction>();
                toDatePd = toDatePd.Or(p => p.CreatedDate <= param.ToDate.Value);

                pd = pd.And(toDatePd);
            }

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MPaymentTransaction>();
                fullTextPd = fullTextPd.Or(p => p.MerchantCode!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.MerchantName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<MPaymentTransaction> AddPaymentTransaction(MPaymentTransaction paymentTransaction)
        {
            paymentTransaction.OrgId = orgId;
            paymentTransaction.CreatedDate = DateTime.UtcNow;

            await context!.PaymentTransactions!.AddAsync(paymentTransaction);
            await context.SaveChangesAsync();

            return paymentTransaction;
        }

        private ExpressionStarter<MPaymentTransaction> IsOrgMatchPredicate(Guid? pmrId)
        {
            var pd = PredicateBuilder.New<MPaymentTransaction>(true);
            if (orgId != "global")
            {
                //ต้องเอา orgId มา where ด้วย
                var orgPd = PredicateBuilder.New<MPaymentTransaction>(true);
                orgPd = orgPd.And(p => p.OrgId!.Equals(orgId));
                pd = pd.And(orgPd);
            }

            if (pmrId != null)
            {
                //ต้องมีการเอา Id ของ payment ไปเช็คด้วย เพื่อดึงเฉพาะตัวนั้น ๆ ออกมา
                var pmrPd = PredicateBuilder.New<MPaymentTransaction>(true);
                pmrPd = pmrPd.And(p => p.Id!.Equals(pmrId));
                pd = pd.And(pmrPd);
            }

            return pd;
        }

        public async Task<MPaymentTransaction?> UpdatePaymentTransactionById(string paymentTransactionId, MPaymentTransaction paymentTransaction)
        {
            Guid id = Guid.Parse(paymentTransactionId);
            var existing = await context!.PaymentTransactions!.AsExpandable().Where(IsOrgMatchPredicate(id)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Tags = paymentTransaction.Tags;
            }

            await context.SaveChangesAsync();
            return existing;
        }
    }
}