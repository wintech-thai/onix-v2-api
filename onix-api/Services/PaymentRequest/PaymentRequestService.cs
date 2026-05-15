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

            result.ResponseDataObj = JsonSerializer.Deserialize<MPaymentResponse>(result.ResponseData!);
            result.ResponseData = "";

            r.PaymentRequest = result;

            return r;
        }

        public async Task<MVPaymentResponse> AddPaymentRequestPayIn(string orgId, MPaymentRequest paymentRequest)
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

            var bnkAcct = await GetPayInBankAccount(paymentRequest);
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

            //Logic สำหรับการสร้าง QR payment ตรงนี้
            paymentRequest.ResponseData = "This should not be seen data";
            paymentRequest.Status = "Pending";
            paymentRequest.Direction = "PayIn";
            paymentRequest.PayinBankAccountName = bnkAcct.AccountName;
            paymentRequest.PayinBankAccountNo = bnkAcct.AccountNumber;
            paymentRequest.PayinBankCode = bnkAcct.BankCode;

            _ = await repository!.AddPaymentRequest(paymentRequest);

            return pmResponse;
        }

        private async Task<MBankAccount?> GetPayInBankAccount(MPaymentRequest pr)
        {
            //1. เลือก list ของ bank account ที่ตรงกับ QrProvider ขึ้นมา
            //2. แต่ละ bank account ให้ดูว่าเกิน condition ที่ตั้งไว้มั้ยเช่น 
            //   2.1 ยอดรวมต่อวัน
            //   2.2 เป็น bank account ของ merchant นั้นหรือไม่ ดูจากว่าเป็น global หรือ selected
            //   2.3 bank account นั้น active อยู่หรือไม่ 
            //3. เลือกตัวแรกที่เงื่อนไขผ่าน

            var accountType = "UNKNOWN";
            if (pr.QrProvider == "PP")
            {
                accountType = "PromptPay";
            }

            var param = new VMBankAccount()
            {
                AccountType = accountType,
                AccountCategory = "PayIn",
                AccountLevel = "", //เอามาทั้ง global และ selected แล้วค่อยมาเลือกอีกที
            };
Console.WriteLine($"DEBUG 1 [{accountType}]"); 
            var banks = await _bankAccountRepo!.GetAllBankAccounts(param); //ไม่มีเรื่องการทำ paging ตรงนี้ ถ้ามี bank account เยอะค่อยว่ากันในอนาคต
            foreach (var bank in banks)
            {
                //TODO : เพิ่มเงื่อนไขอื่น ๆ อีกสำรับ check
Console.WriteLine($"DEBUG 2.0 - [{bank.Status}], [{bank.AccountName}], [{bank.AccountNumber}], [{bank.BankCode}]");
                if (bank.Status == "Active")
                {
Console.WriteLine($"DEBUG 2.1 - [{bank.Status}], [{bank.AccountName}], [{bank.AccountNumber}], [{bank.BankCode}]");

                    return bank; 
                }
            }
Console.WriteLine($"DEBUG 3");
            //ไม่มี bank account ที่ match
            return null;
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

                PayInBankAccountName = bnkAcct.AccountName,
                PayInBankAccountNo = bnkAcct.AccountNumber,
                PayInBankCode = bnkAcct.BankCode
            };

            mvResponse.PaymentResponse = pmr;

            return mvResponse;
        }

        public async Task<List<MPaymentRequest>> GetPaymentRequests(string orgId, VMPaymentRequest param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPaymentRequests(param);

            // ลบ ResponseData ออกเพื่อลด payload
            result.ForEach(p => p.ResponseData = "");

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
