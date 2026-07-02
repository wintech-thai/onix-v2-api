using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;

namespace Its.Onix.Api.Services
{
    public class PaymentTransactionService : BaseService, IPaymentTransactionService
    {
        private readonly IPaymentTransactionRepository? repository = null;
        private readonly IPaymentRequestRepository? _paymentRequestRepo = null;
        private readonly IBankAccountRepository? _bankAccountRepo = null;
        private readonly IPointService? _pointService = null;
        private readonly IJobService? _jobService = null;
        private readonly IRedisHelper _redis;
        private readonly IHubContext<PaymentHub> _hub;

        public PaymentTransactionService(
            IPaymentTransactionRepository repo, 
            IPaymentRequestRepository paymentRequestRepo,
            IPointService pointService,
            IBankAccountRepository bankAccountRepo,
            IJobService jobService,
            IHubContext<PaymentHub> hub,
            IRedisHelper redis) : base()
        {
            repository = repo;
            _paymentRequestRepo = paymentRequestRepo;
            _bankAccountRepo = bankAccountRepo;
            _pointService = pointService;
            _jobService = jobService;
            _redis = redis;
            _hub = hub;
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

        public async Task<List<MPaymentRequest>> GetPaymentRequestsForPaymentTx(string orgId, VMPaymentRequest param)
        {
            _paymentRequestRepo!.SetCustomOrgId(orgId);
            var result = await _paymentRequestRepo.GetPaymentRequestsForPaymentTx(param);

            return result;
        }

        public async Task<MVPaymentTransaction> ProcessLinePaymentTxNotification(
            string orgId, 
            string bankAccountId, 
            MPaymentNotiLine paymentNotiLine)
        {
            //ณ จุดนี้เรายังไม่รู้ว่า transaction เป็นของ merchant ไหน
            _paymentRequestRepo!.SetCustomOrgId("global");
            _bankAccountRepo!.SetCustomOrgId("global");
//Console.WriteLine($"DEBUG1 - [{paymentNotiLine.MerchantId}], [{bankAccountId}]");

            decimal amt = paymentNotiLine.PaymentAmount ?? 0;
            var amtStr = amt.ToString("F2");

            //Acquire payment request lock here to prevent race condition
            //บางครั้ง มี payment request แล้วมีการ hack โดยยิง line noti เข้ามาซ้ำ ๆ ๆ ทำให้ topup เงินเข้าไปเกิน
            using var redPmrLock = await _redis.AcquireRedLockAsync(
                $"lock:ProcessLinePaymentTxNotification:{bankAccountId}:{amtStr}",  // resource
                TimeSpan.FromSeconds(2)   // lock expiry
            );

            if (!redPmrLock.IsAcquired)
            {
                var r = new MVPaymentTransaction()
                {
                    Status = "ERROR_ACQUIRED_RECORD",
                    Description = $"Unable to acquire record for bank account ID [{bankAccountId}], amount=[{amtStr}]",
                };

                return r;
            }

            var prParam = new VMPaymentRequest()
            {
                //ไม่ต้องระบุ merchantId เพราะว่าเรายังไม่รู้ว่า transaction นี้เป็นของ merchant ไหน
                MerchantId = paymentNotiLine.MerchantId, //ตรงนี้อาจจะมี merchantId มาด้วยจาก Line ก็ได้ เผื่อเอาไว้ใช้ match ได้ง่ายขึ้นหน่อย
                BankAccountId = bankAccountId,
                Status = "Pending",
                RefId1 = paymentNotiLine.RefId1,
                
                GeneratedAmountStr = amtStr, //เอาเลขเศษสตางค์ไป match ด้วย
                FromDate = DateTime.UtcNow.AddHours(-1),
            };

            paymentNotiLine.PaymentRequestQuery = prParam;

            var paymentRequests = await GetPaymentRequestsForPaymentTx("global", prParam);
            var matchCount = paymentRequests.Count;

            MPaymentRequest? pmr = null;
            List<string> lines = [];

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
                    var pmrId = pr.Id.ToString()!;
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
                Currency = "THB",
                TxAmount = (double) paymentNotiLine.PaymentAmount!,
                TxAmountDecimal = paymentNotiLine.PaymentAmount,
                FromBankAccountNo = paymentNotiLine.SourceBankAccountNo,
                FromBankCode = paymentNotiLine.SourceBankCode,
            };

            repository!.SetCustomOrgId("global"); //ให้เป็นของ global ไปก่อนถ้า match payment request ไม่ได้
            if (pmr != null)
            {
                repository!.SetCustomOrgId(pmr.OrgId!); //ตรงนี้เป็น orgId ของ Merchant

                pt.Status = "Identified";
                pt.Currency = pmr.Currency;
                pt.PayInFeePct = pmr.PayInFeePct;
                pt.PayInFee = (double) Math.Round((decimal) (pt.TxAmount * pmr.PayInFeePct! / 100.0), 2, MidpointRounding.AwayFromZero);
                pt.PayInTotalAmount = pt.TxAmount - pt.PayInFee;

                if (pmr.DiscardCent)
                {
                    //ที่ merchant มีการ config ไว้ว่าให้หักเศษสตางค์
                    //เอาเศษสตางค์มาเป็น commission ด้วย

                    var amount = (decimal) pt.PayInTotalAmount;
                    var decimalPart = amount - Math.Truncate(amount);

                    pt.PayInFee += (double) decimalPart; //เอาเศษสตางค์มาเป็นค่าธรรมเนียม
                    pt.PayInTotalAmount = pt.TxAmount - pt.PayInFee; //คำนวณยอด Total ใหม่, ยอดออกมาควรเป็นจำนวนเต็ม
                    pt.DiscardCent = true;
                }

                pt.PayInFeeDecimal = (decimal) pt.PayInFee!;
                pt.PayInTotalAmountDecimal = pt.TxAmountDecimal - pt.PayInFeeDecimal;

                pt.PayInBankAccountId = pmr.PayinBankAccountId;
                pt.PayInBankCode = pmr.PayinBankCode;
                pt.PayInBankAccountNo = pmr.PayinBankAccountNo;
                pt.PayInBankAccountName = pmr.PayinBankAccountName;
                pt.PaymentRequestId = pmr.Id.ToString();

                pt.MerchantId = pmr.MerchantId;

                lines.Add($"STEP5 : Info -> Found TxAmount=[{pt.TxAmountDecimal}], PayInFeePct=[{pmr.PayInFeePct}], PayInFee=[{pt.PayInFeeDecimal}], PayInTotal=[{pt.PayInTotalAmountDecimal}]");
            }
            else
            {
                lines.Add($"STEP6 : Info -> No payment request found [{matchCount}], BankAccountId=[{bankAccountId}], GeneratedAmount=[{prParam.GeneratedAmountStr}]");
                var ba = await _bankAccountRepo.GetBankAccountById(bankAccountId);
                if (ba == null)
                {
                    lines.Add($"STEP7 : Info -> Unable to found bank account, BankAccountId=[{bankAccountId}], GeneratedAmount=[{prParam.GeneratedAmountStr}]");                    
                }
                else
                {
                    lines.Add($"STEP8 : Info -> Only able to identify bank account, BankAccountId=[{bankAccountId}], GeneratedAmount=[{prParam.GeneratedAmountStr}]");                    

                    pt.PayInBankAccountId = bankAccountId;
                    pt.PayInBankCode = ba.BankCode;
                    pt.PayInBankAccountNo = ba.AccountNumber;
                    pt.PayInBankAccountName = ba.AccountName;
                }
            }

            var merchantOrgId = "";
            var bankAccountOrgId = "global";

            MVWallet? mcWallet = null;
            MVWallet? baWallet = null;

            if (pmr != null)
            {
                //ตรงนี้มีการ match merchant ได้
                var merchantId = pmr.MerchantId!;
                merchantOrgId = pmr.OrgId!;

                //TODO : ตอนนี้มีแค่ wallet เดียวแต่ merchant, อนาคตถ้าต้องมี wallet ตาม currency ก็ต้องเปลี่ยนตรงนี้หน่อยนะ
                mcWallet = await _pointService!.GetWalletByMerchantId(merchantOrgId, merchantId);
                if (mcWallet!.Status != "OK")
                {
                    //TODO : ให้มี alert flag ใน Payment Tx หน่อย
                    lines.Add($"STEP9 : Error -> No wallet found, MerchantId=[{merchantId}], MerchantOrgId=[{merchantOrgId}]");
                }

                baWallet = await _pointService!.GetWalletByBankAccountId(bankAccountOrgId, bankAccountId);
                if (baWallet!.Status != "OK")
                {
                    //TODO : ให้มี alert flag ใน Payment Tx หน่อย
                    lines.Add($"STEP10 : Error -> No wallet found, BankAccountId=[{bankAccountId}], BankAccountOrgId=[{bankAccountOrgId}]");
                }
            }

            pt.RawInput = JsonSerializer.Serialize(paymentNotiLine); //"{}";
            pt.ProcessingMessages = JsonSerializer.Serialize(lines);

            //สร้าง job ตรงนี้ พร้อมส่ง jobId ให้กับ Payment Tx เผื่อเอาไว้ใช้ดู log การ process ในแต่ละ step ได้ง่ายขึ้น
            var jobType = "Payment.Failed";
            if (pt.Status == "Identified")
            {
                jobType = "Payment.Success";
            }
            var job = CreatePaymentSuccessJob(merchantOrgId, jobType, pt, pmr!);
            pt.JobId = job?.Id.ToString();


            var mpt = await repository!.AddPaymentTransaction(pt);
            var mvPt = new MVPaymentTransaction()
            {
                Status = "OK",
                Description = "Success",
                PaymentTransaction = mpt,
            };

            if ((pmr != null) && (mpt != null))
            {
                var pmrId = pmr.Id.ToString()!;

                //ตรงนี้จะเป็น Identified ได้เสมอ
                var paymentTxId = mpt.Id.ToString()!;
                var _ = await _paymentRequestRepo.UpdatePaymentRequestPaidStatusById(pmrId, paymentTxId);

                if (mcWallet != null)
                {
                    var wallet = mcWallet.Wallet;
                    var pointTx = new MPointTx()
                    {
                        WalletId = wallet!.Id.ToString(),

                        TxAmount =  (long)Math.Floor((decimal) pt.PayInTotalAmount!), //เอาส่วนจำนวนเต็มมาเท่านั้น
                        //TxAmountDecimal ตรงนี้จะเป็นค่าที่หัก commission แล้ว เพื่อนำไปเป็นยอดที่เข้า wallet
                        TxAmountDecimal = pt.PayInTotalAmountDecimal,

                        Tags = $"PaymentTxId=[{paymentTxId}]",
                    };

                    await _pointService!.AddPoint(merchantOrgId, pointTx);
                }

                if (baWallet != null)
                {
                    var wallet = baWallet.Wallet;
                    var pointTx = new MPointTx()
                    {
                        WalletId = wallet!.Id.ToString(),

                        TxAmount = 1, //นับจำนวนครั้งของ transaction เผื่อเอาไว้ใช้ check limit จำนวนครั้งต่อวัน
                        //TxAmountDecimal ตรงนี้จะเป็นค่าที่ยังไม่หัก fee เพื่อนำไปเป็นยอดที่เข้า wallet
                        TxAmountDecimal = pt.TxAmountDecimal,

                        Tags = $"PaymentTxId=[{paymentTxId}]",
                    };

                    await _pointService!.AddPoint(bankAccountOrgId, pointTx);
                }
            }

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var stream = $"JobSubmitted:{environment}:{jobType}";
            var message = JsonSerializer.Serialize(job);
            _ = await _redis.PublishMessageAsync(stream!, message);

            //Notify กลับไปที่ฝั่ง browser ผ่าน SignalR
            if (pmr != null)
            {
                var sessionId = pmr!.Id.ToString();
                Console.WriteLine($"DEBUG1 - [Notify] for sessionId=[{sessionId}]");
                await _hub.Clients
                    .Group($"payment:{sessionId}")
                    .SendAsync(
                        "payment.completed",
                        new
                        {
                            sessionId,
                            amount = pmr.GeneratedAmount
                        });

                Console.WriteLine($"DEBUG2 - [Notify] for sessionId=[{sessionId}]");
            }

            return mvPt;
        }

        private MJob CreatePaymentSuccessJob(string orgId, string jobType, MPaymentTransaction pmt, MPaymentRequest pmr)
        {
            var job = new MJob()
            {
                Name = $"{Guid.NewGuid()}",
                Description = "PaymentTransaction.CreatePaymentSuccessJob()",
                Type = jobType,
                Status = "Pending",
                Tags = jobType,

                Parameters =
                [
                    new NameValue { Name = "ORG_ID", Value = orgId },
                    new NameValue { Name = "PMR_ID", Value = pmr?.Id.ToString() },
                    new NameValue { Name = "PMR_REF_ID1", Value = pmr?.RefId1 },
                    new NameValue { Name = "PMR_REF_ID2", Value = pmr?.RefId2 },
                    new NameValue { Name = "PMR_REF_ID3", Value = pmr?.RefId3 },

                    new NameValue { Name = "MERCHANT_ID", Value = pmr?.MerchantId },
                    new NameValue { Name = "MERCHANT_CODE", Value = pmr?.MerchantCode },
                    new NameValue { Name = "MERCHANT_NAME", Value = pmr?.MerchantName },

                    new NameValue { Name = "PAYIN_REQUEST_AMOUNT", Value = pmr?.RequestedAmount.ToString() },
                    new NameValue { Name = "PAYIN_GENERATED_AMOUNT", Value = pmr?.GeneratedAmount.ToString() },
                    new NameValue { Name = "PAYIN_FEE_PCT", Value = pmt?.PayInFeeDecimal.ToString() },
                    new NameValue { Name = "PAYIN_BANK_CODE", Value = pmt?.PayInBankCode },
                    new NameValue { Name = "PAYIN_BANK_ACCOUNT_NO", Value = pmt?.PayInBankAccountNo },
                    new NameValue { Name = "PAYIN_BANK_ACCOUNT_NAME", Value = pmt?.PayInBankAccountName },
                    new NameValue { Name = "PAYIN_FEE_PCT", Value = pmt?.PayInFeePct.ToString() },
                ]
            };

            //ยังไม่ต้อง trigger job ให้ทำงานทันที เดี๋ยวรอให้สร้าง Payment Tx เสร็จแล้วค่อย trigger พร้อมกับส่ง jobId ไปด้วยจะได้ดู log การ process ได้ง่ายขึ้น
            var result = _jobService!.AddJob(orgId, job, false); 
            var newJob = result?.Job!;

            return newJob;
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
