using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Database.Seeders;
using Its.Onix.Api.Database;
using Its.Onix.Api.Services;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Authorizations;
using Its.Onix.Api.Authentications;
using Its.Onix.Api.AuditLogs;
using System.Threading.RateLimiting;
using StackExchange.Redis;
using Its.Onix.Api.Utils;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.ResponseCompression;
using Minio;
using Microsoft.AspNetCore.Identity;

namespace Its.Onix.Api
{
    [ExcludeFromCodeCoverage]
    class Program
    {
        public static void Main(string[] args)
        {
            var log = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            Log.Logger = log;


            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);


            var cfg = builder.Configuration;
            var connStr = $"Host={cfg["PostgreSQL:Host"]}; Database={cfg["PostgreSQL:Database"]}; Username={cfg["PostgreSQL:User"]}; Password={cfg["PostgreSQL:Password"]}";


            var redisHostStr = $"{cfg["Redis:Host"]}:{cfg["Redis:Port"]}"; 
            builder.Services.AddSingleton<IConnectionMultiplexer>(
                sp => ConnectionMultiplexer.Connect(redisHostStr));
            builder.Services.AddScoped<RedisHelper>();

            builder.Services.AddSingleton(sp =>
            {
                // ถ้าใช้ service account json
                var storageClient = StorageClient.Create(
                    GoogleCredential.FromFile(Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS"))
                );

                return storageClient;
            });

            builder.Services.AddSingleton(sp =>
            {
                var minIoEndpoint = Environment.GetEnvironmentVariable("MINIO_ENDPOINT");
                var minIoAccessKey = Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY"); //User
                var minIoSecretKey = Environment.GetEnvironmentVariable("MINIO_SECRET_KEY"); //Password

                if (string.IsNullOrWhiteSpace(minIoEndpoint))
                {
                    throw new InvalidOperationException("MINIO_ENDPOINT is not configured.");
                }

                var uri = new Uri(minIoEndpoint);
                var clientBuilder = new MinioClient()
                    .WithEndpoint(uri.Host, uri.Port)
                    .WithCredentials(
                        minIoAccessKey,
                        minIoSecretKey);

                if (uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                {
                    clientBuilder = clientBuilder.WithSSL(false);
                }

                return clientBuilder.Build();
            });


            builder.Services.AddSingleton(sp =>
            {
                return GoogleCredential.FromFile(Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS"))
                                    .CreateScoped("https://www.googleapis.com/auth/cloud-platform");
            });
            builder.Services.AddSingleton<IStorageUtils, StorageUtilsGCP>();
            builder.Services.AddSingleton<IStorageUtilsS3, StorageUtilsS3>();
            builder.Services.AddSingleton<IRedisHelper, RedisHelper>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(connStr, o => o.CommandTimeout(1200)));
            builder.Services.AddTransient<DataSeeder>();

            builder.Services.AddScoped<IDataContext, DataContext>();
            builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
            builder.Services.AddScoped<IOrganizationService, OrganizationService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ISystemVariableService, SystemVariableService>();
            builder.Services.AddScoped<IOrganizationUserService, OrganizationUserService>();
            builder.Services.AddScoped<IMasterRefService, MasterRefService>();
            builder.Services.AddScoped<ICycleService, CycleService>();
            builder.Services.AddScoped<IItemService, ItemService>();
            builder.Services.AddScoped<IItemImageService, ItemImageService>();
            builder.Services.AddScoped<IEntityService, EntityService>();
            builder.Services.AddScoped<IPricingPlanService, PricingPlanService>();
            builder.Services.AddScoped<IPricingPlanItemService, PricingPlanItemService>();
            builder.Services.AddScoped<IScanItemService, ScanItemService>();
            builder.Services.AddScoped<IScanItemActionService, ScanItemActionService>();
            builder.Services.AddScoped<IScanItemTemplateService, ScanItemTemplateService>();
            builder.Services.AddScoped<IScanItemFolderService, ScanItemFolderService>();
            builder.Services.AddScoped<IJobService, JobService>();

            var nativeIdpFlag = Environment.GetEnvironmentVariable("IS_NATIVE_IDP");
            if (nativeIdpFlag == "true")
            {
                builder.Services.AddScoped<IAuthService, AuthServiceNative>();
            }
            else
            {
                //ไม่ได้กำหนด ก็จะใช้ Keycloak แบบเดิมเพื่อให้ backword compatible
                builder.Services.AddScoped<IAuthService, AuthServiceKeycloak>();    
            }

            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IAuditLogService, AuditLogService>();
            builder.Services.AddScoped<IStatService, StatService>();
            builder.Services.AddScoped<IPointService, PointService>();
            builder.Services.AddScoped<ILimitService, LimitService>();
            builder.Services.AddScoped<IAdminUserService, AdminUserService>();
            builder.Services.AddScoped<IPointRuleService, PointRuleService>();
            builder.Services.AddScoped<IPointTriggerService, PointTriggerService>();
            builder.Services.AddScoped<IAccountDocService, AccountDocService>();
            builder.Services.AddScoped<IVoucherService, VoucherService>();
            builder.Services.AddScoped<ICustomRoleService, CustomRoleService>();
            builder.Services.AddScoped<IAgentService, AgentService>();
            builder.Services.AddScoped<IAgentPolicyService, AgentPolicyService>();
            builder.Services.AddScoped<IMerchantService, MerchantService>();
            builder.Services.AddScoped<IBankAccountService, BankAccountService>();
            builder.Services.AddScoped<IPaymentRequestService, PaymentRequestService>();
            builder.Services.AddScoped<IPaymentTransactionService, PaymentTransactionService>();
            builder.Services.AddScoped<ISummaryService, SummaryService>();
            builder.Services.AddScoped<IWebhookConfigService, WebhookConfigService>();
            builder.Services.AddScoped<IFileDocumentService, FileDocumentService>();
            builder.Services.AddScoped<IPaymentDocumentService, PaymentDocumentService>();
            builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
            builder.Services.AddScoped<INotiChannelService, NotiChannelService>();
            builder.Services.AddScoped<IFinancialDocService, FinancialDocService>();
            builder.Services.AddScoped<ICaseManagementService, CaseManagementService>();


            builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            builder.Services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ISystemVariableRepository, SystemVariableRepository>();
            builder.Services.AddScoped<IOrganizationUserRepository, OrganizationUserRepository>();
            builder.Services.AddScoped<IMasterRefRepository, MasterRefRepository>();
            builder.Services.AddScoped<ICycleRepository, CycleRepository>();
            builder.Services.AddScoped<IItemRepository, ItemRepository>();
            builder.Services.AddScoped<IItemImageRepository, ItemImageRepository>();
            builder.Services.AddScoped<IEntityRepository, EntityRepository>();
            builder.Services.AddScoped<IPricingPlanRepository, PricingPlanRepository>();
            builder.Services.AddScoped<IPricingPlanItemRepository, PricingPlanItemRepository>();
            builder.Services.AddScoped<IScanItemRepository, ScanItemRepository>();
            builder.Services.AddScoped<IScanItemActionRepository, ScanItemActionRepository>();
            builder.Services.AddScoped<IScanItemTemplateRepository, ScanItemTemplateRepository>();
            builder.Services.AddScoped<IScanItemFolderRepository, ScanItemFolderRepository>();
            builder.Services.AddScoped<IJobRepository, JobRepository>();
            builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            builder.Services.AddScoped<IStatRepository, StatRepository>();
            builder.Services.AddScoped<IPointRepository, PointRepository>();
            builder.Services.AddScoped<ILimitRepository, LimitRepository>();
            builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
            builder.Services.AddScoped<IPointRuleRepository, PointRuleRepository>();
            builder.Services.AddScoped<IPointTriggerRepository, PointTriggerRepository>();
            builder.Services.AddScoped<IAccountDocRepository, AccountDocRepository>();
            builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();
            builder.Services.AddScoped<ICustomRoleRepository, CustomRoleRepository>();
            builder.Services.AddScoped<IAgentRepository, AgentRepository>();
            builder.Services.AddScoped<IAgentPolicyRepository, AgentPolicyRepository>();
            builder.Services.AddScoped<IMerchantRepository, MerchantRepository>();
            builder.Services.AddTransient<IAuthorizationHandler, GenericRbacHandler>();
            builder.Services.AddScoped<IBasicAuthenticationRepo, BasicAuthenticationRepo>();
            builder.Services.AddScoped<IBearerAuthenticationAdminRepo, BearerAuthenticationAdminRepo>();
            builder.Services.AddScoped<IBearerAuthenticationCustomerRepo, BearerAuthenticationCustomerRepo>();
            builder.Services.AddScoped<IBearerAuthenticationRepo, BearerAuthenticationRepo>();
            builder.Services.AddScoped<IBankAccountRepository, BankAccountRepository>();
            builder.Services.AddScoped<IPaymentRequestRepository, PaymentRequestRepository>();
            builder.Services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
            builder.Services.AddScoped<ISummaryRepository, SummaryRepository>();
            builder.Services.AddScoped<IWebhookConfigRepository, WebhookConfigRepository>();
            builder.Services.AddScoped<IFileDocumentRepository, FileDocumentRepository>();
            builder.Services.AddScoped<IPaymentDocumentRepository, PaymentDocumentRepository>();
            builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
            builder.Services.AddScoped<INotiChannelRepository, NotiChannelRepository>();
            builder.Services.AddScoped<IFinancialDocRepository, FinancialDocRepository>();
            builder.Services.AddScoped<ICaseManagementRepository, CaseManagementRepository>();


            builder.Services.AddAuthentication("BasicOrBearer")
                .AddScheme<AuthenticationSchemeOptions, AuthenticationHandlerProxy>("BasicOrBearer", null);

            builder.Services.AddAuthorization(options => {
                var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder("BasicOrBearer");
                defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();

                options.AddPolicy("GenericRolePolicy", policy => policy.AddRequirements(new GenericRbacRequirement()));
            });

            //Begin rate limiter
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = 429;

                options.OnRejected = async (context, token) =>
                {
                    var clientIp = "unknown";
                    if (context.HttpContext.Request.Headers.TryGetValue("X-Original-Forwarded-For", out var xForwardedFor))
                    {
                        clientIp = xForwardedFor.ToString().Split(',')[0].Trim();
                    }

                    Log.Warning($"Rate limit triggered for IP: {clientIp} at {DateTime.UtcNow}");
                    await context.HttpContext.Response.WriteAsync("Too many requests. Try again later.", token);
                };
                
                // ทำ partitioned rate limiter per IP
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var clientIp = "unknown";
                    if (httpContext.Request.Headers.TryGetValue("X-Original-Forwarded-For", out var xForwardedFor))
                    {
                        clientIp = xForwardedFor.ToString().Split(',')[0].Trim();
                    }

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: clientIp,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 20, // อนุญาต 20 requests
                            Window = TimeSpan.FromSeconds(10), // ต่อ 10 วินาที
                            QueueLimit = 0,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        });
                });
            });
            //End rate limit

            // เปิด middleware สำหรับ gzip
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true; // ให้บีบอัดแม้เป็น HTTPS
                options.Providers.Add<GzipCompressionProvider>();
            });

            builder.Services.AddHttpClient();
            builder.Services.AddHealthChecks();
            builder.Services.AddSignalR();

            builder.Services
                .AddIdentityCore<IdentityUser>()
                .AddEntityFrameworkStores<DataContext>()
                .AddDefaultTokenProviders();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                dbContext.Database.Migrate();

                var service = scope.ServiceProvider.GetRequiredService<DataSeeder>();
                service.Seed();

                if (nativeIdpFlag == "true")
                {
                    service.MigrateUsers();
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRateLimiter();
            app.UseMiddleware<AuditLogMiddleware>();
            app.MapHealthChecks("/health");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHub<PaymentHub>("/realtime/payment-tx");
            app.Run();
        }
    }
}
