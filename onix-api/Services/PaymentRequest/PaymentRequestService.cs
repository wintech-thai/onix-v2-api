using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;
using System.Text.Json;
using System.Threading.Tasks;

namespace Its.Onix.Api.Services
{
    public class PaymentRequestService : BaseService, IPaymentRequestService
    {
        private readonly IPaymentRequestRepository? repository = null;
        private readonly IBankAccountRepository? _bankAccountRepo = null;

        public PaymentRequestService(IPaymentRequestRepository repo, IBankAccountRepository bankAcctRepo) : base()
        {
            repository = repo;
            _bankAccountRepo = bankAcctRepo;
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

            //TODO : Validate ว่า amount เกิน range ของ merchant มั้ย

            //TODO : implement logic สำหรับสร้างจุดทศนิยมตรงนี้
            paymentRequest.GeneratedAmount = paymentRequest.RequestedAmount;

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
            //   2.4 จำนวนเงินที่กรอก อยู่ใน range ที่ allow ของ bank account นั้น ๆ หรือไม่
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

            var accountType = "UNKNOWN";
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
                GeneratedAmount = pr.RequestedAmount, //ตรงนี้ต้อง random ทศนิยม
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
    }
}
