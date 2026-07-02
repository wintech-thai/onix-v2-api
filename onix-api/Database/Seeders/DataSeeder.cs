namespace Its.Onix.Api.Database.Seeders;

using Serilog;
using System.Diagnostics.CodeAnalysis;
using PasswordGenerator;
using Its.Onix.Api.Models;
using Microsoft.AspNetCore.Identity;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;

[ExcludeFromCodeCoverage]
public class DataSeeder
{
    private readonly DataContext context;
    private readonly Password pwd = new Password(32);
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IAdminUserRepository _adminUserRepo;
    private readonly IUserRepository _userRepo;

    public DataSeeder(
        DataContext ctx, 
        UserManager<IdentityUser> userManager,
        IAdminUserRepository adminUserRepo,
        IUserRepository userRepo)
    {
        context = ctx;
        _userManager = userManager;
        _userRepo = userRepo;
        _adminUserRepo = adminUserRepo;
    }

    private void SeedDefaultOrganization()
    {
        if (context == null)
        {
            return;
        }

        if (context.Organizations == null)
        {
            return;
        }

        if (!context.Organizations.Any())
        {
            var orgs = new List<MOrganization>()
            {
                new MOrganization
                {
                    OrgId = Guid.NewGuid(),
                    OrgName = "DEFAULT",
                    OrgDescription = "Default initial created organization",
                    OrgCreatedDate = DateTime.UtcNow,
                    OrgCustomId = "default"
                }
            };

            context.Organizations.AddRange(orgs);
            context.SaveChanges();
        }
    }

    private void SeedGlobalOrganization()
    {
        if (context == null)
        {
            return;
        }

        if (context.Organizations == null)
        {
            return;
        }

        string orgId = "global";

        var query = context.Organizations!.Where(x => x.OrgCustomId!.Equals(orgId)).FirstOrDefault();
        if (query == null)
        {
            //Create if not exist
            var orgs = new List<MOrganization>()
            {
                new MOrganization
                {
                    OrgId = Guid.NewGuid(),
                    OrgName = "GLOBAL",
                    OrgDescription = "Global/Root initial created organization",
                    OrgCreatedDate = DateTime.UtcNow,
                    OrgCustomId = orgId
                }
            };
            context.Organizations.AddRange(orgs);

            var apiKey = new MApiKey()
            {
                KeyId = Guid.NewGuid(),
                KeyCreatedDate = DateTime.UtcNow,
                OrgId = orgId,
                ApiKey = pwd.Next(),
                KeyDescription = "Auto created root key"
            };
            context.ApiKeys!.Add(apiKey);

            context.SaveChanges();
        }
    }

    private void UpdateDefaultOrganizationCustomId()
    {
        if (context == null)
        {
            return;
        }

        if (context.Organizations == null)
        {
            return;
        }

        var query = context.Organizations!.Where(x => x.OrgName!.Equals("DEFAULT")).FirstOrDefault();
        if (query == null)
        {
            Log.Error("Default organization 'DEFAULT' not found!!!");
            return;
        }
        query.OrgCustomId = "default";
        context.SaveChanges();
    }

    private void AddRole(string name, string definition, string level, string desc)
    {
        var query = context.Roles!.Where(x => x.RoleName!.Equals(name)).FirstOrDefault();
        if (query != null)
        {
            //Already exist
            return;
        }

        var r = new MRole() 
        {
            RoleName = name,
            RoleDefinition = definition,
            RoleLevel = level,
            RoleDescription = desc
        };

        context!.Roles!.Add(r);
    }

    private void UpdateScanItemTemplateDefaultIfNull()
    {
        var items = context.ScanItemTemplates!.Where(x => x.IsDefault == null || x.IsDefault == "").ToList();
        foreach (var item in items)
        {
            item.IsDefault = "YES";
        }

        context.SaveChanges();
    }

    private void SeedDefaultRoles()
    {
        AddRole("CREATOR", "Admin:CreateOrganization,ApiKey:AddApiKey", "ADMIN", "Organization creator");
        AddRole("OWNER", ".+:.+", "ORGANIZATION", "Organization Owner");
        AddRole("VIEWER", ".+:Get.+", "ORGANIZATION", "Organization Viewer");
        AddRole("EDITOR", ".+:Add.+,.+:Update.+,.+:Delete.+", "ORGANIZATION", "Organization Editor");
        AddRole("UPLOADER", "FileUpload:Upload.+Image", "ORGANIZATION", "Organization File Uploader");

        context.SaveChanges();
    }

    private void UpdateApiKeyRole()
    {
        var apiKeys = context.ApiKeys!.Where(x => x.RolesList!.Equals(null) || x.RolesList!.Equals("")).ToList();
        apiKeys.ForEach(a => a.RolesList = "OWNER");
        context.SaveChanges();
    }

    private void SeedDefaultRoles2()
    {
        AddRole("USER_ORGS_VIEWER", "Organization:AdminGetUserAllowedOrganization", "ADMIN", "Allow only for AdminGetUserAllowedOrganization");
        context.SaveChanges();
    }

    private void SeedDefaultRoles3()
    {
        AddRole("PAYMENT_REQUEST", "PaymentRequest:SubmitPayInRequest", "ORGANIZATION", "For merchant to submit payment request");
        AddRole("PAYOUT_REQUEST", "PaymentRequest:SubmitPayOutRequest", "ORGANIZATION", "For merchant to submit pay-out request");
        
        context.SaveChanges();
    }

    private void SeedDefaultRoles4()
    {
        AddRole("PAYMENT_TX_LINE", "AdminPaymentTx:SubmitLinePaymentTxNotification", "ADMIN", "For LINE agent to submit payment transaction notification");
        AddRole("AGENT_CONNECT", "AdminAgent:Notify.+", "ADMIN", "Agent notification");

        context.SaveChanges();
    }

    public void Seed()
    {
        SeedDefaultOrganization();
        UpdateDefaultOrganizationCustomId();

        SeedGlobalOrganization();
        SeedDefaultRoles();
        UpdateApiKeyRole();
        SeedDefaultRoles2();
        SeedDefaultRoles3();
        SeedDefaultRoles4();

        //UpdateScanItemTemplateDefaultIfNull();
    }

    public void MigrateUsers()
    {
        var users = context.Users!.ToList();
        foreach (var u in users)
        {
            //ใช้สำหรับ migrate user จาก KeycloakIDP มายัง NativeIDP
            Console.WriteLine($"MigrateUsers : Checking for user [{u.UserName}]...");
            var t1 = _userManager.FindByNameAsync(u.UserName!);

            var user = t1.Result;
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = u.UserName,
                    Email = u.UserEmail,
                };

                var initialPassword = ServiceUtils.GeneratePassword();
                var t2 = _userManager.CreateAsync(user, initialPassword);
                var result = t2.Result;

                Console.WriteLine($"MigrateUsers : Added [{u.UserName}] [{initialPassword}] [{result.Succeeded}] [{result.Errors.FirstOrDefault()?.Description}]");
            }
        }

        if (users.Count <= 0)
        {
            //ระบบตอน fresh installed เลยก็จะสร้าง initial admin ให้
            var u = new MUser()
            {
                UserName = "admin",
                UserEmail = "admin@abc.local",
                Name = "Admin",
                LastName = "Administrator",
                IsOrgInitialUser = "YES",
            };

            var addedUser = _userRepo.AddUser(u);

            var au = new MAdminUser()
            {
                UserId = addedUser.UserId.ToString(),
                UserName = u.UserName,
                UserEmail = u.UserEmail,
                TmpUserEmail = u.UserEmail,
                RolesList = "OWNER",
                UserStatus = "Active",
            };

            var t1 = _adminUserRepo.AddUser(au);
            var _ = t1.Result;

            var idpUser = new IdentityUser
            {
                UserName = u.UserName,
                Email = u.UserEmail,
            };

            var initialPassword = ServiceUtils.GeneratePassword();
            var t2 = _userManager.CreateAsync(idpUser, initialPassword);
            var result = t2.Result;

            Console.WriteLine($"##### MigrateUsers : Added [{u.UserName}] [{initialPassword}] [{result.Succeeded}]");
        }
    }
}