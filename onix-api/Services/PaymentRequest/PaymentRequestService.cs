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
        private readonly IPaymentRequestRepository? repository = null;
        private readonly IPaymentTransactionRepository? _paymentTransactionRepo = null;
        private readonly IBankAccountRepository? _bankAccountRepo = null;
        private readonly IPointService? _pointService = null;

        public PaymentRequestService(
            IPaymentRequestRepository repo, 
            IPaymentTransactionRepository paymentTxRepo, 
            IBankAccountRepository bankAcctRepo,
            IPointService pointService) : base()
        {
            repository = repo;
            _paymentTransactionRepo = paymentTxRepo;
            _bankAccountRepo = bankAcctRepo;
            _pointService = pointService;
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

        private double? GetGeneratedAmount(MPaymentRequest paymentRequest, MMerchant merchant)
        {
            var amt = paymentRequest.RequestedAmount;
            if (amt == null)
            {
                return 0;
            }
            
            if (merchant.RandomDecimal == false)
            {
                //ไม่ต้องปรับอะไรทั้งนั้น
                return amt;
            }

            // merchant.RandomDecimal is true or null
            // เอาเฉพาะเลขหน้าทศนิยม
            var integerPart = Math.Truncate(amt.Value);

            // random ทศนิยม 01-99
            var random = new Random();
            var decimalPart = random.Next(1, 100);

            // ประกอบกลับเป็นจำนวนใหม่ เช่น 190 + 0.78 = 190.78
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

            //bankAccountId จะเป็น bank account ID ของฝั่งที่เงินจะออก ซึ่งคือ bank account ของ pool กลาง (ใน DB จะเป็น BankAccount.Direction = "PayIn")
            //Update ได้แต่เฉพาะ Payout เท่านั้น
            //เป็น bank account ที่จะถูกโอนเงินออก
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

            //bankAccountId จะเป็น bank account ID ของฝั่งที่เงินจะออก ซึ่งคือ bank account ของ pool กลาง (ใน DB จะเป็น BankAccount.Direction = "PayIn")
            //Update ได้แต่เฉพาะ Payout เท่านั้น
            //เป็น bank account ที่จะถูกโอนเงินออก
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

        private async Task<MVPaymentTransaction> ProcessPayoutTx(string orgId, MPaymentRequest paymentRequest, MPaymentRequest existing)
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

            //ต่อให้เป็น PayOut เราก็จะใช้ฟีลด์ที่ขึ้นต้นด้วย PayinXXX
            paymentRequest.PayinBankAccountName = bankAccount.AccountName;
            paymentRequest.PayinBankAccountNo = bankAccount.AccountNumber;
            paymentRequest.PayinBankCode = bankAccount.BankCode;
            paymentRequest.PayinPromptPayId = bankAccount.PromptPayId;
            paymentRequest.PayinAccountType = bankAccount.AccountType;
            paymentRequest.PayinAccountLevel = bankAccount.AccountLevel;
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

        public async Task<MVPaymentResponse> AddPaymentRequestPayIn(string orgId, MPaymentRequest paymentRequest, MMerchant merchant)
        {
            repository!.SetCustomOrgId(orgId); //ตรงนี้เป็น orgId ของ Merchant
            _bankAccountRepo!.SetCustomOrgId("global");

            var r = new MVPaymentResponse()
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

            var (bnkAcct, lines) = await GetPayInBankAccount(paymentRequest);
            if (bnkAcct == null)
            {
                r.Status = "ERROR_NO_PAYIN_ACCOUNT_MATCH";
                r.Description = $"No pay-in bank account match!!!";

                return r;
            }

            var pmResponse = CreatePaymentResponse(paymentRequest, bnkAcct);
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

        private async Task<(MBankAccount?, List<string>)> GetPayInBankAccount(MPaymentRequest pr)
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
                return (bankAcct, lines);
            }

            var accountType = "UNKNOWN"; //Native - in the future
            if (pr.QrProvider == "PP")
            {
                accountType = "PromptPay";
                lines.Add($"Step02 - Get bank account type : accountType -> [{accountType}]");
            }

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

                //Here - Bank Account Status is "Active"
                if (bankAccount.AccountLevel == "Global")
                {
                    lines.Add($"Step04 - Use global bank account : Account -> [{bankCode} - {bankAccountName}] [bankAccountNo] [{promptPayId}]");
                    return (bankAccount, lines);
                }

                if (bankAccount.AccountLevel == "Selected")
                {
                    //ต้องดูว่า merchant นั้นได้ผูกกับ bank นี้ไว้หรือไม่
                    if (dict.ContainsKey(bankAccountId))
                    {
                        lines.Add($"Step05.1 - Use selected bank account : Account -> [{bankCode} - {bankAccountName}] [bankAccountNo] [{promptPayId}]");
                        return (bankAccount, lines);
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

        private MVPaymentResponse CreatePaymentResponse(MPaymentRequest pr, MBankAccount bnkAcct)
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

            if (qrResult == null)
            {
                mvResponse.Status = "INVALID_QR_PROVIDER";
                mvResponse.Description = $"Invalid QR provider [{pr.QrProvider}]";
                return mvResponse;
            }

            var pmr = new MPaymentResponse()
            {
                CreatedAt = pr.CreatedDate,
                ExpireAt = pr.ExpireDate,

                Id = pr.Id.ToString(),
                ReferenceId = pr.RefId,
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
