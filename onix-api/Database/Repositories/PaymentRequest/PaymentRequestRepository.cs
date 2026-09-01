using LinqKit;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Data.Entity;
using System.Text.Json;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Database.Repositories
{
    public class PaymentRequestRepository : BaseRepository, IPaymentRequestRepository
    {
        private readonly IRedisHelper _redis;
        public PaymentRequestRepository(IDataContext ctx, IRedisHelper redis)
        {
            context = ctx;
            _redis = redis;
        }

        public async Task<bool> IsRefIdExist(string refId)
        {
            var exists = await context!.PaymentRequests!.AsExpandable().AnyAsync(p => p!.RefId1!.Equals(refId) && p!.OrgId!.Equals(orgId));
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
                RefId3 = x.pr.RefId3,
                Description = x.pr.Description,
                CustomerEmail = x.pr.CustomerEmail,
                CustomerPhone = x.pr.CustomerPhone,
                Currency = x.pr.Currency,
                BankCode = x.pr.BankCode,
                BankAccountNo = x.pr.BankAccountNo,
                PromptPayId = x.pr.PromptPayId,
                AccountType = x.pr.AccountType,
                BankAccountName = x.pr.BankAccountName,
                RequestedAmount = x.pr.RequestedAmount,
                Tags = x.pr.Tags,
                Status = x.pr.Status,
                StatusReason = x.pr.StatusReason,
                Direction = x.pr.Direction,
                MerchantId = x.pr.MerchantId,
                MerchantId2 = x.pr.MerchantId2,
                PaymentTxId = x.pr.PaymentTxId,
                GeneratedAmount = x.pr.GeneratedAmount,
                GeneratedAmountStr = x.pr.GeneratedAmountStr,
                ResponseData = x.pr.ResponseData,
                ProcessingMessages = x.pr.ProcessingMessages,
                CreatedDate = x.pr.CreatedDate,
                ExpireDate = x.pr.ExpireDate,

                PayinBankAccountId = x.pr.PayinBankAccountId,
                PayinBankCode = x.pr.PayinBankCode,
                PayinBankAccountNo = x.pr.PayinBankAccountNo,
                PayinBankAccountName = x.pr.PayinBankAccountName,
                PayinPromptPayId = x.pr.PayinPromptPayId,
                PayinAccountType = x.pr.PayinAccountType,
                PayinAccountLevel = x.pr.PayinAccountLevel,
                PayInFeePct = x.pr.PayInFeePct,
                PayinIsPeerToPeer = x.pr.PayinIsPeerToPeer,
                PayinPeer2PeerPayoutId = x.pr.PayinPeer2PeerPayoutId,

                PayerName = x.pr.PayerName,

                PayoutBankAccountId = x.pr.PayoutBankAccountId,
                PayoutBankCode = x.pr.PayoutBankCode,
                PayoutBankAccountNo = x.pr.PayoutBankAccountNo,
                PayoutBankAccountName = x.pr.PayoutBankAccountName,
                PayoutPromptPayId = x.pr.PayoutPromptPayId,
                PayoutAccountType = x.pr.PayoutAccountType,
                PayoutAccountLevel = x.pr.PayoutAccountLevel,
                PayoutFeePct = x.pr.PayoutFeePct,
                PayoutFeeDecimal = x.pr.PayoutFeeDecimal,
                TotalPayOutPendingPaidAmountDecimal = x.pr.TotalPayOutPendingPaidAmountDecimal,
                TotalPayOutPaidAmountDecimal = x.pr.TotalPayOutPaidAmountDecimal,
                PartialPayoutHistory = x.pr.PartialPayoutHistory,
                PayOutTotalAmountDecimalP2P = x.pr.PayOutTotalAmountDecimalP2P,
                PayoutPartialCountLimitP2P = x.pr.PayoutPartialCountLimitP2P,
                PayoutPartialCountP2P = x.pr.PayoutPartialCountP2P,

                PayOutTotalAmountDecimal = x.pr.PayOutTotalAmountDecimal,
                QrCode = x.pr.QrCode,
                QrCodeP2P = x.pr.QrCodeP2P,
                RejectReason = x.pr.RejectReason,

                IsPayInBankAccountOverride = x.pr.IsPayInBankAccountOverride,
                PayinBankCodeOverride = x.pr.PayinBankCodeOverride,
                PayinBankAccountNoOverride = x.pr.PayinBankAccountNoOverride,
                PayinBankAccountNameOverride = x.pr.PayinBankAccountNameOverride,
                PayinPromptPayIdOverride = x.pr.PayinPromptPayIdOverride,
                PayinAccountTypeOverride = x.pr.PayinAccountTypeOverride,

                MerchantName = x.merchant != null ? x.merchant.Name : null,
                MerchantCode = x.merchant != null ? x.merchant.Code : null,
                MerchantMinPayout = x.merchant != null ? x.merchant.PayoutMinAmount : null,
                MerchantMaxPayout = x.merchant != null ? x.merchant.PayoutMaxAmount : null,
                DiscardCent = x.merchant != null && x.merchant.DiscardCent,

                JobId = x.pr.JobId,
                StatusCode = x.pr.StatusCode,

                PayInSlipUploadCount = x.pr.PayInSlipUploadCount,
                PayInSlipUploads = x.pr.PayInSlipUploads,
                PayOutSlipUploadCount = x.pr.PayOutSlipUploadCount,
                PayOutSlipUploads = x.pr.PayOutSlipUploads,
                NoticeCount = x.pr.NoticeCount,
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

        private ExpressionStarter<MPaymentRequest> PaymentRequestPredicate2(VMPaymentRequest param)
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

            if ((param.MerchantId != null) && (param.MerchantId != ""))
            {
                var merchantIdPd = PredicateBuilder.New<MPaymentRequest>();
                merchantIdPd = merchantIdPd.Or(p => p.MerchantId!.Equals(param.MerchantId));

                pd = pd.And(merchantIdPd);
            }

            if ((param.PayinRequestId != null) && (param.PayinRequestId != ""))
            {
                var payinRequestId = Guid.Parse(param.PayinRequestId);
                var payinRequestIdPd = PredicateBuilder.New<MPaymentRequest>();
                payinRequestIdPd = payinRequestIdPd.Or(p => p.Id!.Equals(payinRequestId));

                pd = pd.And(payinRequestIdPd);
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

            if ((param.GeneratedAmountStr != null) && (param.GeneratedAmountStr != ""))
            {
                var amountStrPd = PredicateBuilder.New<MPaymentRequest>();
                amountStrPd = amountStrPd.Or(p => p.GeneratedAmountStr!.Equals(param.GeneratedAmountStr));

                pd = pd.And(amountStrPd);
            }

            if ((param.RefId1 != null) && (param.RefId1 != ""))
            {
                var refId1Pd = PredicateBuilder.New<MPaymentRequest>();
                refId1Pd = refId1Pd.Or(p => p.RefId1!.Equals(param.RefId1));

                pd = pd.And(refId1Pd);
            }

            if ((param.BankAccountId != null) && (param.BankAccountId != ""))
            {
                var bankAccountIdPd = PredicateBuilder.New<MPaymentRequest>();
                bankAccountIdPd = bankAccountIdPd.Or(p => p.PayinBankAccountId!.Equals(param.BankAccountId));

                pd = pd.And(bankAccountIdPd);
            }

            return pd;
        }

        public async Task<List<MPaymentRequest>> GetPendingPayOutRequests()
        {
            var oldOrgId = orgId;

            var param = new VMPaymentRequest()
            {
                Direction = "PayOut",
                Status = "Pending",
                FromDate = DateTime.UtcNow.AddDays(-1), //ให้ย้อนหลังแค่ 1 วันพอ เพราะอยากให้เคลียร์ payment out request ทุกวันอยู่แล้ว
            };

            orgId = "global"; //เอามาหมดทุก merchant

            var predicate = PaymentRequestPredicate2(param!);
            var result = await GetPaymentRequestSelection().AsExpandable()
            .Where(predicate)
            .OrderBy(e => e.CreatedDate) //น้อยไปมาก เอาตัวที่สร้างก่อนขึ้นก่อน
            .ToListAsync();

            orgId = oldOrgId;

            return result;
        }

        public async Task<List<MPaymentRequest>> GetPaymentRequestsForPaymentTx(VMPaymentRequest param)
        {
            var predicate = PaymentRequestPredicate2(param!);
            var result = await GetPaymentRequestSelection().AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.CreatedDate)
            .ToListAsync();

            return result;
        }

        public async Task<int> GetPaymentRequestCount(VMPaymentRequest param)
        {
            var predicate = PaymentRequestPredicate(param!);
            var result = await GetPaymentRequestSelection().Where(predicate).AsExpandable().CountAsync();

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

            // IsPeerToPeer — PayIn ใช้ PayinIsPeerToPeer, PayOut ใช้ยอด partial-payout เป็นตัวบอกว่าเป็น P2P
            if (param.IsPeerToPeer.HasValue)
            {
                var p2pPd = PredicateBuilder.New<MPaymentRequest>();
                if (param.IsPeerToPeer.Value)
                {
                    p2pPd = p2pPd.Or(p => p.PayinIsPeerToPeer == true);
                    p2pPd = p2pPd.Or(p => ((p.TotalPayOutPendingPaidAmountDecimal ?? 0) + (p.TotalPayOutPaidAmountDecimal ?? 0)) > 0);
                }
                else
                {
                    p2pPd = p2pPd.Or(p => (p.PayinIsPeerToPeer == null || p.PayinIsPeerToPeer == false)
                        && ((p.TotalPayOutPendingPaidAmountDecimal ?? 0) + (p.TotalPayOutPaidAmountDecimal ?? 0)) == 0);
                }

                pd = pd.And(p2pPd);
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
                fullTextPd = fullTextPd.Or(p => p.GeneratedAmountStr!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.PayinBankCode!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.PayinBankAccountNo!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.PayinBankAccountName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.PayinPromptPayId!.Contains(param.FullTextSearch));

                fullTextPd = fullTextPd.Or(p => p.PayinBankCodeOverride!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.PayinBankAccountNoOverride!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.PayinBankAccountNameOverride!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.PayinPromptPayIdOverride!.Contains(param.FullTextSearch));

                fullTextPd = fullTextPd.Or(p => p.PayerName!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<MPaymentRequest> AddPaymentRequest(MPaymentRequest paymentRequest)
        {
            paymentRequest.OrgId = orgId;
            paymentRequest.CreatedDate = DateTime.UtcNow;

            var amt = (decimal) paymentRequest.GeneratedAmount!;
            var amtStr = amt.ToString("F2");
            paymentRequest.GeneratedAmountStr = amtStr;

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

        public async Task<MPaymentRequest?> UpdatePaymentRequestPaidStatusById(string paymentRequestId, string paymentTxId)
        {
            Guid id = Guid.Parse(paymentRequestId);
            var existing = await context!.PaymentRequests!.AsExpandable().Where(IsOrgMatchPredicate(id)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Status = "Paid";
                existing.PaymentTxId = paymentTxId;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MPaymentRequest?> UpdatePaymentRequestById(string paymentRequestId, MPaymentRequest paymentRequest)
        {
            Guid id = Guid.Parse(paymentRequestId);
            var existing = await context!.PaymentRequests!.AsExpandable().Where(IsOrgMatchPredicate(id)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.PayinBankAccountId = paymentRequest.PayinBankAccountId;
                existing.PayinBankCode = paymentRequest.PayinBankCode;
                existing.PayinBankAccountNo = paymentRequest.PayinBankAccountNo;
                existing.PayinBankAccountName = paymentRequest.PayinBankAccountName;
                existing.PayinPromptPayId = paymentRequest.PayinPromptPayId;
                existing.PayinAccountType = paymentRequest.PayinAccountType;
                existing.PayinAccountLevel = paymentRequest.PayinAccountLevel;
                existing.PayInFeePct = paymentRequest.PayInFeePct;

                existing.PayoutBankAccountId = paymentRequest.PayoutBankAccountId;
                existing.PayoutBankCode = paymentRequest.PayoutBankCode;
                existing.PayoutBankAccountNo = paymentRequest.PayoutBankAccountNo;
                existing.PayoutBankAccountName = paymentRequest.PayoutBankAccountName;
                existing.PayoutPromptPayId = paymentRequest.PayoutPromptPayId;
                existing.PayoutAccountType = paymentRequest.PayoutAccountType;
                existing.PayoutAccountLevel = paymentRequest.PayoutAccountLevel;
                existing.PayoutFeePct = paymentRequest.PayoutFeePct;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MPaymentRequest?> UpdatePayOutRequestById(string paymentRequestId, MPaymentRequest paymentRequest)
        {
            //ให้ update เฉพาะ field ที่เกี่ยวกับการจ่ายเงินออกไปเท่านั้น เพื่อให้แน่ใจว่า field อื่น ๆ จะไม่ถูกแก้ไขโดยไม่ได้ตั้งใจ
            Guid id = Guid.Parse(paymentRequestId);

            var existing = await context!.PaymentRequests!.AsExpandable().Where(IsOrgMatchPredicate(id)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.PayoutBankAccountId = paymentRequest.PayoutBankAccountId;
                existing.PayoutBankCode = paymentRequest.PayoutBankCode;
                existing.PayoutBankAccountNo = paymentRequest.PayoutBankAccountNo;
                existing.PayoutBankAccountName = paymentRequest.PayoutBankAccountName;
                existing.PayoutPromptPayId = paymentRequest.PayoutPromptPayId;
                existing.PayoutAccountType = paymentRequest.PayoutAccountType;
                existing.PayoutAccountLevel = paymentRequest.PayoutAccountLevel;
                existing.PayoutFeePct = paymentRequest.PayoutFeePct;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MPaymentRequest?> UpdatePayOutPeer2PeerHistoryById(string paymentRequestId, MPaymentRequest paymentRequest)
        {
            var oldOrgId = orgId;
            orgId = "global";

            //ให้ update เฉพาะ field ที่เกี่ยวกับการตัดจ่ายด้วย P2P เท่านั้น
            Guid id = Guid.Parse(paymentRequestId);

            var existing = await context!.PaymentRequests!.AsExpandable().Where(IsOrgMatchPredicate(id)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.PartialPayoutHistory = paymentRequest.PartialPayoutHistory;
                existing.TotalPayOutPaidAmountDecimal = paymentRequest.TotalPayOutPaidAmountDecimal;
                existing.TotalPayOutPendingPaidAmountDecimal = paymentRequest.TotalPayOutPendingPaidAmountDecimal;
                existing.PayOutTotalAmountDecimalP2P = paymentRequest.PayOutTotalAmountDecimalP2P;
                existing.PayoutPartialCountP2P = paymentRequest.PayoutPartialCountP2P;
            }

            orgId = oldOrgId;

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MPaymentRequest?> UpdateTransferRequestById(string paymentRequestId, MPaymentRequest paymentRequest)
        {
            //ให้ update เฉพาะ field ที่เกี่ยวกับการจ่ายเงินออกไปเท่านั้น เพื่อให้แน่ใจว่า field อื่น ๆ จะไม่ถูกแก้ไขโดยไม่ได้ตั้งใจ
            Guid id = Guid.Parse(paymentRequestId);

            var existing = await context!.PaymentRequests!.AsExpandable().Where(IsOrgMatchPredicate(id)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.PayoutBankAccountId = paymentRequest.PayoutBankAccountId;
                existing.PayoutBankCode = paymentRequest.PayoutBankCode;
                existing.PayoutBankAccountNo = paymentRequest.PayoutBankAccountNo;
                existing.PayoutBankAccountName = paymentRequest.PayoutBankAccountName;
                existing.PayoutPromptPayId = paymentRequest.PayoutPromptPayId;
                existing.PayoutAccountType = paymentRequest.PayoutAccountType;
                existing.PayoutAccountLevel = paymentRequest.PayoutAccountLevel;
                existing.PayoutFeePct = paymentRequest.PayoutFeePct;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MPaymentRequest?> UpdatePaymentStatusRejectById(string paymentRequestId, MPaymentRequest paymentRequest)
        {
            Guid id = Guid.Parse(paymentRequestId);
            var existing = await context!.PaymentRequests!.AsExpandable().Where(IsOrgMatchPredicate(id)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Status = "Rejected";
                existing.RejectReason = paymentRequest.RejectReason;
                existing.StatusReason = paymentRequest.StatusReason;
                existing.StatusCode = paymentRequest.StatusCode;
                existing.JobId = paymentRequest.JobId;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MPaymentRequest?> UpdatePaymentStatusApprovedById(string paymentRequestId, MPaymentRequest paymentRequest)
        {
            Guid id = Guid.Parse(paymentRequestId);
            var existing = await context!.PaymentRequests!.AsExpandable().Where(IsOrgMatchPredicate(id)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Status = "Approved";
                existing.StatusReason = paymentRequest.StatusReason;
                existing.StatusCode = paymentRequest.StatusCode;
                existing.PayoutBankAccountId = paymentRequest.PayoutBankAccountId;
                existing.PayoutBankCode = paymentRequest.PayoutBankCode;
                existing.PayoutBankAccountNo = paymentRequest.PayoutBankAccountNo;
                existing.PayoutBankAccountName = paymentRequest.PayoutBankAccountName;
                existing.PayoutPromptPayId = paymentRequest.PayoutPromptPayId;
                existing.PayoutAccountType = paymentRequest.PayoutAccountType;
                existing.PayoutAccountLevel = paymentRequest.PayoutAccountLevel;
                existing.PayoutFeePct = paymentRequest.PayoutFeePct;
                existing.PaymentTxId = paymentRequest.PaymentTxId;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeletePayOutRequestById(string paymentRequestId)
        {
            Guid id = Guid.Parse(paymentRequestId);
            var existing = await context!.PaymentRequests!.AsExpandable().Where(IsOrgMatchPredicate(id)).FirstOrDefaultAsync();
            if (existing == null) return false;
            context.PaymentRequests!.Remove(existing);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<MPaymentRequest?> RejectPaymentRequestById(string paymentRequestId, MPaymentRequest paymentRequest)
        {
            Guid id = Guid.Parse(paymentRequestId);
            var existing = await context!.PaymentRequests!.AsExpandable().Where(IsOrgMatchPredicate(id)).FirstOrDefaultAsync();
            if (existing != null)
            {
                //Update แต่ฟีลด์ที่จำเป็นเท่านั้น
                existing.Status = "Rejected";
                existing.StatusReason = paymentRequest.StatusReason;
                existing.RejectReason = paymentRequest.RejectReason;
                existing.StatusCode = paymentRequest.StatusCode;
                existing.JobId = paymentRequest.JobId;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MPaymentRequest?> ApprovePaymentRequestById(string paymentRequestId, MPaymentRequest? payload = null)
        {
            Guid id = Guid.Parse(paymentRequestId);
            var existing = await context!.PaymentRequests!.AsExpandable().Where(IsOrgMatchPredicate(id)).FirstOrDefaultAsync();
            if (existing != null)
            {
                //Update แต่ฟีลด์ที่จำเป็นเท่านั้น
                existing.Status = "Approved";
                if (payload != null)
                {
                    existing.StatusReason = payload.StatusReason;
                    existing.StatusCode = payload.StatusCode;
                }
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MPaymentRequest?> UpdateQrCodeByIdForP2P(string paymentRequestId, MPaymentRequest payOut)
        {
            Guid id = Guid.Parse(paymentRequestId);
            var existing = await context!.PaymentRequests!.AsExpandable().Where(IsOrgMatchPredicate(id)).FirstOrDefaultAsync();
            if (existing != null)
            {
                //Update แต่ฟีลด์ที่จำเป็นเท่านั้น
                existing.QrCodeP2P = payOut.QrCodeP2P;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MPaymentRequest?> ProcessPartialPayoutHistory(MPaymentRequest payOut, MPaymentRequest payIn, string action)
        {
            //เอามาไว้ตรงนี้เพราะมีการเรียกใช้ร่วมกันใน Payment Request service และ Payment Transaction service
            var payoutRequestId = payOut.Id.ToString()!;

            //ให้มีการ lock ในระดับ payoutRequestId ด้วยเพื่อกัน race condition
            using var redPmrLock = await _redis.AcquireRedLockAsync(
                $"lock:ProcessPartialPayoutHistory:{payoutRequestId}",  // resource
                TimeSpan.FromSeconds(5)   // lock expiry
            );

            if (!redPmrLock.IsAcquired)
            {
                Console.WriteLine($"Unable to acquire lock 'lock:ProcessPartialPayoutHistory:{payoutRequestId}'");
                return null;
            }

            var txHistory = payOut.PartialPayoutHistory;
            if (string.IsNullOrEmpty(txHistory))
            {
                txHistory = "[]";
            }

            var txs = JsonSerializer.Deserialize<List<MPartialPayout>>(txHistory);
            txs ??= [];

            var amt = (decimal?) payIn.RequestedAmount;
            amt ??= 0;

            if (action == "Add")
            {
                var ppo = new MPartialPayout()
                {
                    PayinRequestId = payIn.Id.ToString(),
                    PartialAmount = amt,
                    Status = "Pending",
                    TxDate = payIn.CreatedDate,
                    ExpireDate = payIn.ExpireDate,
                };

                txs.Add(ppo);
            }
            else if (action == "Cancel")
            {
                var payinRequestId = payIn.Id.ToString();
                var partialPayout = txs.FirstOrDefault(x => x.PayinRequestId == payinRequestId);
                if (partialPayout != null)
                {
                    partialPayout.Status = "Canceled";
                }
            }
            else if (action == "Approve")
            {
                var payinRequestId = payIn.Id.ToString();
                var partialPayout = txs.FirstOrDefault(x => x.PayinRequestId == payinRequestId);
                if (partialPayout != null)
                {
                    partialPayout.Status = "Approved";
                }
            }
//txs.ForEach(s =>
//{
//    Console.WriteLine($"DEBUG5 - [{action}] [{s.PayinRequestId}] [{s.Status}] [{s.PartialAmount}]");
//});
            payOut.PartialPayoutHistory = JsonSerializer.Serialize(txs);
            payOut.TotalPayOutPendingPaidAmountDecimal = txs.Where(x => x.Status == "Pending").Sum(x => x.PartialAmount);
            payOut.TotalPayOutPaidAmountDecimal = txs.Where(x => x.Status == "Approved").Sum(x => x.PartialAmount);
            payOut.PayOutTotalAmountDecimalP2P = payOut.PayOutTotalAmountDecimal - payOut.TotalPayOutPaidAmountDecimal;
            payOut.PayoutPartialCountP2P = txs.Count(/* x => x.Status == "Approved" */); //เอาทุก status ไม่ใช่เฉพาะ Approved

//Console.WriteLine($"DEBUG_X - [{payOut.PayOutTotalAmountDecimalP2P}] [{payOut.PayOutTotalAmountDecimal}] [{payOut.TotalPayOutPaidAmountDecimal}]");
            var result = await UpdatePayOutPeer2PeerHistoryById(payoutRequestId, payOut);
            return result;
        }

        public async Task<MPaymentRequest?> UpdatePayInSlipById(string paymentRequestId, string slipsJson, int uploadCount)
        {
            Guid id = Guid.Parse(paymentRequestId);
            var existing = await context!.PaymentRequests!.AsExpandable().Where(x => x!.Id == id).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.PayInSlipUploads = slipsJson;
                existing.PayInSlipUploadCount = uploadCount;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MPaymentRequest?> UpdatePayOutSlipById(string paymentRequestId, string slipsJson, int uploadCount)
        {
            Guid id = Guid.Parse(paymentRequestId);
            var existing = await context!.PaymentRequests!.AsExpandable().Where(x => x!.Id == id).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.PayOutSlipUploads = slipsJson;
                existing.PayOutSlipUploadCount = uploadCount;
            }

            await context.SaveChangesAsync();
            return existing;
        }
    }
}