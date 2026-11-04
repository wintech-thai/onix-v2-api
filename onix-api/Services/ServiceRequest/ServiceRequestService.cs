using System.Text.RegularExpressions;
using System.Text.Json;
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
        private static readonly Regex ShopCodeRegex = new("^[a-z0-9_-]{3,20}$");

        private readonly IServiceRequestRepository repository;
        private readonly IJobService jobService;
        private readonly IFileDocumentService fileDocumentService;
        private readonly IMerchantRepository merchantRepository;
        private readonly IUserRepository userRepository;
        private readonly IOrganizationService organizationService;
        private readonly IOrganizationUserService organizationUserService;
        private readonly IBankAccountService bankAccountService;

        public ServiceRequestService(IServiceRequestRepository repo, IJobService jobSvc, IFileDocumentService fileDocumentSvc, IMerchantRepository merchantRepo, IUserRepository userRepo,
            IOrganizationService orgSvc, IOrganizationUserService orgUserSvc, IBankAccountService bankAccountSvc) : base()
        {
            repository = repo;
            jobService = jobSvc;
            fileDocumentService = fileDocumentSvc;
            merchantRepository = merchantRepo;
            userRepository = userRepo;
            organizationService = orgSvc;
            organizationUserService = orgUserSvc;
            bankAccountService = bankAccountSvc;
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

            // x089 Bug 7 : เช็คซ้ำกับ MUser.cs (user ที่ approve ไปแล้ว) ตั้งแต่ตอนสร้าง link เลย ไม่ต้องรอถึงตอน confirm
            if (userRepository.IsEmailExist(email))
            {
                r.Status = "EMAIL_ALREADY_EXISTS";
                r.Description = "This email is already registered";
                return r;
            }

            // Approve ยังไม่มี MUser เลยไม่พอ ต้องกันชนกับ ServiceRequest แถวอื่นที่ยัง Pending/Confirmed ด้วย (เหมือน Bug 8.a ของ shop code)
            if (await repository.IsEmailExist(email))
            {
                r.Status = "EMAIL_REQUEST_ALREADY_EXISTS";
                r.Description = "A registration request for this email is already pending";
                return r;
            }

            var adminDomain = NormalizeAdminDomain(request.AdminDomain);
            if (string.IsNullOrEmpty(adminDomain))
            {
                r.Status = "ADMIN_DOMAIN_MISSING";
                r.Description = "AdminDomain is required";
                return r;
            }

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
            jobService.CreateMerchantRegistrationEventJob(ORG_ID, saved, "create");

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
                return new MVServiceRequest { Status = "SHOP_CODE_INVALID", Description = "Shop code must be 3-20 characters, lowercase English letters/digits/underscore/hyphen only" };
            }

            // x089 ปรับแก้ 8.a : Approve ยังไม่มี เลยต้องกันชนกับ ServiceRequest แถวอื่นด้วย ไม่ใช่แค่ MMerchant
            if (await merchantRepository.IsMerchantCodeExist(shopCode) || await repository.IsShopCodeExist(shopCode, existing!.Id!.Value))
            {
                return new MVServiceRequest { Status = "SHOP_CODE_ALREADY_EXISTS", Description = "This shop code is already in use" };
            }

            var username = request.Username?.Trim() ?? "";
            var usernameValidate = ValidationUtils.ValidateUserName(username);
            if (usernameValidate.Status != "OK")
            {
                return new MVServiceRequest { Status = usernameValidate.Status, Description = usernameValidate.Description };
            }

            // x089 ปรับแก้ 8.b : ต้องเป็น username และ email ที่ไม่มีในระบบ (MUser.cs) มาก่อนเลย
            if (userRepository.IsUserNameExist(username))
            {
                return new MVServiceRequest { Status = "USERNAME_ALREADY_EXISTS", Description = "This username is already registered" };
            }

            if (userRepository.IsEmailExist(existing!.Email!))
            {
                return new MVServiceRequest { Status = "EMAIL_ALREADY_EXISTS", Description = "This email is already registered" };
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
            jobService.CreateMerchantRegistrationEventJob(ORG_ID, updated!, "update");
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

            // x090 2.d : Approve/Reject แล้วแก้ไขอะไรไม่ได้ทั้งนั้น ต้องกันที่ระดับ API ไม่ใช่แค่ UI
            var existingCheck = await repository.GetServiceRequestById(serviceRequestId);
            if (existingCheck == null)
            {
                return new MVServiceRequest { Status = "NOTFOUND", Description = $"Service request ID [{serviceRequestId}] not found" };
            }
            if (existingCheck.Status == "Approved" || existingCheck.Status == "Rejected")
            {
                return new MVServiceRequest { Status = "NOT_EDITABLE", Description = $"Service request is already [{existingCheck.Status}] and can no longer be edited" };
            }

            var updated = await repository.UpdateServiceRequestById(serviceRequestId, request);
            if (updated == null)
            {
                return new MVServiceRequest { Status = "NOTFOUND", Description = $"Service request ID [{serviceRequestId}] not found" };
            }

            jobService.CreateMerchantRegistrationEventJob(ORG_ID, updated, "update");
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

            if (existing.Status == "Approved" || existing.Status == "Rejected")
            {
                return new MVServiceRequest { Status = "APPROVED_CANNOT_DELETE", Description = $"Service request is already [{existing.Status}] and cannot be deleted" };
            }

            var deleted = await repository.DeleteServiceRequestById(serviceRequestId);
            return new MVServiceRequest { Status = "OK", Description = "Success", ServiceRequest = deleted };
        }

        public async Task<MVServiceRequest> RegenerateRegistrationLink(string serviceRequestId, string? adminDomain = null)
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

            // x089 ปรับแก้ : regenerate จาก browser ปัจจุบันแล้ว ให้ domain อัปเดตตามด้วย ไม่งั้นแถวที่สร้างจาก localhost จะติด localhost ตลอดไป
            var newDomain = NormalizeAdminDomain(adminDomain) ?? existing.AdminDomain;
            var newOtp = ServiceUtils.GenerateSecureRandomString(32);
            var newExpiredDate = DateTime.UtcNow.AddDays(1);
            var updated = await repository.RegenerateOtp(serviceRequestId, newOtp, newExpiredDate, newDomain);

            var link = $"{newDomain}/customer-signup/global/{existing.Id}/{newOtp}";
            CreateRegistrationLinkEmailJob(updated!, link, "customer-registration-link-regenerate");

            return new MVServiceRequest { Status = "OK", Description = "Success", ServiceRequest = updated, RegistrationLink = link };
        }

        private static string? NormalizeAdminDomain(string? adminDomain)
        {
            var domain = adminDomain?.Trim();
            if (string.IsNullOrEmpty(domain)) return null;
            if (!domain.StartsWith("http://") && !domain.StartsWith("https://")) domain = $"https://{domain}";
            return domain.TrimEnd('/');
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

        public async Task<MVServiceRequest> RejectServiceRequest(string serviceRequestId, string? reason)
        {
            if (!ServiceUtils.IsGuidValid(serviceRequestId))
            {
                return new MVServiceRequest { Status = "UUID_INVALID", Description = $"Service request ID [{serviceRequestId}] format is invalid" };
            }

            var trimmedReason = reason?.Trim();
            if (string.IsNullOrEmpty(trimmedReason))
            {
                return new MVServiceRequest { Status = "REASON_REQUIRED", Description = "Rejection reason is required" };
            }

            var existing = await repository.GetServiceRequestById(serviceRequestId);
            if (existing == null)
            {
                return new MVServiceRequest { Status = "NOTFOUND", Description = $"Service request ID [{serviceRequestId}] not found" };
            }

            if (existing.Status == "Approved" || existing.Status == "Rejected")
            {
                return new MVServiceRequest { Status = "NOT_EDITABLE", Description = $"Service request is already [{existing.Status}] and cannot be rejected again" };
            }

            existing.Status = "Rejected";
            existing.RejectReason = trimmedReason;

            var updated = await repository.UpdateServiceRequestById(serviceRequestId, existing);

            CreateRejectionEmailJob(updated!, trimmedReason);
            jobService.CreateMerchantRegistrationEventJob(ORG_ID, updated!, "reject", trimmedReason);

            return new MVServiceRequest { Status = "OK", Description = "Success", ServiceRequest = updated };
        }

        public async Task<MVServiceRequest> ApproveServiceRequest(string serviceRequestId, VMApproveServiceRequest payload)
        {
            if (!ServiceUtils.IsGuidValid(serviceRequestId))
            {
                return new MVServiceRequest { Status = "UUID_INVALID", Description = $"Service request ID [{serviceRequestId}] format is invalid" };
            }

            if (payload?.Organization?.Merchant == null)
            {
                return new MVServiceRequest { Status = "MERCHANT_INFO_REQUIRED", Description = "Merchant info is required" };
            }
            if (payload.BankAccount == null)
            {
                return new MVServiceRequest { Status = "BANK_ACCOUNT_INFO_REQUIRED", Description = "Bank account info is required" };
            }

            var existing = await repository.GetServiceRequestById(serviceRequestId);
            if (existing == null)
            {
                return new MVServiceRequest { Status = "NOTFOUND", Description = $"Service request ID [{serviceRequestId}] not found" };
            }

            if (existing.Status == "Approved" || existing.Status == "Rejected")
            {
                return new MVServiceRequest { Status = "NOT_EDITABLE", Description = $"Service request is already [{existing.Status}] and cannot be approved again" };
            }

            if (string.IsNullOrEmpty(existing.ShopCode) || string.IsNullOrEmpty(existing.Username))
            {
                return new MVServiceRequest { Status = "NOT_CONFIRMED", Description = "Service request must be Confirmed by the customer (shop code / username) before it can be approved" };
            }

            // ล็อก OrgCustomId/Merchant.Code ให้ตรงกับ shop code ที่ลูกค้ากรอกตอน confirm เสมอ ไม่เชื่อค่าที่ส่งมาจาก wizard
            var org = payload.Organization;
            org.OrgCustomId = existing.ShopCode;
            org.OrgType = "PLEASE-PAYMENT";
            org.Status = "Active";
            org.Merchant!.Code = existing.ShopCode;

            var orgResult = organizationService.AddOrganization(existing.ShopCode, org);
            if (orgResult.Status != "OK")
            {
                return new MVServiceRequest { Status = orgResult.Status, Description = $"Failed to create merchant: {orgResult.Description}" };
            }

            var bankAccount = payload.BankAccount;
            bankAccount.BookBankFileDocumentId = existing.BookBankFileDocumentId; // x090 4.b : copy book-bank photo from the service request over to the new bank account
            var bankAccountResult = await bankAccountService.AddBankAccount("global", bankAccount);
            if (bankAccountResult.Status != "OK")
            {
                return new MVServiceRequest { Status = bankAccountResult.Status, Description = $"Merchant was created, but failed to create bank account: {bankAccountResult.Description}" };
            }

            var bankAccountId = bankAccountResult.BankAccount!.Id.ToString()!;
            var linkResult = await bankAccountService.SelectMerchant("global", bankAccountId, existing.ShopCode);
            if (linkResult?.Status != "OK")
            {
                return new MVServiceRequest { Status = linkResult?.Status ?? "LINK_FAILED", Description = $"Merchant and bank account were created, but failed to link them: {linkResult?.Description}" };
            }

            var ou = new MOrganizationUser
            {
                UserName = existing.Username,
                TmpUserEmail = existing.Email,
                InvitedBy = "ServiceRequestApprove",
                InvitedByAdmin = true,
                Roles = ["OWNER"],
            };
            var inviteResult = organizationUserService.InviteUserWithLink(existing.ShopCode, ou);
            if (inviteResult?.Status != "OK")
            {
                return new MVServiceRequest { Status = inviteResult?.Status ?? "INVITE_FAILED", Description = $"Merchant and bank account were created and linked, but failed to invite user: {inviteResult?.Description}" };
            }

            // InviteUserWithLink คืน URL แบบ placeholder "<REGISTER_SERVICE_DOMAIN>" เสมอ ต้องแทนที่ด้วย merchant domain จริงก่อนส่งอีเมลให้ลูกค้า
            var signupLink = inviteResult.RegistrationUrl!.Replace("https://<REGISTER_SERVICE_DOMAIN>", DeriveMerchantDomain(existing.AdminDomain));

            existing.Status = "Approved";
            existing.ApprovedDate = DateTime.UtcNow;
            existing.MerchantSetupDataJson = JsonSerializer.Serialize(org);
            existing.BankAccountSetupDataJson = JsonSerializer.Serialize(bankAccount);

            var updated = await repository.UpdateServiceRequestById(serviceRequestId, existing);

            CreateApprovalEmailJob(updated!, signupLink);
            jobService.CreateMerchantRegistrationEventJob(ORG_ID, updated!, "approve");

            return new MVServiceRequest { Status = "OK", Description = "Success", ServiceRequest = updated, RegistrationLink = signupLink };
        }

        // admin domain กับ merchant domain เป็น subdomain คู่กันเสมอ (เช่น admin-dev.x / merchant-dev.x) ตามรูปแบบเดียวกับ getMerchantBase() ฝั่ง frontend
        private static string DeriveMerchantDomain(string? adminDomain)
        {
            if (string.IsNullOrWhiteSpace(adminDomain)) return "<REGISTER_SERVICE_DOMAIN>";
            return adminDomain.Contains("admin", StringComparison.OrdinalIgnoreCase)
                ? adminDomain.Replace("admin", "merchant", StringComparison.OrdinalIgnoreCase)
                : adminDomain;
        }

        // ส่ง event ไปที่ Redis Stream ให้ Ruby job ส่งอีเมลแจ้งผลอนุมัติ พร้อม link สำหรับ signup
        private void CreateApprovalEmailJob(MServiceRequest sr, string signupLink)
        {
            var job = new MJob
            {
                Name = $"{Guid.NewGuid()}",
                Description = "ServiceRequestService.CreateApprovalEmailJob()",
                Type = "PleasePaymentEmailSend",
                Status = "Pending",
                Tags = "service-request-approved",
                RefId = sr.Id.ToString(),

                Parameters =
                [
                    new NameValue { Name = "EMAIL_OTP_ADDRESS", Value = sr.Email },
                    new NameValue { Name = "FIRST_NAME", Value = sr.FirstName },
                    new NameValue { Name = "LAST_NAME", Value = sr.LastName },
                    new NameValue { Name = "SIGNUP_LINK", Value = signupLink },
                    new NameValue { Name = "LANG", Value = sr.Lang },
                ]
            };

            jobService.AddJob(ORG_ID, job);
        }

        // ส่ง event ไปที่ Redis Stream ให้ Ruby job ส่งอีเมลแจ้งผล reject พร้อมเหตุผล
        private void CreateRejectionEmailJob(MServiceRequest sr, string reason)
        {
            var job = new MJob
            {
                Name = $"{Guid.NewGuid()}",
                Description = "ServiceRequestService.CreateRejectionEmailJob()",
                Type = "PleasePaymentEmailSend",
                Status = "Pending",
                Tags = "service-request-rejected",
                RefId = sr.Id.ToString(),

                Parameters =
                [
                    new NameValue { Name = "EMAIL_OTP_ADDRESS", Value = sr.Email },
                    new NameValue { Name = "FIRST_NAME", Value = sr.FirstName },
                    new NameValue { Name = "LAST_NAME", Value = sr.LastName },
                    new NameValue { Name = "REASON", Value = reason },
                    new NameValue { Name = "LANG", Value = sr.Lang },
                ]
            };

            jobService.AddJob(ORG_ID, job);
        }
    }
}
