using System.Text.RegularExpressions;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    public class ServiceRequestService : BaseService, IServiceRequestService
    {
        private const string ORG_ID = "global"; // service request ยังไม่ผูกกับ org ใด ๆ จนกว่าจะ approve (ยังไม่ทำรอบนี้)
        private static readonly Regex ShopCodeRegex = new("^[A-Za-z0-9_-]{3,30}$");

        private readonly IServiceRequestRepository repository;
        private readonly IJobService jobService;
        private readonly IFileDocumentService fileDocumentService;

        public ServiceRequestService(IServiceRequestRepository repo, IJobService jobSvc, IFileDocumentService fileDocumentSvc) : base()
        {
            repository = repo;
            jobService = jobSvc;
            fileDocumentService = fileDocumentSvc;
            repository.SetCustomOrgId(ORG_ID);
        }

        public async Task<MVServiceRequest> CreatePleasePaymentRegistrationLink(MServiceRequest request)
        {
            var r = new MVServiceRequest { Status = "OK", Description = "Success" };

            var email = request.Email?.Trim();
            if (string.IsNullOrEmpty(email))
            {
                r.Status = "EMAIL_MISSING";
                r.Description = "Email is required";
                return r;
            }

            var emailValidate = ValidationUtils.ValidateEmail(email);
            if (emailValidate.Status != "OK")
            {
                r.Status = emailValidate.Status;
                r.Description = emailValidate.Description;
                return r;
            }

            var adminDomain = request.AdminDomain?.Trim();
            if (string.IsNullOrEmpty(adminDomain))
            {
                r.Status = "ADMIN_DOMAIN_MISSING";
                r.Description = "AdminDomain is required";
                return r;
            }
            if (!adminDomain.StartsWith("http://") && !adminDomain.StartsWith("https://"))
            {
                adminDomain = $"https://{adminDomain}";
            }
            adminDomain = adminDomain.TrimEnd('/');

            var lang = (request.Lang ?? "en").Trim().ToLowerInvariant();
            if (lang != "th" && lang != "en") lang = "en";

            var toSave = new MServiceRequest
            {
                ServiceType = "CustomerRegistration",
                Status = "Pending",
                Otp = ServiceUtils.GenerateSecureRandomString(32),
                ExpiredDate = DateTime.UtcNow.AddDays(1),
                Lang = lang,
                AdminDomain = adminDomain,
                Email = email,
                FirstName = request.FirstName?.Trim(),
                LastName = request.LastName?.Trim(),
                Phone = request.Phone?.Trim(),
            };

            var saved = await repository.AddServiceRequest(toSave);
            var link = $"{adminDomain}/customer-signup/global/{saved.Id}/{saved.Otp}";

            CreateRegistrationLinkEmailJob(saved, link);

            r.ServiceRequest = saved;
            r.RegistrationLink = link;
            return r;
        }

        // ส่ง event ไปที่ Redis Stream ให้ Ruby job ส่งอีเมล, RefId ชี้กลับ service request เพื่อดู job log
        private void CreateRegistrationLinkEmailJob(MServiceRequest sr, string link, string tags = "customer-registration-link")
        {
            var job = new MJob
            {
                Name = $"{Guid.NewGuid()}",
                Description = "ServiceRequestService.CreateRegistrationLinkEmailJob()",
                Type = "PleasePaymentEmailSend",
                Status = "Pending",
                Tags = tags,
                RefId = sr.Id.ToString(),

                Parameters =
                [
                    new NameValue { Name = "EMAIL_OTP_ADDRESS", Value = sr.Email },
                    new NameValue { Name = "FIRST_NAME", Value = sr.FirstName },
                    new NameValue { Name = "LAST_NAME", Value = sr.LastName },
                    new NameValue { Name = "REGISTRATION_LINK", Value = link },
                    new NameValue { Name = "LANG", Value = sr.Lang },
                    new NameValue { Name = "EXPIRE_DATE", Value = sr.ExpiredDate?.ToString("O") },
                ]
            };

            jobService.AddJob(ORG_ID, job);
        }

        private async Task<(MServiceRequest? existing, MVServiceRequest? error)> ValidateEditableByOtp(string requestId, string otp)
        {
            if (!ServiceUtils.IsGuidValid(requestId))
            {
                return (null, new MVServiceRequest { Status = "UUID_INVALID", Description = $"Service request ID [{requestId}] format is invalid" });
            }

            var existing = await repository.GetServiceRequestByIdAndOtp(requestId, otp);
            if (existing == null)
            {
                return (null, new MVServiceRequest { Status = "NOTFOUND_OR_OTP_INVALID", Description = "Service request not found, or OTP does not match" });
            }

            if (existing.Status != "Pending" && existing.Status != "Confirmed")
            {
                return (null, new MVServiceRequest { Status = "NOT_EDITABLE", Description = $"Service request is already [{existing.Status}] and can no longer be edited" });
            }

            if (existing.ExpiredDate.HasValue && existing.ExpiredDate.Value < DateTime.UtcNow)
            {
                return (null, new MVServiceRequest { Status = "EXPIRED", Description = "Service request link has expired" });
            }

            return (existing, null);
        }

        public async Task<MVServiceRequest> GetServiceRequestByIdAndOtp(string requestId, string otp)
        {
            if (!ServiceUtils.IsGuidValid(requestId))
            {
                return new MVServiceRequest { Status = "UUID_INVALID", Description = $"Service request ID [{requestId}] format is invalid" };
            }

            var existing = await repository.GetServiceRequestByIdAndOtp(requestId, otp);
            if (existing == null)
            {
                return new MVServiceRequest { Status = "NOTFOUND_OR_OTP_INVALID", Description = "Service request not found, or OTP does not match" };
            }

            // ลิงค์ที่ Approved/Rejected ไปแล้วดูได้เสมอ ไม่เช็คหมดอายุซ้ำ — หมดอายุมีผลเฉพาะตอนยังแก้ไขอยู่
            var stillEditable = existing.Status == "Pending" || existing.Status == "Confirmed";
            if (stillEditable && existing.ExpiredDate.HasValue && existing.ExpiredDate.Value < DateTime.UtcNow)
            {
                return new MVServiceRequest { Status = "EXPIRED", Description = "Service request link has expired" };
            }

            return new MVServiceRequest { Status = "OK", Description = "Success", ServiceRequest = existing };
        }

        public async Task<MVServiceRequest> ConfirmPleasePaymentServiceRequest(string requestId, string otp, MServiceRequest request)
        {
            var (existing, error) = await ValidateEditableByOtp(requestId, otp);
            if (error != null) return error;

            var shopCode = request.ShopCode?.Trim();
            if (string.IsNullOrEmpty(shopCode) || !ShopCodeRegex.IsMatch(shopCode))
            {
                return new MVServiceRequest { Status = "SHOP_CODE_INVALID", Description = "Shop code must be 3-30 characters, English letters/digits/underscore/hyphen only" };
            }

            var username = request.Username?.Trim() ?? "";
            var usernameValidate = ValidationUtils.ValidateUserName(username);
            if (usernameValidate.Status != "OK")
            {
                return new MVServiceRequest { Status = usernameValidate.Status, Description = usernameValidate.Description };
            }

            if (string.IsNullOrEmpty(request.BankCode) || string.IsNullOrEmpty(request.BankAccountNumber))
            {
                return new MVServiceRequest { Status = "BANK_ACCOUNT_REQUIRED", Description = "Bank code and account number are required" };
            }

            existing!.Status = "Confirmed";
            existing.FirstName = request.FirstName?.Trim();
            existing.LastName = request.LastName?.Trim();
            existing.Phone = request.Phone?.Trim();
            existing.BankCode = request.BankCode?.Trim();
            existing.BankAccountNumber = request.BankAccountNumber?.Trim();
            existing.BankAccountName = request.BankAccountName?.Trim();
            existing.PromptPayId = request.PromptPayId?.Trim();
            existing.ShopCode = shopCode;
            existing.Username = username;

            var updated = await repository.UpdateServiceRequestById(requestId, existing);
            return new MVServiceRequest { Status = "OK", Description = "Success", ServiceRequest = updated };
        }

        public async Task<MVServiceRequest> UploadServiceRequestBookBank(string requestId, string otp, MFileDocument fileDocument)
        {
            var (existing, error) = await ValidateEditableByOtp(requestId, otp);
            if (error != null) return error;

            fileDocument.DocumentType = "BookBank";
            fileDocument.IsPublic = false;

            var fileResult = await fileDocumentService.AddFileDocument(ORG_ID, fileDocument);
            if (fileResult.Status != "OK")
            {
                return new MVServiceRequest { Status = fileResult.Status, Description = fileResult.Description };
            }

            existing!.BookBankFileDocumentId = fileResult.FileDocument!.Id.ToString();
            var updated = await repository.UpdateServiceRequestById(requestId, existing);
            return new MVServiceRequest { Status = "OK", Description = "Success", ServiceRequest = updated };
        }

        public async Task<MVServiceRequest> GetServiceRequestById(string serviceRequestId)
        {
            if (!ServiceUtils.IsGuidValid(serviceRequestId))
            {
                return new MVServiceRequest { Status = "UUID_INVALID", Description = $"Service request ID [{serviceRequestId}] format is invalid" };
            }

            var result = await repository.GetServiceRequestById(serviceRequestId);
            if (result == null)
            {
                return new MVServiceRequest { Status = "NOTFOUND", Description = $"Service request ID [{serviceRequestId}] not found" };
            }

            var r = new MVServiceRequest { Status = "OK", Description = "Success", ServiceRequest = result };

            if (!string.IsNullOrEmpty(result.BookBankFileDocumentId))
            {
                var fileResult = await fileDocumentService.GetFileDocumentById(ORG_ID, result.BookBankFileDocumentId);
                if (fileResult.Status == "OK" && fileResult.FileDocument != null)
                {
                    r.BookBankImageBase64 = fileResult.FileDocument.FileContent;
                    r.BookBankMimeType = fileResult.FileDocument.MimeType;
                }
            }

            return r;
        }

        public async Task<IEnumerable<MServiceRequest>> GetServiceRequests(VMServiceRequest param)
        {
            return await repository.GetServiceRequests(param);
        }

        public async Task<int> GetServiceRequestCount(VMServiceRequest param)
        {
            return await repository.GetServiceRequestCount(param);
        }

        public async Task<MVServiceRequest> UpdateServiceRequestById(string serviceRequestId, MServiceRequest request)
        {
            if (!ServiceUtils.IsGuidValid(serviceRequestId))
            {
                return new MVServiceRequest { Status = "UUID_INVALID", Description = $"Service request ID [{serviceRequestId}] format is invalid" };
            }

            var updated = await repository.UpdateServiceRequestById(serviceRequestId, request);
            if (updated == null)
            {
                return new MVServiceRequest { Status = "NOTFOUND", Description = $"Service request ID [{serviceRequestId}] not found" };
            }

            return new MVServiceRequest { Status = "OK", Description = "Success", ServiceRequest = updated };
        }

        public async Task<MVServiceRequest> DeleteServiceRequestById(string serviceRequestId)
        {
            if (!ServiceUtils.IsGuidValid(serviceRequestId))
            {
                return new MVServiceRequest { Status = "UUID_INVALID", Description = $"Service request ID [{serviceRequestId}] format is invalid" };
            }

            var existing = await repository.GetServiceRequestById(serviceRequestId);
            if (existing == null)
            {
                return new MVServiceRequest { Status = "NOTFOUND", Description = $"Service request ID [{serviceRequestId}] not found" };
            }

            if (existing.Status == "Approved")
            {
                return new MVServiceRequest { Status = "APPROVED_CANNOT_DELETE", Description = "Service request is already Approved and cannot be deleted" };
            }

            var deleted = await repository.DeleteServiceRequestById(serviceRequestId);
            return new MVServiceRequest { Status = "OK", Description = "Success", ServiceRequest = deleted };
        }

        public async Task<MVServiceRequest> RegenerateRegistrationLink(string serviceRequestId)
        {
            if (!ServiceUtils.IsGuidValid(serviceRequestId))
            {
                return new MVServiceRequest { Status = "UUID_INVALID", Description = $"Service request ID [{serviceRequestId}] format is invalid" };
            }

            var existing = await repository.GetServiceRequestById(serviceRequestId);
            if (existing == null)
            {
                return new MVServiceRequest { Status = "NOTFOUND", Description = $"Service request ID [{serviceRequestId}] not found" };
            }

            if (existing.Status == "Approved" || existing.Status == "Rejected")
            {
                return new MVServiceRequest { Status = "NOT_EDITABLE", Description = $"Service request is already [{existing.Status}]" };
            }

            var newOtp = ServiceUtils.GenerateSecureRandomString(32);
            var newExpiredDate = DateTime.UtcNow.AddDays(1);
            var updated = await repository.RegenerateOtp(serviceRequestId, newOtp, newExpiredDate);

            var link = $"{existing.AdminDomain}/customer-signup/global/{existing.Id}/{newOtp}";
            CreateRegistrationLinkEmailJob(updated!, link, "customer-registration-link-regenerate");

            return new MVServiceRequest { Status = "OK", Description = "Success", ServiceRequest = updated, RegistrationLink = link };
        }

        public async Task<MVServiceRequest> ResendRegistrationLinkEmail(string serviceRequestId)
        {
            if (!ServiceUtils.IsGuidValid(serviceRequestId))
            {
                return new MVServiceRequest { Status = "UUID_INVALID", Description = $"Service request ID [{serviceRequestId}] format is invalid" };
            }

            var existing = await repository.GetServiceRequestById(serviceRequestId);
            if (existing == null)
            {
                return new MVServiceRequest { Status = "NOTFOUND", Description = $"Service request ID [{serviceRequestId}] not found" };
            }

            if (existing.Status == "Approved" || existing.Status == "Rejected")
            {
                return new MVServiceRequest { Status = "NOT_EDITABLE", Description = $"Service request is already [{existing.Status}]" };
            }

            if (existing.ExpiredDate.HasValue && existing.ExpiredDate.Value < DateTime.UtcNow)
            {
                return new MVServiceRequest { Status = "EXPIRED", Description = "Service request link has expired — use Regenerate Link instead" };
            }

            var link = $"{existing.AdminDomain}/customer-signup/global/{existing.Id}/{existing.Otp}";
            CreateRegistrationLinkEmailJob(existing, link, "customer-registration-link-resend");

            return new MVServiceRequest { Status = "OK", Description = "Success", ServiceRequest = existing, RegistrationLink = link };
        }
    }
}
