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
                return GoogleCredential.FromFile(Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS"))
                                    .CreateScoped("https://www.googleapis.com/auth/cloud-platform");
            });
            builder.Services.AddSingleton<IStorageUtils, StorageUtils>();

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
            builder.Services.AddScoped<IJobService, JobService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

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
            builder.Services.AddScoped<IJobRepository, JobRepository>();

            builder.Services.AddTransient<IAuthorizationHandler, GenericRbacHandler>();
            builder.Services.AddScoped<IBasicAuthenticationRepo, BasicAuthenticationRepo>();
            builder.Services.AddScoped<IBearerAuthenticationRepo, BearerAuthenticationRepo>();

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

            builder.Services.AddHttpClient();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                dbContext.Database.Migrate();

                var service = scope.ServiceProvider.GetRequiredService<DataSeeder>();
                service.Seed();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRateLimiter();
            app.UseMiddleware<AuditLogMiddleware>();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
