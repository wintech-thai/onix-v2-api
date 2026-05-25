using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;

namespace Its.Onix.Api.Database.Repositories
{
    public class PaymentDocumentRepository : BaseRepository, IPaymentDocumentRepository
    {
        public PaymentDocumentRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<bool> IsPaymentRequestIdExist(string paymentDocumentId)
        {
            var exists = await context!.PaymentDocuments!.AsExpandable().AnyAsync(p => p!.Id!.Equals(paymentDocumentId));
            return exists;
        }

        //=== Start V2 ===
        public IQueryable<MPaymentDocument> GetPaymentDocumentSelection()
        {
            var query =
                from pd in context!.PaymentDocuments

                join mc in context.Merchants!
                    on pd.MerchantId equals mc.Id.ToString() into merchants
                from merchant in merchants.DefaultIfEmpty()

                join fd in context.FileDocuments!
                    on pd.FileDocumentId equals fd.Id.ToString() into fileDocuments
                from fileDocument in fileDocuments.DefaultIfEmpty()

                select new { pd, merchant, fileDocument };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MPaymentDocument
            {
                Id = x.pd.Id,
                OrgId = x.pd.OrgId,
                MerchantId = x.pd.MerchantId,
                PaymentRequestId = x.pd.PaymentRequestId,
                Description = x.pd.Description,
                Currency = x.pd.Currency,
                Tags = x.pd.Tags,
                Status = x.pd.Status,
                Direction = x.pd.Direction,
                TxAmount = x.pd.TxAmount,
                TxAmountDecimal = x.pd.TxAmountDecimal,
                FileDocumentId = x.pd.FileDocumentId,
                UploadedFilePath = x.pd.FileDocumentId,

                PayInBankAccountId = x.pd.PayInBankAccountId,
                PayInBankCode = x.pd.PayInBankCode,
                PayInBankAccountNo = x.pd.PayInBankAccountNo,
                PayInBankAccountName = x.pd.PayInBankAccountName,

                PayOutBankCode = x.pd.PayOutBankCode,
                PayOutBankAccountNo = x.pd.PayOutBankAccountNo,
                PayOutBankAccountName = x.pd.PayOutBankAccountName,

                FromBankCode = x.pd.FromBankCode,
                FromBankAccountNo = x.pd.FromBankAccountNo,
                FromBankAccountName = x.pd.FromBankAccountName,
                
                ProcessingMessages = x.pd.ProcessingMessages,

                CreatedDate = x.pd.CreatedDate,
                MerchantName = x.merchant.Name,
                MerchantCode = x.merchant.Code,
                TxAmountStr = x.pd.TxAmountDecimal.ToString(),

                MimeType = x.fileDocument.MimeType,
                DocumentType = x.fileDocument.DocumentType,
            });
        }

        public async Task<List<MPaymentDocument>> GetPaymentDocuments(VMPaymentDocument param)
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

            var predicate = PaymentDocumentPredicate(param!);
            var result = await GetPaymentDocumentSelection().AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.CreatedDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            return result;
        }

        public async Task<int> GetPaymentDocumentCount(VMPaymentDocument param)
        {
            var predicate = PaymentDocumentPredicate(param!);
            var result = await GetPaymentDocumentSelection().Where(predicate).AsExpandable().CountAsync();

            return result;
        }

        public async Task<MPaymentDocument?> GetPaymentDocumentById(string paymentDocId)
        {
            Guid id = Guid.Parse(paymentDocId);
            var u = await GetPaymentDocumentSelection().AsExpandable().Where(IsOrgMatchPredicate(id)).FirstOrDefaultAsync();
            return u;
        }

        private ExpressionStarter<MPaymentDocument> PaymentDocumentPredicate(VMPaymentDocument param)
        {
            var pd = IsOrgMatchPredicate(null);

            if ((param.Direction != null) && (param.Direction != ""))
            {
                var directionPd = PredicateBuilder.New<MPaymentDocument>();
                directionPd = directionPd.Or(p => p.Direction!.Equals(param.Direction));

                pd = pd.And(directionPd);
            }

            if ((param.Status != null) && (param.Status != ""))
            {
                var statusPd = PredicateBuilder.New<MPaymentDocument>();
                statusPd = statusPd.Or(p => p.Status!.Equals(param.Status));

                pd = pd.And(statusPd);
            }

            // FromDate
            if (param.FromDate.HasValue)
            {
                var fromDatePd = PredicateBuilder.New<MPaymentDocument>();
                fromDatePd = fromDatePd.Or(p => p.CreatedDate >= param.FromDate.Value);

                pd = pd.And(fromDatePd);
            }

            // ToDate
            if (param.ToDate.HasValue)
            {
                var toDatePd = PredicateBuilder.New<MPaymentDocument>();
                toDatePd = toDatePd.Or(p => p.CreatedDate <= param.ToDate.Value);

                pd = pd.And(toDatePd);
            }

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MPaymentDocument>();
                fullTextPd = fullTextPd.Or(p => p.MerchantCode!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.MerchantName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.PayInBankCode!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.PayInBankAccountNo!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.PayInBankAccountName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.TxAmountStr!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.FromBankCode!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.FromBankAccountNo!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.FromBankAccountName!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<MPaymentDocument> AddPaymentDocument(MPaymentDocument paymentDocument)
        {
            paymentDocument.OrgId = orgId;
            paymentDocument.CreatedDate = DateTime.UtcNow;

            await context!.PaymentDocuments!.AddAsync(paymentDocument);
            await context.SaveChangesAsync();

            return paymentDocument;
        }

        private ExpressionStarter<MPaymentDocument> IsOrgMatchPredicate(Guid? pmrId)
        {
            var pd = PredicateBuilder.New<MPaymentDocument>(true);
            if (orgId != "global")
            {
                //ต้องเอา orgId มา where ด้วย
                var orgPd = PredicateBuilder.New<MPaymentDocument>(true);
                orgPd = orgPd.And(p => p.OrgId!.Equals(orgId));
                pd = pd.And(orgPd);
            }

            if (pmrId != null)
            {
                //ต้องมีการเอา Id ของ payment ไปเช็คด้วย เพื่อดึงเฉพาะตัวนั้น ๆ ออกมา
                var pmrPd = PredicateBuilder.New<MPaymentDocument>(true);
                pmrPd = pmrPd.And(p => p.Id!.Equals(pmrId));
                pd = pd.And(pmrPd);
            }

            return pd;
        }

        public async Task<MPaymentDocument?> UpdatePaymentDocumentById(string paymentDocumentId, MPaymentDocument paymentDocument)
        {
            Guid id = Guid.Parse(paymentDocumentId);
            var existing = await context!.PaymentDocuments!.AsExpandable().Where(IsOrgMatchPredicate(id)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Tags = paymentDocument.Tags;
            }

            await context.SaveChangesAsync();
            return existing;
        }
    }
}