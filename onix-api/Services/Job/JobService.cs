using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Utils;
using System.Text.Json;

namespace Its.Onix.Api.Services
{
    public class JobService : BaseService, IJobService
    {
        private readonly IJobRepository? repository = null;
        private readonly IRedisHelper _redis;
        private readonly IUserRepository _userRepo;

        public JobService(IJobRepository repo, IRedisHelper redis, IUserRepository userRepo) : base()
        {
            repository = repo;
            _redis = redis;
            _userRepo = userRepo;
        }

        public MJob? GetJobById(string orgId, string jobId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetJobById(jobId);

            if (result != null)
            {
                if (string.IsNullOrEmpty(result.Configuration))
                {
                    result.Configuration = "[]";
                }
                
                var parameters = JsonSerializer.Deserialize<List<NameValue>>(result.Configuration!);

                result.Parameters = parameters!;
                result.Configuration = "";
            }

            return result;
        }

        public MVJob? DeleteJobById(string orgId, string jobId)
        {
            var r = new MVJob()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(jobId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Job ID [{jobId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = repository!.DeleteJobById(jobId);

            r.Job = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Job ID [{jobId}] not found for the organization [{orgId}]";
            }

            return r;
        }

        public MJob GetJobTemplate(string orgId, string jobType, string userName)
        {
            var email = "your-email@email-xxx.com";
            _userRepo.SetCustomOrgId(orgId);

            if (!string.IsNullOrEmpty(userName))
            {
                //หา email ของ user คนนั้นเพื่อใส่เป็นค่า default
                var user = _userRepo.GetUserByName(userName);
                if (user != null)
                {
                    email = user.UserEmail!;
                }
            }

            var parameters = new[]
            {
                new { Name = "EMAIL_NOTI_ADDRESS", Value = email },
                new { Name = "SCAN_ITEM_COUNT", Value = "100" },
            };

            var jobKey = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var job = new MJob()
            {
                Name = $"{jobType}-{jobKey}",
                Description = $"Job to generate Scan Items",
            };

            foreach (var p in parameters)
            {
                var o = new NameValue() { Name = p.Name, Value = p.Value };
                job.Parameters.Add(o);
            }

            return job;
        }

        private string? GetEmail(MJob job, string varName)
        {
            foreach (var parm in job.Parameters)
            {
                if (parm.Name == varName)
                {
                    return parm.Value;
                }
            }

            return null;
        }

        public MVJob? AddJob(string orgId, MJob job, bool triggerJob = true)
        {
            repository!.SetCustomOrgId(orgId);
            var r = new MVJob
            {
                Status = "OK",
                Description = "Success"
            };

            var email = GetEmail(job, "EMAIL_NOTI_ADDRESS");
            if (email != null)
            {
                var emailValidateResult = ValidationUtils.ValidateEmail(email);
                if (emailValidateResult.Status != "OK")
                {
                    r.Status = emailValidateResult.Status;
                    r.Description = emailValidateResult.Description;

                    return r;
                }
            }

            job.Configuration = JsonSerializer.Serialize(job.Parameters);
            var result = repository!.AddJob(job);
            
            //Comment ไว้เพราะมัน update กลับไปที่ DB อยู่
            //result.Configuration = "";

            r.Job = result;

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var stream = $"JobSubmitted:{environment}:{job.Type}";
            var message = JsonSerializer.Serialize(r.Job);

            if (triggerJob)
            {
                _ = _redis.PublishMessageAsync(stream!, message);
            }

            return r;
        }

        public IEnumerable<MJob> GetJobs(string orgId, VMJob param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetJobs(param);

            return result;
        }

        public int GetJobCount(string orgId, VMJob param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetJobCount(param);

            return result;
        }

        public MJob CreatePayOutSuccessJob(string orgId, string jobType, MPaymentTransaction pmt, MPaymentRequest pmr)
        {
            //์ยิง webhook และ notify เพือ่แจ้ง merchant ว่า transaction โอนออกไปแล้ว

            var bankCode = pmr?.PayinBankCode;
            var accountNo = pmr?.PayinBankAccountNo;
            var accountName = pmr?.PayinBankAccountName;
            var promptPayId = pmr?.PayinPromptPayId;

            if (pmr!.IsPayInBankAccountOverride)
            {
                bankCode = pmr?.PayinBankCodeOverride;
                accountNo = pmr?.PayinBankAccountNoOverride;
                accountName = pmr?.PayinBankAccountNameOverride;
                promptPayId = pmr?.PayinPromptPayIdOverride;
            }

            var job = new MJob()
            {
                Name = $"{Guid.NewGuid()}",
                Description = "JobService.CreatePayOutSuccessJob()",
                Type = jobType,
                Status = "Pending",
                Tags = jobType,

                Parameters =
                [
                    new NameValue { Name = "ORG_ID", Value = orgId },
                    new NameValue { Name = "PAYMENT_TYPE", Value = "PayOut" },
                    new NameValue { Name = "PMT_ID", Value = pmt?.Id.ToString() },
                    new NameValue { Name = "PMR_ID", Value = pmr?.Id.ToString() },
                    new NameValue { Name = "PMR_REF_ID1", Value = pmr?.RefId },
                    new NameValue { Name = "PMR_REF_ID2", Value = pmr?.RefId1 },
                    new NameValue { Name = "PMR_REF_ID3", Value = pmr?.RefId2 },

                    new NameValue { Name = "MERCHANT_ID", Value = pmr?.MerchantId },
                    new NameValue { Name = "MERCHANT_CODE", Value = pmr?.MerchantCode },
                    new NameValue { Name = "MERCHANT_NAME", Value = pmr?.MerchantName },

                    new NameValue { Name = "TX_AMOUNT", Value = pmt?.TxAmountDecimal.ToString() },
                    new NameValue { Name = "PAYOUT_REQUEST_AMOUNT", Value = pmr?.RequestedAmount.ToString() },
                    new NameValue { Name = "PAYOUT_FEE", Value = pmt?.PayoutFeeDecimal.ToString() },
                    new NameValue { Name = "PAYOUT_FEE_PCT", Value = ((decimal?) pmt?.PayOutFeePct).ToString() },

                    new NameValue { Name = "PAYOUT_BANK_CODE", Value = bankCode },
                    new NameValue { Name = "PAYOUT_BANK_ACCOUNT_NO", Value = accountNo },
                    new NameValue { Name = "PAYOUT_BANK_ACCOUNT_NAME", Value = accountName },
                    new NameValue { Name = "PAYOUT_PROMPTPAY_ID", Value = promptPayId },

                    new NameValue { Name = "PAYOUT_IS_PARTIAL", Value = pmt?.TxIsPeerToPeer.ToString() },
                ]
            };

            //ยังไม่ต้อง trigger job ให้ทำงานทันที เดี๋ยวรอให้สร้าง Payment Tx เสร็จแล้วค่อย trigger พร้อมกับส่ง jobId ไปด้วยจะได้ดู log การ process ได้ง่ายขึ้น
            var result = AddJob(orgId, job, false); 
            var newJob = result?.Job!;

            return newJob;
        }
    }
}
