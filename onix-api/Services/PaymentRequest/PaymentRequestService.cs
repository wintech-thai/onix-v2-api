using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;
using System.Text.Json;

namespace Its.Onix.Api.Services
{
    public class PaymentRequestService : BaseService, IPaymentRequestService
    {
        //ธนาคารที่ support การสร้าง QR payment ตอนนี้ - ถ้าจะเพิ่มธนาคารอื่นในอนาคต แค่เพิ่มเข้า array นี้ (ไม่ต้องแก้ logic เดิม)
        private static readonly string[] allowedQrProvider = { "PP", "SCB" };

        private readonly IPaymentRequestRepository? repository = null;
        private readonly IPaymentTransactionRepository? _paymentTransactionRepo = null;
        private readonly IBankAccountRepository? _bankAccountRepo = null;
        private readonly IPointService? _pointService = null;
        private readonly IRedisHelper _redis;

        public PaymentRequestService(
            IPaymentRequestRepository repo, 
            IPaymentTransactionRepository paymentTxRepo, 
            IBankAccountRepository bankAcctRepo,
            IRedisHelper redis,
            IPointService pointService) : base()
        {
            repository = repo;
            _paymentTransactionRepo = paymentTxRepo;
            _bankAccountRepo = bankAcctRepo;
            _pointService = pointService;
            _redis = redis;
        }

        public async Task<MVPaymentRequest> GetPaymentRequestById(string orgId, string paymentRequestId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPaymentRequest()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(paymentRequestId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Payment Request ID [{paymentRequestId}] format is invalid";

                return r;
            }

            var result = await repository!.GetPaymentRequestById(paymentRequestId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Request ID [{paymentRequestId}] not found for the organization [{orgId}]";

                return r;
            }

            if (orgId != "global")
            {
                //Filter ว่า data ที่ส่งออกไปต้องเป็นของ orgId นั้น ๆ เท่าน้้น
                if (result.OrgId != orgId)
                {
                    r.Status = "ERROR_DATA_NOT_OWN_BY_ORG_ID";
                    r.Description = $"Payment Request ID [{paymentRequestId}] own by organization [{orgId}] --> [{result.OrgId}]";

                    return r;
                }
            }

            try
            {
                result.ResponseDataObj = JsonSerializer.Deserialize<MPaymentResponse>(result.ResponseData!);
            }
            catch
            {
                result.ResponseDataObj = null;
            }

            List<string> lines;
            try
            {
                lines = JsonSerializer.Deserialize<List<string>>(result.ProcessingMessages!) ?? new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR - [{ex.Message}]");
                lines = [];
            }
            
            result.ProcessingSteps = lines;

            result.ResponseData = "";
            result.ProcessingMessages = "";

            r.PaymentRequest = result;

            return r;
        }

        //เช็คสถานะ payment กับ SCB ตรง ๆ แทนรอ webhook
        public async Task<MVScbInquiryResult> InquireScbPaymentStatus(string orgId, string paymentRequestId)
        {
            repository!.SetCustomOrgId(orgId);
            _bankAccountRepo!.SetCustomOrgId("global");

            var r = new MVScbInquiryResult()
            {
                Status = "OK",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(paymentRequestId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Payment Request ID [{paymentRequestId}] format is invalid";

                return r;
            }

            var pr = await repository!.GetPaymentRequestById(paymentRequestId);
            if (pr == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Request ID [{paymentRequestId}] not found";

                return r;
            }

            if (pr.QrProvider != "SCB")
            {
                r.Status = "BANK_PROVIDER_NOT_SUPPORT";
                r.Description = $"Payment Request [{paymentRequestId}] is not a SCB payment (QrProvider=[{pr.QrProvider}])";

                return r;
            }

            if (string.IsNullOrEmpty(pr.PayinBankAccountId))
            {
                r.Status = "ERROR_NO_PAYIN_ACCOUNT";
                r.Description = $"Payment Request [{paymentRequestId}] has no pay-in bank account assigned";

                return r;
            }

            var bankAccount = await _bankAccountRepo!.GetBankAccountById(pr.PayinBankAccountId);
            if (bankAccount == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Bank account [{pr.PayinBankAccountId}] not found";

                return r;
            }

            var transactionDate = (pr.CreatedDate ?? DateTime.UtcNow).ToString("yyyy-MM-dd");
            var qrGenerator = new QrGeneratorSCB(pr, bankAccount, _redis);
            var inquiryResult = await qrGenerator.InquireAsync(transactionDate);

            r.Status = inquiryResult.Status;
            r.Description = inquiryResult.Description;
            r.RawResponse = inquiryResult.RawResponse;

            return r;
        }

        private double? GetGeneratedAmount(MPaymentRequest paymentRequest, MMerchant merchant)
        {
            var amt = paymentRequest.RequestedAmount;
            if (amt == null)
            {
                return 0;
            }
            
            if (merchant.RandomDecimal == false)
            {
                return amt;
            }

            // merchant.RandomDecimal is true or null
            // เอาเฉพาะเลขหน้าทศนิยม
            var integerPart = Math.Truncate(amt.Value);

            // random ทศนิยม 01-99
            var random = new Random();

            int decimalPart = 0;
            for (int i = 0; i < 3; i++)
            {
                //มันไม่ควรเกิน 3 ครั้งอยู่แล้วตรงนี้
                decimalPart = random.Next(1, 100);
                if (decimalPart % 10 != 0)
                {
                    break;
                }
            }

            var newAmt = integerPart + (decimalPart / 100.0);

            return Math.Round(newAmt, 2);
        }

        public async Task<MVPaymentRequest> UpdatePaymentRequestPayOut(string orgId, string paymentRequestId, MPaymentRequest paymentRequest, MBankAccount bankAccount, MMerchant merchant)
        {
            repository!.SetCustomOrgId(orgId); //ตรงนี้เป็น orgId ของ Merchant
            _bankAccountRepo!.SetCustomOrgId("global");

            var r = new MVPaymentRequest()
            {
                Status = "OK",
                Description = "Success",
            };

            var existing = await repository!.GetPaymentRequestById(paymentRequestId);
            if (existing == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Request ID [{paymentRequestId}] not found for the organization [{orgId}]";

                return r;
            }

            if (existing.Status != "Pending")
            {
                r.Status = "INVALID_STATUS";
                r.Description = $"Payment Request ID [{paymentRequestId}] has invalid status [{existing.Status}] for update payout";

                return r;
            }

            paymentRequest.PayoutBankAccountName = bankAccount!.AccountName;
            paymentRequest.PayoutBankAccountNo = bankAccount.AccountNumber;
            paymentRequest.PayoutBankCode = bankAccount.BankCode;
            paymentRequest.PayoutPromptPayId = bankAccount.PromptPayId;
            paymentRequest.PayoutAccountType = bankAccount.AccountType;
            paymentRequest.PayoutAccountLevel = bankAccount.AccountLevel;
            paymentRequest.PayoutFeePct = merchant.PayinFeePct;
            paymentRequest.PayoutBankAccountId = bankAccount.Id.ToString();

            var result = await repository!.UpdatePayOutRequestById(paymentRequestId, paymentRequest);

            r.PaymentRequest = result;

            return r;
        }


        public async Task<MVPaymentRequest> UpdatePaymentRequestTransfer(string orgId, string paymentRequestId, MPaymentRequest paymentRequest, MBankAccount srcBa)
        {
            repository!.SetCustomOrgId(orgId); //ตรงนี้เป็น orgId ของ Merchant
            _bankAccountRepo!.SetCustomOrgId("global");

            var r = new MVPaymentRequest()
            {
                Status = "OK",
                Description = "Success",
            };

            var existing = await repository!.GetPaymentRequestById(paymentRequestId);
            if (existing == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Request ID [{paymentRequestId}] not found for the organization [{orgId}]";

                return r;
            }

            if (existing.Status != "Pending")
            {
                r.Status = "INVALID_STATUS";
                r.Description = $"Payment Request ID [{paymentRequestId}] has invalid status [{existing.Status}] for update payout";

                return r;
            }

            paymentRequest.PayoutBankAccountName = srcBa!.AccountName;
            paymentRequest.PayoutBankAccountNo = srcBa.AccountNumber;
            paymentRequest.PayoutBankCode = srcBa.BankCode;
            paymentRequest.PayoutPromptPayId = srcBa.PromptPayId;
            paymentRequest.PayoutAccountType = srcBa.AccountType;
            paymentRequest.PayoutAccountLevel = srcBa.AccountLevel;
            paymentRequest.PayoutFeePct = 0;
            paymentRequest.PayoutBankAccountId = srcBa.Id.ToString();

            var result = await repository!.UpdateTransferRequestById(paymentRequestId, paymentRequest);

            r.PaymentRequest = result;

            return r;
        }

        public async Task<MVPaymentRequest> RejectPaymentRequestTransfer(string orgId, string paymentRequestId, MPaymentRequest paymentRequest)
        {
            repository!.SetCustomOrgId(orgId); //ตรงนี้เป็น global ได้
            _bankAccountRepo!.SetCustomOrgId("global");

            var r = new MVPaymentRequest()
            {
                Status = "OK",
                Description = "Success",
            };

            var existing = await repository!.GetPaymentRequestById(paymentRequestId);
            if (existing == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Request ID [{paymentRequestId}] not found for the organization [{orgId}]";

                return r;
            }

            if (existing.Status != "Pending")
            {
                r.Status = "INVALID_STATUS";
                r.Description = $"Payment Request ID [{paymentRequestId}] has invalid status [{existing.Status}] for update payout";

                return r;
            }

            var result = await repository!.UpdatePaymentStatusRejectById(paymentRequestId, paymentRequest);
            r.PaymentRequest = result;

            return r;
        }

        public async Task<MVPaymentRequest> ApprovePaymentRequestTransfer(string orgId, string paymentRequestId, MPaymentRequest paymentRequest)
        {
            repository!.SetCustomOrgId(orgId); //ตรงนี้เป็น global ได้
            _bankAccountRepo!.SetCustomOrgId("global");

            var r = new MVPaymentRequest()
            {
                Status = "OK",
                Description = "Success",
            };

            var existing = await repository!.GetPaymentRequestById(paymentRequestId);
            if (existing == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Request ID [{paymentRequestId}] not found for the organization [{orgId}]";

                return r;
            }

            if (existing.Status != "Pending")
            {
                r.Status = "INVALID_STATUS";
                r.Description = $"Payment Request ID [{paymentRequestId}] has invalid status [{existing.Status}] for update payout";

                return r;
            }

            var mvPtx = await ProcessTransferTx(existing.OrgId!, paymentRequest, existing);
            if (mvPtx.Status != "OK")
            {
                r.Status = mvPtx.Status;
                r.Description = mvPtx.Description;

                return r;
            }

            var srcBankAccount = await _bankAccountRepo!.GetBankAccountById(paymentRequest.PayoutBankAccountId!);
            if (srcBankAccount == null)
            {
                r.Status = "PAYOUT_BANK_ACCOUNT_NOT_FOUND";
                r.Description = $"Payout bank account ID [{paymentRequest.PayoutBankAccountId}] not found";

                return r;
            }

            paymentRequest.PayoutBankCode = srcBankAccount.BankCode;
            paymentRequest.PayoutBankAccountNo = srcBankAccount.AccountNumber;
            paymentRequest.PayoutBankAccountName = srcBankAccount.AccountName;
            paymentRequest.PayoutAccountType = srcBankAccount.AccountType;
            paymentRequest.PayoutPromptPayId = srcBankAccount.PromptPayId;
            paymentRequest.PayoutAccountLevel = srcBankAccount.AccountLevel;
            paymentRequest.PayoutFeePct = paymentRequest.PayoutFeePct;

            paymentRequest.PaymentTxId = mvPtx.PaymentTransaction!.Id.ToString();

            var result = await repository!.UpdatePaymentStatusApprovedById(paymentRequestId, paymentRequest);
            r.PaymentRequest = result;
            r.PayoutTransaction = mvPtx.PaymentTransaction;

            return r;
        }

        public async Task<MVPaymentRequest> RejectPaymentRequestPayOut(string orgId, string paymentRequestId, MPaymentRequest paymentRequest)
        {
            repository!.SetCustomOrgId(orgId); //ตรงนี้เป็น global ได้
            _bankAccountRepo!.SetCustomOrgId("global");

            var r = new MVPaymentRequest()
            {
                Status = "OK",
                Description = "Success",
            };

            var existing = await repository!.GetPaymentRequestById(paymentRequestId);
            if (existing == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Request ID [{paymentRequestId}] not found for the organization [{orgId}]";

                return r;
            }

            if (existing.Status != "Pending")
            {
                r.Status = "INVALID_STATUS";
                r.Description = $"Payment Request ID [{paymentRequestId}] has invalid status [{existing.Status}] for update payout";

                return r;
            }

            var result = await repository!.UpdatePaymentStatusRejectById(paymentRequestId, paymentRequest);
            r.PaymentRequest = result;

            return r;
        }

        public async Task<MVPaymentRequest> ApprovePaymentRequestPayOut(string orgId, string paymentRequestId, MPaymentRequest paymentRequest)
        {
            repository!.SetCustomOrgId(orgId); //ตรงนี้เป็น global ได้
            _bankAccountRepo!.SetCustomOrgId("global");

            var r = new MVPaymentRequest()
            {
                Status = "OK",
                Description = "Success",
            };

            var existing = await repository!.GetPaymentRequestById(paymentRequestId);
            if (existing == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Request ID [{paymentRequestId}] not found for the organization [{orgId}]";

                return r;
            }

            if (existing.Status != "Pending")
            {
                r.Status = "INVALID_STATUS";
                r.Description = $"Payment Request ID [{paymentRequestId}] has invalid status [{existing.Status}] for update payout";

                return r;
            }

            var mvPtx = await ProcessPayoutTx(existing.OrgId!, paymentRequest, existing);
            if (mvPtx.Status != "OK")
            {
                r.Status = mvPtx.Status;
                r.Description = mvPtx.Description;

                return r;
            }

            var srcBankAccount = await _bankAccountRepo!.GetBankAccountById(paymentRequest.PayoutBankAccountId!);
            if (srcBankAccount == null)
            {
                r.Status = "PAYOUT_BANK_ACCOUNT_NOT_FOUND";
                r.Description = $"Payout bank account ID [{paymentRequest.PayoutBankAccountId}] not found";

                return r;
            }

            paymentRequest.PayoutBankCode = srcBankAccount.BankCode;
            paymentRequest.PayoutBankAccountNo = srcBankAccount.AccountNumber;
            paymentRequest.PayoutBankAccountName = srcBankAccount.AccountName;
            paymentRequest.PayoutAccountType = srcBankAccount.AccountType;
            paymentRequest.PayoutPromptPayId = srcBankAccount.PromptPayId;
            paymentRequest.PayoutAccountLevel = srcBankAccount.AccountLevel;
            paymentRequest.PayoutFeePct = paymentRequest.PayoutFeePct;

            paymentRequest.PaymentTxId = mvPtx.PaymentTransaction!.Id.ToString();

            var result = await repository!.UpdatePaymentStatusApprovedById(paymentRequestId, paymentRequest);
            r.PaymentRequest = result;
            r.PayoutTransaction = mvPtx.PaymentTransaction;

            return r;
        }

        private async Task<MVPaymentTransaction> ProcessPayoutTx(string orgId, 
            MPaymentRequest paymentRequest, 
            MPaymentRequest existing)
        {
            _paymentTransactionRepo!.SetCustomOrgId(orgId); //ให้เป็นของ orgId ของ merchant
            var mvPt = new MVPaymentTransaction()
            {
                Status = "OK",
                Description = "Success",
            };

            var pt = new MPaymentTransaction
            {
                Status = "Approved",
                Direction = "PayOut",
                Currency = "THB",
                TxAmount = (double) existing.RequestedAmount!,
                TxAmountDecimal = (decimal) existing.RequestedAmount!,
                FromBankAccountNo = existing.PayoutBankAccountNo,
                FromBankCode = existing.PayoutBankCode,
                PayOutFeePct = existing.PayoutFeePct,
                PaymentRequestId = existing.Id.ToString(),
            };

            pt.PayOutFee = (double) Math.Round((decimal) (pt.TxAmount * existing.PayoutFeePct! / 100.0), 2, MidpointRounding.AwayFromZero);
            pt.PayOutTotalAmount = pt.TxAmount - pt.PayOutFee;

            pt.PayoutFeeDecimal = (decimal) pt.PayOutFee!;
            pt.PayOutTotalAmountDecimal = pt.TxAmountDecimal - pt.PayoutFeeDecimal;

            pt.PayOutBankAccountId = paymentRequest.PayoutBankAccountId;
            pt.PayOutBankCode = paymentRequest.PayoutBankCode;
            pt.PayInBankAccountNo = paymentRequest.PayinBankAccountNo;
            pt.PayInBankAccountName = paymentRequest.PayinBankAccountName;

            pt.MerchantId = existing.MerchantId;

            var srcBankAccountId = paymentRequest.PayoutBankAccountId!; //อันนี้คือ bank account ที่เป็น pool กลาง
            var dstBankAccountId = existing.PayinBankAccountId!; //อันนี้คือ bank account ของ merchant ที่จะเอาเงินเข้าไปให้

            var mcWallet = await _pointService!.GetWalletByMerchantId(orgId, existing.MerchantId!);
            if (mcWallet!.Status != "OK")
            {
                mvPt.Status = mcWallet.Status;
                mvPt.Description = $"Failed to get merchant wallet, MerchantId=[{existing.MerchantId}]";
                return(mvPt);
            }

            var baWallet = await _pointService!.GetWalletByBankAccountId("global", paymentRequest.PayoutBankAccountId!);
            if (baWallet!.Status != "OK")
            {
                mvPt.Status = baWallet.Status;
                mvPt.Description = $"Failed to get bank account wallet, BankAccountId=[{paymentRequest.PayoutBankAccountId}]";
                return(mvPt);
            }

            var merchantWallet = mcWallet.Wallet;
            var bankWallet = baWallet.Wallet;

            var merchantDeductAmt = pt.TxAmountDecimal;
            var merchantCurrentBalance = merchantWallet!.PointBalanceDecimal;
            var bankAccountDeductAmt = pt.PayOutTotalAmountDecimal;
            var bankAccountCurrentBalance = bankWallet!.PointBalanceDecimal;

            //ทำการเช็ค ยอด balance ของ merchant และ bank account ที่โอนออกด้วยว่าพอหรือไม่ ถ้าไม่พอก็ต้อง reject การจ่ายเงินออกครั้งนี้ไปเลย
            if (merchantCurrentBalance < merchantDeductAmt)
            {
                mvPt.Status = "ERROR_INSUFFICIENT_BALANCE";
                mvPt.Description = $"Merchant wallet has insufficient balance, MerchantId=[{existing.MerchantId}], WalletId=[{merchantWallet.Id}], CurrentBalance=[{merchantCurrentBalance}], RequiredAmount=[{merchantDeductAmt}]";
                return mvPt;
            }

            if (bankAccountCurrentBalance < bankAccountDeductAmt)
            {
                mvPt.Status = "ERROR_INSUFFICIENT_BALANCE";
                mvPt.Description = $"Bank account has insufficient balance, BankAccountId=[{bankWallet.Id}], CurrentBalance=[{bankAccountCurrentBalance}], RequiredAmount=[{bankAccountDeductAmt}]";
                return mvPt;
            }

            //===== update point wallet ===            
            var pointTx1 = new MPointTx()
            {
                WalletId = merchantWallet!.Id.ToString(),

                TxAmount =  (long) Math.Floor((decimal) pt.TxAmount!), //เอาส่วนจำนวนเต็มมาเท่านั้น
                //TxAmountDecimal ตรงนี้จะเป็นค่าที่แจ้งโอนออก
                TxAmountDecimal = merchantDeductAmt,

                Tags = $"PayOutRequestId=[{existing.Id.ToString()}]",
            };
            await _pointService!.DeductPoint(orgId, pointTx1);

            var pointTx2 = new MPointTx()
            {
                WalletId = bankWallet!.Id.ToString(),

                TxAmount =  0, //นับจำนวนครั้ง
                //TxAmountDecimal ตรงนี้จะเป็นค่าที่โอนเข้าจริง ๆ ซึ่งจะต้องเป็นจำนวนเงินที่หักค่าธรรมเนียมออกไปแล้ว
                TxAmountDecimal = bankAccountDeductAmt,

                Tags = $"PayOutRequestId=[{existing.Id.ToString()}]",
            };
            var pointVm = await _pointService!.DeductPoint("global", pointTx2);
            if (pointVm.Status != "OK")
            {
                mvPt.Status = pointVm.Status;
                mvPt.Description = $"{pointVm.Description}, [{bankWallet.PointBalanceDecimal}], [{bankWallet.PointBalance}] WalletId=[{bankWallet.Id}], amount=[{pointTx2.TxAmountDecimal}]";
                return mvPt;
            }

            //===== update point wallet ===


            pt.PayInBankAccountId = dstBankAccountId; //ของ merchant
            pt.PayOutBankAccountId = srcBankAccountId; //ของ pool กลาง

            var mpt = await _paymentTransactionRepo!.AddPaymentTransaction(pt);
            mvPt.PaymentTransaction = mpt;

            return mvPt;
        }

        private async Task<MVPaymentTransaction> ProcessTransferTx(string orgId, 
            MPaymentRequest paymentRequest, 
            MPaymentRequest existing)
        {
            _paymentTransactionRepo!.SetCustomOrgId(orgId); //ให้เป็นของ orgId ของ merchant
            var mvPt = new MVPaymentTransaction()
            {
                Status = "OK",
                Description = "Success",
            };

            var pt = new MPaymentTransaction
            {
                Status = "Approved",
                Direction = "Transfer",
                Currency = "THB",
                TxAmount = (double) existing.RequestedAmount!,
                TxAmountDecimal = (decimal) existing.RequestedAmount!,
                FromBankAccountNo = existing.PayoutBankAccountNo,
                FromBankCode = existing.PayoutBankCode,
                PayOutFeePct = 0, //ไม่มีค่าธรรมเนียม
                PaymentRequestId = existing.Id.ToString(),
            };

            pt.PayOutFee = (double) Math.Round((decimal) (pt.TxAmount * existing.PayoutFeePct! / 100.0), 2, MidpointRounding.AwayFromZero);
            pt.PayOutTotalAmount = pt.TxAmount - pt.PayOutFee;

            pt.PayoutFeeDecimal = (decimal) pt.PayOutFee!;
            pt.PayOutTotalAmountDecimal = pt.TxAmountDecimal - pt.PayoutFeeDecimal;

            pt.PayOutBankAccountId = paymentRequest.PayoutBankAccountId;
            pt.PayOutBankCode = paymentRequest.PayoutBankCode;
            pt.PayInBankAccountNo = paymentRequest.PayinBankAccountNo;
            pt.PayInBankAccountName = paymentRequest.PayinBankAccountName;

            pt.MerchantId = existing.MerchantId;

            var srcBankAccountId = paymentRequest.PayoutBankAccountId!; //อันนี้คือ bank account ที่เป็น pool กลาง
            var dstBankAccountId = existing.PayinBankAccountId!; //อันนี้คือ bank account ของ merchant ที่จะเอาเงินเข้าไปให้


            var baWallet = await _pointService!.GetWalletByBankAccountId("global", paymentRequest.PayoutBankAccountId!);
            if (baWallet!.Status != "OK")
            {
                mvPt.Status = baWallet.Status;
                mvPt.Description = $"Failed to get bank account wallet, BankAccountId=[{paymentRequest.PayoutBankAccountId}]";
                return(mvPt);
            }

            var bankWallet = baWallet.Wallet;

            var bankAccountDeductAmt = pt.PayOutTotalAmountDecimal;
            var bankAccountCurrentBalance = bankWallet!.PointBalanceDecimal;

            if (bankAccountCurrentBalance < bankAccountDeductAmt)
            {
                mvPt.Status = "ERROR_INSUFFICIENT_BALANCE";
                mvPt.Description = $"Bank account has insufficient balance, BankAccountId=[{bankWallet.Id}], CurrentBalance=[{bankAccountCurrentBalance}], RequiredAmount=[{bankAccountDeductAmt}]";
                return mvPt;
            }

            //===== Start : update point wallet ===            
            var pointTx2 = new MPointTx()
            {
                WalletId = bankWallet!.Id.ToString(),

                TxAmount =  0, //นับจำนวนครั้ง
                //TxAmountDecimal ตรงนี้จะเป็นค่าที่โอนเข้าจริง ๆ ซึ่งจะต้องเป็นจำนวนเงินที่หักค่าธรรมเนียมออกไปแล้ว
                TxAmountDecimal = bankAccountDeductAmt,

                Tags = $"TransferRequestId=[{existing.Id.ToString()}]",
            };
            var pointVm = await _pointService!.DeductPoint("global", pointTx2);
            if (pointVm.Status != "OK")
            {
                mvPt.Status = pointVm.Status;
                mvPt.Description = $"{pointVm.Description}, [{bankWallet.PointBalanceDecimal}], [{bankWallet.PointBalance}] WalletId=[{bankWallet.Id}], amount=[{pointTx2.TxAmountDecimal}]";
                return mvPt;
            }
            //===== End : update point wallet ===


            pt.PayInBankAccountId = dstBankAccountId; //Transit bank account
            pt.PayOutBankAccountId = srcBankAccountId; //ของ pool กลาง

            var mpt = await _paymentTransactionRepo!.AddPaymentTransaction(pt);
            mvPt.PaymentTransaction = mpt;

            return mvPt;
        }

        public async Task<MVPaymentRequest> AddPaymentRequestTransfer(string orgId, MPaymentRequest paymentRequest, MBankAccount destBa, MBankAccount srcBa)
        {
            repository!.SetCustomOrgId(orgId); //global
            _bankAccountRepo!.SetCustomOrgId("global");

            var r = new MVPaymentRequest()
            {
                Status = "OK",
                Description = "Success",
            };

            if (string.IsNullOrEmpty(paymentRequest.RefId))
            {
                r.Status = "REF_ID_MISSING";
                r.Description = $"Ref ID is missing!!!";

                return r;
            }

            var isRefIdExist = await repository!.IsRefIdExist(paymentRequest.RefId);
            if (isRefIdExist)
            {
                r.Status = "REF_ID_DUPLICATE";
                r.Description = $"Ref ID [{paymentRequest.RefId}] is duplicate!!!";

                return r;
            }

            if (paymentRequest.Currency != "THB")
            {
                r.Status = "CURRENCY_NOT_SUPPORT";
                r.Description = $"Currency [{paymentRequest.Currency}] not currently support, only THB is allowed.";

                return r;
            }

            if (paymentRequest.QrProvider != "PP") //PromptPay
            {
                //ตอนนี้ support แค่ PromptPay
                r.Status = "BANK_PROVIDER_NOT_SUPPORT";
                r.Description = $"Provider [{paymentRequest.QrProvider}] not currently support, only PP is allowed.";

                return r;
            }

            if (paymentRequest.RequestedAmount <= 0)
            {
                r.Status = "INVALID_PAYMENT_AMOUNT";
                r.Description = $"Request amount [{paymentRequest.RequestedAmount}] must be greater than 0.00";

                return r;
            }

            paymentRequest.ResponseData = "{}";
            paymentRequest.ProcessingMessages = "[]";

            //Logic สำหรับการสร้าง QR payment ตรงนี้
            paymentRequest.Status = "Pending";
            paymentRequest.Direction = "Transit";

            //ต่อให้เป็น Transit เราก็จะใช้ฟีลด์ที่ขึ้นต้นด้วย PayinXXX
            paymentRequest.PayinBankAccountName = destBa.AccountName;
            paymentRequest.PayinBankAccountNo = destBa.AccountNumber;
            paymentRequest.PayinBankCode = destBa.BankCode;
            paymentRequest.PayinPromptPayId = destBa.PromptPayId;
            paymentRequest.PayinAccountType = destBa.AccountType;
            paymentRequest.PayinAccountLevel = destBa.AccountLevel;
            paymentRequest.PayInFeePct = 0;
            paymentRequest.PayinBankAccountId = destBa.Id.ToString();
            paymentRequest.PayoutFeePct = 0;
            paymentRequest.GeneratedAmount = paymentRequest.RequestedAmount;

            var requestAmt = paymentRequest.RequestedAmount ?? 0;
            var payoutFee = Math.Round((decimal) (requestAmt * paymentRequest.PayoutFeePct! / 100.0), 2, MidpointRounding.AwayFromZero);

            paymentRequest.PayOutTotalAmountDecimal = ((decimal) requestAmt) - payoutFee;
            paymentRequest.PayoutFeeDecimal = payoutFee;

            //บัญชีต้นทาง
            paymentRequest.PayoutBankAccountName = srcBa.AccountName;
            paymentRequest.PayoutBankAccountNo = srcBa.AccountNumber;
            paymentRequest.PayoutBankCode = srcBa.BankCode;
            paymentRequest.PayoutPromptPayId = srcBa.PromptPayId;
            paymentRequest.PayoutAccountType = srcBa.AccountType;
            paymentRequest.PayoutAccountLevel = srcBa.AccountLevel;
            paymentRequest.PayInFeePct = 0;
            paymentRequest.PayoutBankAccountId = srcBa.Id.ToString();
   
            //สร้าง QR
            IQrGenerator qrGenerator;
            QrGeneratorResult? qrResult = null;
            if (destBa.AccountType == "PromptPay")
            {
                var tmpPr = new MPaymentRequest()
                {
                    RefId = paymentRequest.RefId,
                    GeneratedAmount = (double) paymentRequest.PayOutTotalAmountDecimal,
                };

                qrGenerator = new QrGeneratorPromptPay(tmpPr, destBa);
                qrResult = qrGenerator.Generate();
            }
            paymentRequest.QrCode = qrResult?.QrPayload;

            var result = await repository!.AddPaymentRequest(paymentRequest);

            r.PaymentRequest = result;

            return r;
        }

        public async Task<MVPaymentRequest> AddPaymentRequestPayOut(string orgId, MPaymentRequest paymentRequest, MMerchant merchant, MBankAccount bankAccount)
        {
            repository!.SetCustomOrgId(orgId); //ตรงนี้เป็น orgId ของ Merchant
            _bankAccountRepo!.SetCustomOrgId("global");

            var r = new MVPaymentRequest()
            {
                Status = "OK",
                Description = "Success",
            };

            if (string.IsNullOrEmpty(paymentRequest.RefId))
            {
                r.Status = "REF_ID_MISSING";
                r.Description = $"Ref ID is missing!!!";

                return r;
            }

            var isRefIdExist = await repository!.IsRefIdExist(paymentRequest.RefId);
            if (isRefIdExist)
            {
                r.Status = "REF_ID_DUPLICATE";
                r.Description = $"Ref ID [{paymentRequest.RefId}] is duplicate!!!";

                return r;
            }

            if (paymentRequest.Currency != "THB")
            {
                r.Status = "CURRENCY_NOT_SUPPORT";
                r.Description = $"Currency [{paymentRequest.Currency}] not currently support, only THB is allowed.";

                return r;
            }

            if (paymentRequest.QrProvider != "PP") //PromptPay
            {
                //ตอนนี้ support แค่ PromptPay
                r.Status = "BANK_PROVIDER_NOT_SUPPORT";
                r.Description = $"Provider [{paymentRequest.QrProvider}] not currently support, only PP is allowed.";

                return r;
            }

            if (paymentRequest.RequestedAmount <= 0)
            {
                r.Status = "INVALID_PAYMENT_AMOUNT";
                r.Description = $"Request amount [{paymentRequest.RequestedAmount}] must be greater than 0.00";

                return r;
            }

            paymentRequest.ResponseData = "{}";
            paymentRequest.ProcessingMessages = "[]";

            //Logic สำหรับการสร้าง QR payment ตรงนี้
            paymentRequest.Status = "Pending";
            paymentRequest.Direction = "PayOut";

            var bankAccountId = bankAccount.Id.ToString();
            if (!string.IsNullOrEmpty(bankAccountId))
            {
                paymentRequest.IsPayInBankAccountOverride = false;

                //ต่อให้เป็น PayOut เราก็จะใช้ฟีลด์ที่ขึ้นต้นด้วย PayinXXX
                paymentRequest.PayinBankAccountName = bankAccount.AccountName;
                paymentRequest.PayinBankAccountNo = bankAccount.AccountNumber;
                paymentRequest.PayinBankCode = bankAccount.BankCode;
                paymentRequest.PayinPromptPayId = bankAccount.PromptPayId;
                paymentRequest.PayinAccountType = bankAccount.AccountType;
                paymentRequest.PayinAccountLevel = bankAccount.AccountLevel;
            }
            else
            {
                //ส่งเข้ามาจาก merchant เอง
                paymentRequest.IsPayInBankAccountOverride = true;
                paymentRequest.PayinBankAccountNameOverride = bankAccount.AccountName;
                paymentRequest.PayinBankAccountNoOverride = bankAccount.AccountNumber;
                paymentRequest.PayinBankCodeOverride = bankAccount.BankCode;
                paymentRequest.PayinPromptPayIdOverride = bankAccount.PromptPayId;
                paymentRequest.PayinAccountTypeOverride = bankAccount.AccountType;
            }

            paymentRequest.PayInFeePct = merchant.PayinFeePct;
            paymentRequest.PayinBankAccountId = bankAccount.Id.ToString();
            paymentRequest.PayoutFeePct = merchant.PayinFeePct;
            paymentRequest.GeneratedAmount = paymentRequest.RequestedAmount;

            var requestAmt = paymentRequest.RequestedAmount ?? 0;
            var payoutFee = Math.Round((decimal) (requestAmt * paymentRequest.PayoutFeePct! / 100.0), 2, MidpointRounding.AwayFromZero);

            paymentRequest.PayOutTotalAmountDecimal = ((decimal) requestAmt) - payoutFee;
            paymentRequest.PayoutFeeDecimal = payoutFee;

            //สร้าง QR
            IQrGenerator qrGenerator;
            QrGeneratorResult? qrResult = null;
            if (bankAccount.AccountType == "PromptPay")
            {
                var tmpPr = new MPaymentRequest()
                {
                    RefId = paymentRequest.RefId,
                    GeneratedAmount = (double) paymentRequest.PayOutTotalAmountDecimal,
                };

                qrGenerator = new QrGeneratorPromptPay(tmpPr, bankAccount);
                qrResult = qrGenerator.Generate();
            }
            paymentRequest.QrCode = qrResult?.QrPayload;

            var result = await repository!.AddPaymentRequest(paymentRequest);

            r.PaymentRequest = result;

            return r;
        }

        private async Task<MTxBalance> GetMerchantCurrentDailyTxBalance(string orgId, MMerchant merchant)
        {
            var merchantId = merchant.Id.ToString()!;

            var cacheKey = CacheHelper.CreateMerchantDailyTxKey(orgId, merchantId);
            var cacheValue = await _redis!.GetObjectAsync<MTxBalance>(cacheKey);

            if (cacheValue == null)
            {
                cacheValue = new MTxBalance()
                {
                    TxAmount = 0,
                    TxCount = 0,
                };
            }

            return cacheValue;
        }

        public async Task<MVPaymentResponse> AddPaymentRequestPayIn(string orgId, MPaymentRequest paymentRequest, MMerchant merchant)
        {
            repository!.SetCustomOrgId(orgId); //ตรงนี้เป็น orgId ของ Merchant
            _bankAccountRepo!.SetCustomOrgId("global");

            var r = new MVPaymentResponse()
            {
                Status = "OK",
                Description = "Success",
            };

            if (string.IsNullOrEmpty(paymentRequest.RefId1))
            {
                r.Status = "REF_ID1_MISSING";
                r.Description = $"Ref ID1 is missing!!!";

                return r;
            }

            paymentRequest.RefId = paymentRequest.RefId1; /* RefId ไม่ใช้แล้วแต่ก็ set ให้ท่ากับ RefId1 ไปเลย แล้วยังคงความป็น unique อยู่นะ */
            if (string.IsNullOrEmpty(paymentRequest.RefId))
            {
                r.Status = "REF_ID_MISSING";
                r.Description = $"Ref ID is missing!!!";

                return r;
            }

            var isRefIdExist = await repository!.IsRefIdExist(paymentRequest.RefId);
            if (isRefIdExist)
            {
                r.Status = "REF_ID_DUPLICATE";
                r.Description = $"Ref ID1 [{paymentRequest.RefId}] is duplicate!!!";

                return r;
            }

            if (paymentRequest.Currency != "THB")
            {
                r.Status = "CURRENCY_NOT_SUPPORT";
                r.Description = $"Currency [{paymentRequest.Currency}] not currently support, only THB is allowed.";

                return r;
            }

            if (!allowedQrProvider.Contains(paymentRequest.QrProvider))
            {
                r.Status = "BANK_PROVIDER_NOT_SUPPORT";
                r.Description = $"Provider [{paymentRequest.QrProvider}] not currently support, only [{string.Join(", ", allowedQrProvider)}] are allowed.";

                return r;
            }

            if (paymentRequest.RequestedAmount <= 0)
            {
                r.Status = "INVALID_PAYMENT_AMOUNT";
                r.Description = $"Request amount [{paymentRequest.RequestedAmount}] must be greater than 0.00";

                return r;
            }

            var currentDailyTxBalance = await GetMerchantCurrentDailyTxBalance("global", merchant);

            var txAmountLimit = merchant.PayinDailyTxAmountLimit;
            if (txAmountLimit > 0)
            {
                //จะทำการ check ถ้า set ค่า txAmountLimit ไว้มากกว่า 0
                if ((currentDailyTxBalance.TxAmount + (decimal) paymentRequest.RequestedAmount!) > txAmountLimit)
                {
                    r.Status = "ERROR_DAILY_AMOUNT_EXCEEDED";
                    r.Description = $"Merchant daily transaction amount exceeded, CurrentDailyTxAmount=[{currentDailyTxBalance.TxAmount}], RequestedAmount=[{paymentRequest.RequestedAmount}], MaxDailyAmount=[{txAmountLimit}]";
                }
            }

            var txCountLimit = merchant.PayinDailyTxCountLimit;
            if (txCountLimit > 0)
            {
                //จะทำการ check ถ้า set ค่า txCountLimit ไว้มากกว่า 0
                if ((currentDailyTxBalance.TxCount + 1) > txCountLimit)
                {
                    r.Status = "ERROR_DAILY_COUNT_EXCEEDED";
                    r.Description = $"Merchant daily transaction count exceeded, CurrentDailyTxCount=[{currentDailyTxBalance.TxCount}], MaxDailyCount=[{txCountLimit}]";
                }
            }

            //Validate ว่า amount เกิน range ของ merchant มั้ย
            var minAmt = merchant.PayinMinAmount;
            var maxAmt = merchant.PayinMaxAmount;
            var requestAmt = paymentRequest.RequestedAmount;

            if ((requestAmt < minAmt) || (requestAmt > maxAmt))
            {
                r.Status = "ERROR_VALUE_NOT_IN_RANGE";
                r.Description = $"Amount [{requestAmt}] not in allow range -> [{minAmt}, {maxAmt}]";

                return r;
            }

            paymentRequest.GeneratedAmount = GetGeneratedAmount(paymentRequest, merchant);

            var (bnkAcct, lines) = await GetPayInBankAccount(paymentRequest, merchant);
            if (bnkAcct == null)
            {
                r.Status = "ERROR_NO_PAYIN_ACCOUNT_MATCH";
                r.Description = $"No pay-in bank account match!!!";

                return r;
            }

            var pmResponse = await CreatePaymentResponse(paymentRequest, bnkAcct);
            if (pmResponse.Status != "OK")
            {
                return pmResponse;
            }

            var jsonString = JsonSerializer.Serialize(pmResponse.PaymentResponse);
            paymentRequest.ResponseData = jsonString;

            var messageString = JsonSerializer.Serialize(lines);
            paymentRequest.ProcessingMessages = messageString;

            //Logic สำหรับการสร้าง QR payment ตรงนี้
            paymentRequest.Status = "Pending";
            paymentRequest.Direction = "PayIn";
            paymentRequest.PayinBankAccountName = bnkAcct.AccountName;
            paymentRequest.PayinBankAccountNo = bnkAcct.AccountNumber;
            paymentRequest.PayinBankCode = bnkAcct.BankCode;
            paymentRequest.PayinPromptPayId = bnkAcct.PromptPayId;
            paymentRequest.PayinAccountType = bnkAcct.AccountType;
            paymentRequest.PayinAccountLevel = bnkAcct.AccountLevel;
            paymentRequest.PayInFeePct = merchant.PayinFeePct;
            paymentRequest.PayinBankAccountId = bnkAcct.Id.ToString();

            _ = await repository!.AddPaymentRequest(paymentRequest);

            return pmResponse;
        }

        private async Task<(MBankAccount?, List<string>)> GetPayInBankAccount(MPaymentRequest pr, MMerchant merchant)
        {
            var merchantId = pr.MerchantId!;
            List<string> lines = [];

            //1. เลือก list ของ bank account ที่ตรงกับ QrProvider ขึ้นมา
            //2. แต่ละ bank account ให้ดูว่าเกิน condition ที่ตั้งไว้มั้ยเช่น 
            //   2.1 ยอดรวมต่อวัน
            //   2.2 เป็น bank account ของ merchant นั้นหรือไม่ ดูจากว่าเป็น global หรือ selected
            //   2.3 bank account นั้น active อยู่หรือไม่ 
            //   2.5 ดู bank account ที่ match SelectedPayInBankAccountId มั้ยถ้า SelectedPayInBankAccountId ไม่เป็น null or empty
            //       2.5.1 อันนี้ทำเพื่อให้ผู้ใช้ระบุ PayIn bank account ID เข้ามาเองเลย
            //3. เลือกตัวแรกที่เงื่อนไขผ่าน

            if (!string.IsNullOrEmpty(pr.SelectedPayInBankAccountId))
            {
                lines.Add($"Step01 - User specified bank account ID : SelectedPayInBankAccountId -> [{pr.SelectedPayInBankAccountId}]");
                //มีการระบุ Bank Account ID เข้ามาเองโดย user
                var bankAcct = await _bankAccountRepo!.GetBankAccountById(pr.SelectedPayInBankAccountId);

                //ไม่ต้องเข็ค daily limit ตรงนี้ เพราะถือว่า user ระบุเข้ามาเองแล้ว
                return (bankAcct, lines);
            }

            //QrProvider != "PP" หมายถึงชื่อธนาคารแบบ Native โดยตรง (เช่น "SCB") ใช้ค่านี้ filter ทั้ง AccountType และ BankCode ด้านล่าง
            var accountType = pr.QrProvider == "PP" ? "PromptPay" : "Native";
            lines.Add($"Step02 - Get bank account type : accountType -> [{accountType}]");

            var param = new VMBankAccount()
            {
                AccountType = accountType,
                AccountCategory = "PayIn",
                AccountLevel = "", //เอามาทั้ง global และ selected แล้วค่อยมาเลือกอีกที
            };


            var selectedBankAccounts = await _bankAccountRepo!.GetPayInBankAccountsForMerchant(merchantId);
            var dict = selectedBankAccounts.ToDictionary(g => g.BankAccountId!, g => g.AccountNumber);

            lines.Add($"Step03 - Get all PayIn bank accounts : accountType -> [{accountType}]");

            var rawBankAccounts = await _bankAccountRepo!.GetAllBankAccounts(param); //ไม่มีเรื่องการทำ paging ตรงนี้ ถ้ามี bank account เยอะค่อยว่ากันในอนาคต

            //ควรปรับให้เอา AccountLevel ที่เป็น Selected ขึ้นมาก่อน
            var bankAccounts = rawBankAccounts
                .OrderByDescending(x => x.AccountLevel)
                .ThenByDescending(x => x.CreatedDate)
                .ToList();

            //TODO : อนาคตอาจจะให้ loop จาก selectedBankAccounts union กับ global bank accounts ก็ได้
            //จะดีกว่าในกรณีที่มี bank accounts เยอะ ๆ มาก ๆ ในระบบ แต่เลือกมาสำหรับ merchant นั้นไม่กี bank account
            foreach (var bankAccount in bankAccounts)
            {
                var bankAccountId = bankAccount.Id.ToString()!;
                var bankCode = bankAccount.BankCode;
                var bankAccountNo = bankAccount.AccountNumber;
                var bankAccountName = bankAccount.AccountName;
                var promptPayId = bankAccount.PromptPayId;

                if (bankAccount.Status == "Disabled")
                {
                    lines.Add($"Step03 - Skip disabled bank account : Account -> [{bankCode} - {bankAccountName}] [bankAccountNo] [{promptPayId}]");
                    continue;
                }

                if (!IsBankAccountNameWhitelisted(merchant, bankAccountName, out var whitelistReason))
                {
                    lines.Add($"Step03.1 - Skip bank account, name not in merchant whitelist ({whitelistReason}) : Account -> [{bankCode} - {bankAccountName}] [bankAccountNo] [{promptPayId}]");
                    continue;
                }

                //ถ้า provider ไม่ใช่ "PP" แสดงว่าเจาะจงธนาคารแบบ Native (เช่น "SCB") ต้องเช็ค BankCode ให้ตรงกับ QrProvider ด้วย
                //ไม่งั้น auto-select อาจไปหยิบ native bank account ของธนาคารอื่นที่ไม่รองรับ QrProvider นี้มาใช้
                if (pr.QrProvider != "PP" && bankCode != pr.QrProvider)
                {
                    lines.Add($"Step03.2 - Skip bank account, bank code not match QrProvider [{pr.QrProvider}] : Account -> [{bankCode} - {bankAccountName}] [bankAccountNo] [{promptPayId}]");
                    continue;
                }

                //Here - Bank Account Status is "Active"
                if (bankAccount.AccountLevel == "Global")
                {
                    if (!merchant.IncludeGlobalBankAccount)
                    {
                        lines.Add($"Step04.0 - Skip global bank account, merchant not allowed to use global bank account : Account -> [{bankCode} - {bankAccountName}] [bankAccountNo] [{promptPayId}]");
                        continue;
                    }

                    //ให้เช็คต่อว่ายอด daily balance ของ bank account นี้เกิน limit หรือยัง ถ้าเกินก็ skip ไป
                    var ba1 = await _bankAccountRepo!.GetBankAccountById(bankAccountId);
                    var mvBa1 = await IsDailyBalanceExceeded(ba1!);
                    if (mvBa1.Status == "YES")
                    {
                        lines.Add($"Step04.1 - Skip global bank account, daily balance exceeded ({mvBa1.Description}) : Account -> [{bankCode} - {bankAccountName}] [bankAccountNo] [{promptPayId}]");
                        continue;
                    }

                    lines.Add($"Step04 - Use global bank account : Account -> [{bankCode} - {bankAccountName}] [bankAccountNo] [{promptPayId}]");
                    return (ba1, lines);
                }

                if (bankAccount.AccountLevel == "Selected")
                {
                    //ต้องดูว่า merchant นั้นได้ผูกกับ bank นี้ไว้หรือไม่
                    if (dict.ContainsKey(bankAccountId))
                    {
                        //ให้เช็คต่อว่ายอด daily balance ของ bank account นี้เกิน limit หรือยัง ถ้าเกินก็ skip ไป
                        var ba2 = await _bankAccountRepo!.GetBankAccountById(bankAccountId);
                        var mvBa2 = await IsDailyBalanceExceeded(ba2!);
                        if (mvBa2.Status == "YES")
                        {
                            lines.Add($"Step05.0 - Skip selected bank account, daily balance exceeded ({mvBa2.Description}) : Account -> [{bankCode} - {bankAccountName}] [bankAccountNo] [{promptPayId}]");
                            continue;
                        }

                        lines.Add($"Step05.1 - Use selected bank account : Account -> [{bankCode} - {bankAccountName}] [bankAccountNo] [{promptPayId}]");
                        return (ba2, lines);
                    }
                    else
                    {
                        lines.Add($"Step05.2 - Skip unselected bank account : Account -> [{bankCode} - {bankAccountName}] [bankAccountNo] [{promptPayId}]");                        
                    }
                }
            }
            
            lines.Add($"Step06 - No bank account match!!!");

            //ไม่มี bank account ที่ match
            return (null, lines);
        }

        private async Task<MVBankAccount> IsDailyBalanceExceeded(MBankAccount ba)
        {
            var result = new MVBankAccount()
            {
                Status = "NO",
                Description = "Success",
            };
            
            var dailyLimit = ba.DailyQuota;
            if (dailyLimit <= 0)
            {
                //ไม่มีการตั้งค่า daily limit เลยถือว่าไม่เกิน limit
                result.Status = "NO";
                result.Description = "No daily limit configured for bank account";
                return result;
            }

            //ให้เอา bankAccountId ไป lookup ใน Redis ว่ามี daily balance ของ bank account นี้เท่าไหร่แล้ว ถ้าเกิน limit ก็ return false
            var cacheKey = CacheHelper.CreatePayInBankAccountDailyTxKey("global", ba.Id.ToString()!);
            var cacheValue = await _redis!.GetObjectAsync<MTxBalance>(cacheKey);

            if (cacheValue == null)
            {
                result.Status = "NO";
                result.Description = "No daily balance found for bank account";
                return result;
            }

            //ตรงนี้ ยังไม่ต้องเอายอดเงินที่จะโอนมารวมกับ cacheValue.TxAmount
            if ((double) cacheValue.TxAmount >= dailyLimit)
            {
                result.Status = "YES";
                result.Description = $"Daily balance [{cacheValue.TxAmount}] exceeded, limit [{dailyLimit}]";
                return result;
            }

            return result;
        }

        //ถ้า merchant ไม่ได้กรอก whitelist ไว้เลย ก็ถือว่าใช้ได้กับทุกชื่อ bank account name
        //ถ้ากรอกไว้ ชื่อใน whitelist ต้องเป็น substring ของ bank account name ถึงจะถือว่าใช้ได้ (case-insensitive)
        private static bool IsBankAccountNameWhitelisted(MMerchant merchant, string? bankAccountName, out string reason)
        {
            var whitelist = merchant.WhitelistBankAccountNamesArr;
            if (whitelist == null || whitelist.Count == 0)
            {
                reason = "no whitelist configured for merchant, allow any account name";
                return true;
            }

            if (string.IsNullOrEmpty(bankAccountName))
            {
                reason = "bank account name is empty";
                return false;
            }

            foreach (var name in whitelist)
            {
                if (!string.IsNullOrEmpty(name) && bankAccountName.Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    reason = $"matched whitelist entry [{name}]";
                    return true;
                }
            }

            reason = "no whitelist entry matched";
            return false;
        }

        private async Task<MVPaymentResponse> CreatePaymentResponse(MPaymentRequest pr, MBankAccount bnkAcct)
        {
            var mvResponse = new MVPaymentResponse()
            {
                Status = "OK",
                Description = "Success",    
            };

            IQrGenerator qrGenerator;
            QrGeneratorResult? qrResult = null;

            if (pr.QrProvider == "PP")
            {
                qrGenerator = new QrGeneratorPromptPay(pr, bnkAcct);
                qrResult = qrGenerator.Generate();
            }
            else if (pr.QrProvider == "SCB")
            {
                qrGenerator = new QrGeneratorSCB(pr, bnkAcct, _redis);
                qrResult = await qrGenerator.GenerateAsync();
            }

            if (qrResult == null)
            {
                mvResponse.Status = "INVALID_QR_PROVIDER";
                mvResponse.Description = $"Invalid QR provider [{pr.QrProvider}]";
                return mvResponse;
            }
            else if (qrResult.Status != "OK")
            {
                //ส่ง error จริง ๆ ที่เกิดขึ้นเพื่อส่งออกไปด้วย เช่น ปัญหาจากการเรียก API ของธนาคาร
                mvResponse.Status = qrResult.Status;
                mvResponse.Description = qrResult.Description;
                return mvResponse;
            }

            var pmr = new MPaymentResponse()
            {
                CreatedAt = pr.CreatedDate,
                ExpireAt = pr.ExpireDate,

                Id = pr.Id.ToString(),
                ReferenceId = pr.RefId,
                SessionId = pr.Id.ToString(),
                WebsocketPath = "/realtime/payment-tx",
                Type = pr.Direction,
                Status = pr.Status,
                RequestedAmount = pr.RequestedAmount,
                GeneratedAmount = pr.GeneratedAmount, //ตรงนี้ต้อง random ทศนิยม
                Currency = pr.Currency,
                QrCodeImage = qrResult.Base64Image,
                QrCode = qrResult.QrPayload,

                PayInBankAccountName = bnkAcct.AccountName,
                PayInBankAccountNo = bnkAcct.AccountNumber,
                PayInBankCode = bnkAcct.BankCode,
                PayInPromptPayId = bnkAcct.PromptPayId,
            };

            mvResponse.PaymentResponse = pmr;

            return mvResponse;
        }

        public async Task<List<MPaymentRequest>> GetPaymentRequests(string orgId, VMPaymentRequest param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPaymentRequests(param);

            // ลบ ResponseData ออกเพื่อลด payload
            result.ForEach(p => 
            { 
                p.ResponseData = ""; 
                p.ProcessingMessages = "";
            });

            // ถ้าไม่ใช่ global ให้เหลือเฉพาะรายการของ orgId นั้น
            if (orgId != "global")
            {
                //ป้องกันความผิดพลาดไม่ให้ payment request ของ org หนึ่งไปโผล่ในอีก org หนึ่ง
                result.RemoveAll(p => p.OrgId != orgId);
            }

            return result;
        }

        public async Task<int> GetPaymentRequestCount(string orgId, VMPaymentRequest param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPaymentRequestCount(param);

            return result;
        }

        public async Task<MVPaymentRequest> UpdatePaymentRequestById(string orgId, string paymentRequestId, MPaymentRequest paymentRequest)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPaymentRequest()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(paymentRequestId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Payment Request ID [{paymentRequestId}] format is invalid";

                return r;
            }

            var result = await repository!.UpdatePaymentRequestById(paymentRequestId, paymentRequest);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Request ID [{paymentRequestId}] not found for the organization [{orgId}]";

                return r;
            }

            r.PaymentRequest = result;

            return r;
        }

        public async Task<MVPaymentRequest> DeletePayOutRequestById(string orgId, string paymentRequestId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPaymentRequest()
            {
                Status = "OK",
                Description = "Success",
            };

            var existing = await repository!.GetPaymentRequestById(paymentRequestId);
            if (existing == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Payment Request ID [{paymentRequestId}] not found";
                return r;
            }

            if (existing.Status?.ToLower() != "pending")
            {
                r.Status = "INVALID_STATUS";
                r.Description = "Only Pending requests can be deleted";
                return r;
            }

            await repository!.DeletePayOutRequestById(paymentRequestId);
            return r;
        }
    }
}
