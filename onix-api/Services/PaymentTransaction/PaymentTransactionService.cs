using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;
using System.Text.Json;

namespace Its.Onix.Api.Services
{
    public class PaymentTransactionService : BaseService, IPaymentTransactionService
    {
        private readonly IPaymentTransactionRepository? repository = null;
        private readonly IPaymentRequestRepository? _paymentRequestRepo = null;

        public PaymentTransactionService(IPaymentTransactionRepository repo, IPaymentRequestRepository paymentRequestRepo) : base()
        {
            repository = repo;
            _paymentRequestRepo = paymentRequestRepo;
        }

        public async Task<MVPaymentTransaction> GetPaymentTransactionById(string orgId, string paymentTransactionId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPaymentTransaction()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(paymentTransactionId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Payment Tx ID [{paymentTransactionId}] format is invalid";

                return r;
            }

            var result = await repository!.GetPaymentTransactionById(paymentTransactionId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Tx ID [{paymentTransactionId}] not found for the organization [{orgId}]";

                return r;
            }

            if (orgId != "global")
            {
                //Filter ว่า data ที่ส่งออกไปต้องเป็นของ orgId นั้น ๆ เท่าน้้น
                if (result.OrgId != orgId)
                {
                    r.Status = "ERROR_DATA_NOT_OWN_BY_ORG_ID";
                    r.Description = $"Payment Tx ID [{paymentTransactionId}] own by organization [{orgId}] --> [{result.OrgId}]";

                    return r;
                }
            }

            List<string> lines;
            try
            {
                lines = JsonSerializer.Deserialize<List<string>>(result.ProcessingMessages!) ?? new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR1 - [{ex.Message}]");
                lines = [];
            }

            JsonElement rawInputObj;
            try
            {
                rawInputObj = JsonSerializer.Deserialize<JsonElement>(result.RawInput!);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR2 - [{ex.Message}]");
                rawInputObj = JsonSerializer.Deserialize<JsonElement>("{}");
            }


            result.ProcessingSteps = lines;
            result.RawInputObj = rawInputObj;

            result.ProcessingMessages = "";
            result.RawInput = "";

            r.PaymentTransaction = result;

            return r;
        }

        public async Task<MVPaymentTransaction> ProcessLinePaymentTxNotification(
            string orgId, 
            string bankAccountId, 
            MPaymentNotiLine paymentNotiLine)
        {
            //ณ จุดนี้เรายังไม่รู้ว่า transaction เป็นของ merchant ไหน
            _paymentRequestRepo!.SetCustomOrgId("global");

            var prParam = new VMPaymentRequest()
            {
                BankAccountId = bankAccountId,
                Status = "Pending",
                GeneratedAmountStr = paymentNotiLine.PaymentAmount.ToString(), //เอาเลขเศษสตางค์ไป match ด้วย
                FromDate = DateTime.UtcNow.AddHours(-1),
            };

            MPaymentRequest? pmr = null;
            List<string> lines = [];

            var paymentRequests = await _paymentRequestRepo.GetPaymentRequestsForPaymentTx(prParam);
            var matchCount = paymentRequests.Count;

            lines.Add($"STEP1 : Info -> Found [{matchCount}] payment request matche, BankAccountId=[{bankAccountId}], GeneratedAmount=[{prParam.GeneratedAmountStr}]");
            foreach (var pr in paymentRequests)
            {
                if (pr.Status == "Paid")
                {
                    //อาจจะเจอตัวที่จำนวนเงินเท่ากันแล้วชำระไปแล้ว
                    //ไม่ควรเจอ case นี้เพราะว่าเราเลือกแต่ Pending มาเท่านั้น
                    lines.Add($"STEP2 : Warning -> Found Satus=[{pr.Status}], PaymentRequestId=[{pr.Id.ToString()}], Amount=[{pr.GeneratedAmount}]");
                    continue;
                }

                if (pr.PayinBankAccountId != bankAccountId)
                {
                    //ไม่ควรเจอ case นี้เพราะว่าเรา select เฉพาะ bankAccountId ออกมาแล้ว
                    lines.Add($"STEP3 : Warning -> Different BankAccountId BankAccountId=[{pr.PayinBankAccountId}], PaymentRequestId=[{pr.Id.ToString()}], Amount=[{pr.GeneratedAmount}]");
                    continue;
                }

                if (pr.Status == "Pending")
                {
                    //หยิบตัวนี้มาใช้เลย
                    lines.Add($"STEP4 : Success -> Found Satus=[{pr.Status}], BankAccountId=[{pr.PayinBankAccountId}], Amount=[{pr.GeneratedAmount}]");

                    pmr = pr;
                    break;
                }
            }

            var pt = new MPaymentTransaction()
            {
                Status = "UnIdentified",
                Direction = "PayIn",
                TxAmount = (double) paymentNotiLine.PaymentAmount!,
                TxAmountDecimal = paymentNotiLine.PaymentAmount,
                FromBankAccountNo = paymentNotiLine.SourceBankAccountNo,
                FromBankCode = paymentNotiLine.SourceBankCode,
            };

            repository!.SetCustomOrgId("global"); //ให้เป็นของ global ไปก่อนถ้า match payment request ไม่ได้
            if (pmr != null)
            {
                lines.Add($"STEP5 : Info -> Found TxAmount=[{pt.TxAmountDecimal}], PayInFeePct=[{pmr.PayInFeePct}], PayInFee=[{pt.PayInFeeDecimal}], PayInTotal=[{pt.PayInTotalAmountDecimal}]");

                repository!.SetCustomOrgId(pmr.OrgId!); //ตรงนี้เป็น orgId ของ Merchant

                pt.Status = "Identified";
                pt.Currency = pmr.Currency;
                pt.PayInFeePct = pmr.PayInFeePct;
                pt.PayInFee = (double) Math.Round((decimal) (pt.TxAmount * pmr.PayInFeePct! / 100.0), 2, MidpointRounding.AwayFromZero);
                pt.PayInTotalAmount = pt.TxAmount - pt.PayInFee;

                pt.PayInFeeDecimal = (decimal) pt.PayInFee!;
                pt.PayInTotalAmountDecimal = pt.TxAmountDecimal - pt.PayInFeeDecimal;

                pt.PayInBankAccountId = pmr.PayinBankAccountId;
                pt.PayInBankCode = pmr.PayinBankCode;
                pt.PayInBankAccountNo = pmr.PayinBankAccountNo;
                pt.PayInBankAccountName = pmr.PayinBankAccountName;
                pt.PaymentRequestId = pmr.Id.ToString();

                pt.MerchantId = pmr.MerchantId;
            }

            pt.RawInput = JsonSerializer.Serialize(paymentNotiLine); //"{}";
            pt.ProcessingMessages = JsonSerializer.Serialize(lines);

            var mpt = await repository!.AddPaymentTransaction(pt);

            if ((pmr != null) && (mpt != null))
            {
                var _ = await _paymentRequestRepo.UpdatePaymentRequestPaidStatusById(pmr.Id.ToString()!, mpt.Id.ToString()!);
            }

            var mvPt = new MVPaymentTransaction()
            {
                Status = "OK",
                Description = "Success",
                PaymentTransaction = mpt,
            };

            return mvPt;
        }


        public async Task<List<MPaymentTransaction>> GetPaymentTransactions(string orgId, VMPaymentTransaction param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPaymentTransactions(param);

            // ลบ ResponseData ออกเพื่อลด payload
            result.ForEach(p => 
            { 
                p.ProcessingMessages = "";
                p.RawInput = "";
            });

            // ถ้าไม่ใช่ global ให้เหลือเฉพาะรายการของ orgId นั้น
            if (orgId != "global")
            {
                //ป้องกันความผิดพลาดไม่ให้ payment request ของ org หนึ่งไปโผล่ในอีก org หนึ่ง
                result.RemoveAll(p => p.OrgId != orgId);
            }

            return result;
        }

        public async Task<int> GetPaymentTransactionCount(string orgId, VMPaymentTransaction param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPaymentTransactionCount(param);

            return result;
        }

        public async Task<MVPaymentTransaction> UpdatePaymentTransactionById(string orgId, string paymentTransactionId, MPaymentTransaction paymentTransaction)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPaymentTransaction()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(paymentTransactionId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Payment Tx ID [{paymentTransactionId}] format is invalid";

                return r;
            }

            var result = await repository!.UpdatePaymentTransactionById(paymentTransactionId, paymentTransaction);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Tx ID [{paymentTransactionId}] not found for the organization [{orgId}]";

                return r;
            }

            r.PaymentTransaction = result;

            return r;
        }
    }
}
