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
        private readonly IMerchantService? _merchantService = null;
        private readonly IRedisHelper _redis;
        private readonly IHubContext<PaymentHub> _hub;

        public PaymentTransactionService(
            IPaymentTransactionRepository repo, 
            IPaymentRequestRepository paymentRequestRepo,
            IPointService pointService,
            IBankAccountRepository bankAccountRepo,
            IJobService jobService,
            IMerchantService merchantService,
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
            _merchantService = merchantService;
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

        private async Task UpdateDailyTxBalance(MPaymentTransaction pt, MBankAccount? bankAccount)
        {
            var orgId = "global"; //ให้ตรงกับ GetMerchantCurrentDailyTxBalance() ใน PaymentRequestService.cs
            var merchantId = pt.MerchantId;
            var bankAccountId = pt.PayInBankAccountId;
            var txAmount = pt.TxAmountDecimal ?? 0;
            double currentDailyTxAmount = 0;

            var merchantCacheKey = CacheHelper.CreateMerchantDailyTxKey(orgId, merchantId!);
            var bankAccountCacheKey = CacheHelper.CreatePayInBankAccountDailyTxKey(orgId, bankAccountId!);

            if (merchantId != null)
            {
                //สามารถ match merchant ได้ ให้เก็บ Daily Tx Balance ของ merchant ไว้ด้วย
                var currentMerchantDailyTxBalance = await _redis.GetObjectAsync<MTxBalance>(merchantCacheKey);
                if (currentMerchantDailyTxBalance == null)
                {
                    currentMerchantDailyTxBalance = new MTxBalance();
                }

                currentMerchantDailyTxBalance.TxCount += 1;
                currentMerchantDailyTxBalance.TxAmount += txAmount;

                await _redis.SetObjectAsync(merchantCacheKey, currentMerchantDailyTxBalance, TimeSpan.FromDays(1.1));
            }

            if (bankAccountId != null)
            {
                //สามารถ match bank account ได้ ให้เก็บ Daily Tx Balance ของ bank account ไว้ด้วย
                var currentBankAccountDailyTxBalance = await _redis.GetObjectAsync<MTxBalance>(bankAccountCacheKey);
                if (currentBankAccountDailyTxBalance == null)
                {
                    currentBankAccountDailyTxBalance = new MTxBalance();
                }

                currentBankAccountDailyTxBalance.TxCount += 1;
                currentBankAccountDailyTxBalance.TxAmount += txAmount;

                currentDailyTxAmount = (double) currentBankAccountDailyTxBalance.TxAmount;

                await _redis.SetObjectAsync(bankAccountCacheKey, currentBankAccountDailyTxBalance, TimeSpan.FromDays(1.1));
            }

            if (bankAccount != null)
            {
                //Check logic ว่ายอด daily balance เกิน limit, ถ้าเกินให้สร้าง job แจ้งเตือนให้ admin ของ merchant ทราบ
                var threshold = 0.95;
                double dailyTxAmountLimit = bankAccount.DailyQuota ?? 0;
                var thresholdAmount = (double) dailyTxAmountLimit * threshold;

                if (dailyTxAmountLimit > 0)
                {
                    if (currentDailyTxAmount > thresholdAmount)
                    {
                        var jobType = "Payment.DailyTxAmountLimitExceeded";
                        await CreatePaymentExceededLimitJob(orgId, jobType, bankAccount, pt, currentDailyTxAmount, threshold); //ให้มี threshold นิดหน่อยเพื่อป้องกันการสร้าง job ซ้ำ ๆ
                    }
                }
            }
        }

        public async Task<MVPaymentTransaction> ApproveUnidentifiedPaymentTx(string orgId, string paymentTransactionId, string merchantId)
        {
            repository!.SetCustomOrgId(orgId);
            var r = new MVPaymentTransaction()
            {
                Status = "OK",
                Description = "Success"
            };

            var paymentTx = await GetPaymentTransactionById(orgId, paymentTransactionId);
            if (paymentTx.Status != "OK")
            {
                return paymentTx;
            }

            var pmt = paymentTx.PaymentTransaction!;
            if (pmt.Status != "UnIdentified")
            {
                r.Status = "ERROR_PAYMENT_TX_NOT_UNIDENTIFIED";
                r.Description = $"Payment Tx ID [{paymentTransactionId}] is not UnIdentified, current status=[{pmt.Status}]";

                return r;
            }

            var merchant = await _merchantService!.GetMerchantById(orgId, merchantId);
            if (merchant.Status != "OK")
            {
                r.Status = merchant.Status;
                r.Description = merchant.Description;
                return r;
            }

            //TODO : Check ว่า merchant ที่เลือกนั้นสามารถ match กับ bank account ของ payment tx ได้หรือไม่ ถ้าไม่ match ให้ return error กลับไป
            //TODO : ให้ดูด้วยว่า merchant นั้นใช้ global bank account ได้ด้วยหรือไม่ ถ้าไม่ได้ก็ return error กลับไป
            // ดู GetMerchantsForBankAccount() จาก BankAccountService.cs

            var mc = merchant.Merchant!;
            var merchantOrgId = mc.OrgId!;

            pmt.OrgId = merchantOrgId; //เปลี่ยนเป็นของ merchant ที่เลือก จากของเดิมที่เป็น global เพราะ เป็น Unidentified Payment Tx
            pmt.Status = "Approved";
            pmt.MerchantId = mc.Id.ToString();
            pmt.Currency = "THB"; //ให้เป็น THB ไปก่อนเพราะว่า merchant มี wallet เดียว
            pmt.PayInFeePct = mc.PayinFeePct;
            pmt.PayInFee = (double) Math.Round((decimal) (pmt.TxAmount! * mc.PayinFeePct! / 100.0), 2, MidpointRounding.AwayFromZero);
            pmt.PayInTotalAmount = pmt.TxAmount - pmt.PayInFee;

            if (mc.DiscardCent)
            {
                //ที่ merchant มีการ config ไว้ว่าให้หักเศษสตางค์
                //เอาเศษสตางค์มาเป็น commission ด้วย

                var amount = (decimal) pmt.PayInTotalAmount!;
                var decimalPart = amount - Math.Truncate(amount);

                pmt.PayInFee += (double) decimalPart; //เอาเศษสตางค์มาเป็นค่าธรรมเนียม
                pmt.PayInTotalAmount = pmt.TxAmount - pmt.PayInFee; //คำนวณยอด Total ใหม่, ยอดออกมาควรเป็นจำนวนเต็ม
                pmt.DiscardCent = true;
            }

            pmt.PayInFeeDecimal = (decimal) pmt.PayInFee!;
            pmt.PayInTotalAmountDecimal = pmt.TxAmountDecimal - pmt.PayInFeeDecimal;
            pmt.PaymentRequestId = null; //เป็น Unidentified Payment Tx ที่ถูก Approve ให้เป็นของ merchant โดยตรง ไม่ได้ match กับ Payment Request ใด ๆ


            //UPdate cache - daily tx balance ของ merchant แต่ของ bank account ได้อัพเดตไปก่อนหน้าแล้วตอนที่สร้าง Payment Tx
            //เก็บ Daily Tx Balance ของ merchant และ bank account ไว้ด้วย เพื่อเอาไว้ใช้ตรวจสอบ limit ต่อวัน
            var oldBankAccountId = pmt.PayInBankAccountId;
            pmt.PayInBankAccountId = null; //ไม่ต้องอัพเดต daily tx balance ของ bank account อีกครั้งเพราะว่าอัพเดตไปแล้วตอนที่สร้าง Payment
            await UpdateDailyTxBalance(pmt, null);
            pmt.PayInBankAccountId = oldBankAccountId;


            //==== Update Payment Tx
            var mpt = await repository!.ApprovePaymentTransactionById(paymentTransactionId, pmt);
            paymentTx.PaymentTransaction = mpt;


            //=== Update merchant & bank account wallet
            var mcWallet = await _pointService!.GetWalletByMerchantId(merchantOrgId, merchantId);
            if (mcWallet!.Status == "OK")
            {
                var wallet = mcWallet.Wallet;
                var pointTx = new MPointTx()
                {
                    WalletId = wallet!.Id.ToString(),

                    TxAmount =  (long)Math.Floor((decimal) pmt.PayInTotalAmount!), //เอาส่วนจำนวนเต็มมาเท่านั้น
                    //TxAmountDecimal ตรงนี้จะเป็นค่าที่หัก commission แล้ว เพื่อนำไปเป็นยอดที่เข้า wallet
                    TxAmountDecimal = pmt.PayInTotalAmountDecimal,

                    Tags = $"PaymentTxId=[{pmt.Id.ToString()}]",
                };

                await _pointService!.AddPoint(merchantOrgId, pointTx);
            }

            var baWallet = await _pointService!.GetWalletByBankAccountId(orgId, pmt.PayInBankAccountId!);
            if (baWallet!.Status == "OK")
            {
                var wallet = baWallet.Wallet;
                var pointTx = new MPointTx()
                {
                    WalletId = wallet!.Id.ToString(),

                    TxAmount = 1, //นับจำนวนครั้งของ transaction เผื่อเอาไว้ใช้ check limit จำนวนครั้งต่อวัน
                    //TxAmountDecimal ตรงนี้จะเป็นค่าที่ยังไม่หัก fee เพื่อนำไปเป็นยอดที่เข้า wallet
                    TxAmountDecimal = pmt.TxAmountDecimal,

                    Tags = $"PaymentTxId=[{pmt.Id.ToString()}]",
                };

                await _pointService!.AddPoint(orgId, pointTx);
            }

            //=== Trigger job
            var pmr = new MPaymentRequest()
            {
                Id = Guid.NewGuid(),
                RefId1 = pmt.Id.ToString(),
                RefId2 = "",
                RefId3 = "",

                MerchantId = mc.Id.ToString(),
                MerchantCode = mc.Code,
                MerchantName = mc.Name,

                RequestedAmount = pmt.TxAmount,
                GeneratedAmount = pmt.TxAmount,
            };

            var jobType = "Payment.Success";
            var job = CreatePaymentSuccessJob(mc.OrgId!, jobType, pmt, pmr!);
            pmt.JobId = job?.Id.ToString();

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var stream = $"JobSubmitted:{environment}:{jobType}";
            var message = JsonSerializer.Serialize(job);
            _ = await _redis.PublishMessageAsync(stream!, message);

            return paymentTx;
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
            var bankAccount = await _bankAccountRepo.GetBankAccountById(bankAccountId);

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
                if (bankAccount == null)
                {
                    lines.Add($"STEP7 : Info -> Unable to find bank account, BankAccountId=[{bankAccountId}], GeneratedAmount=[{prParam.GeneratedAmountStr}]");                    
                }
                else
                {
                    lines.Add($"STEP8 : Info -> Only able to identify bank account, BankAccountId=[{bankAccountId}], GeneratedAmount=[{prParam.GeneratedAmountStr}]");                    

                    pt.PayInBankAccountId = bankAccountId;
                    pt.PayInBankCode = bankAccount.BankCode;
                    pt.PayInBankAccountNo = bankAccount.AccountNumber;
                    pt.PayInBankAccountName = bankAccount.AccountName;
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

            //เก็บ Daily Tx Balance ของ merchant และ bank account ไว้ด้วย เพื่อเอาไว้ใช้ตรวจสอบ limit ต่อวัน
            await UpdateDailyTxBalance(pt, bankAccount);


            //สร้าง job ตรงนี้ พร้อมส่ง jobId ให้กับ Payment Tx เผื่อเอาไว้ใช้ดู log การ process ในแต่ละ step ได้ง่ายขึ้น
            var jobType = "Payment.Unidentified";
            if (pt.Status == "Identified")
            {
                jobType = "Payment.Success";
            }
            var job = CreatePaymentSuccessJob(merchantOrgId, jobType, pt, pmr!);
            pt.JobId = job?.Id.ToString();


            //=== Create Payment Tx
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
                    new NameValue { Name = "PMT_ID", Value = pmt?.Id.ToString() },
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

        private async Task<MJob> CreatePaymentExceededLimitJob(
            string orgId, string jobType, 
            MBankAccount bankAccount, 
            MPaymentTransaction pmt,
            double currentDailyTxAmount,
            double threshold)
        {
            double dailyTxAmountLimit = bankAccount.DailyQuota ?? 0;
            var thresholdAmount = (double) dailyTxAmountLimit * threshold;

            var job = new MJob()
            {
                Name = $"{Guid.NewGuid()}",
                Description = "PaymentTransaction.CreatePaymentExceededLimitJob()",
                Type = jobType,
                Status = "Pending",
                Tags = jobType,

                Parameters =
                [
                    new NameValue { Name = "ORG_ID", Value = orgId },
                    new NameValue { Name = "PMT_ID", Value = pmt?.Id.ToString() },
                    new NameValue { Name = "CURRENT_DAILY_TX_AMOUNT", Value = currentDailyTxAmount.ToString() },
                    new NameValue { Name = "THRESHOLD_RATIO", Value = threshold.ToString() },
                    new NameValue { Name = "THRESHOLD_AMOUNT", Value = thresholdAmount.ToString() },

                    new NameValue { Name = "BANK_ACCOUNT_ID", Value = bankAccount?.Id.ToString() },
                    new NameValue { Name = "BANK_ACCOUNT_NAME", Value = bankAccount?.AccountName },
                    new NameValue { Name = "BANK_ACCOUNT_NO", Value = bankAccount?.AccountNumber },
                    new NameValue { Name = "BANK_ACCOUNT_DAILY_QUOTA", Value = bankAccount?.DailyQuota.ToString() },
                    new NameValue { Name = "BANK_CODE", Value = bankAccount?.BankCode },
                ]
            };

            //trigger job ให้ทำงานทันที
            var result = _jobService!.AddJob(orgId, job, false); 
            var newJob = result?.Job!;

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var stream = $"JobSubmitted:{environment}:{jobType}";
            var message = JsonSerializer.Serialize(job);
            _ = await _redis.PublishMessageAsync(stream!, message);

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
