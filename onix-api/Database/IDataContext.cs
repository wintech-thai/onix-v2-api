using Microsoft.EntityFrameworkCore;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.Database
{
    public interface IDataContext : IDisposable
    {
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        public DbSet<MOrganization>? Organizations { get; set; }
        public DbSet<MApiKey>? ApiKeys { get; set; }
        public DbSet<MRole>? Roles { get; set; }
        public DbSet<MUser>? Users { get; set; }
        public DbSet<MOrganizationUser>? OrganizationUsers { get; set; }
        public DbSet<MSystemVariable>? SystemVariables { get; set; }
        public DbSet<MMasterRef>? MasterRefs { get; set; }
        public DbSet<MCycle>? Cycles { get; set; }
        public DbSet<MItem>? Items { get; set; }
        public DbSet<MItemImage>? ItemImages { get; set; }
        public DbSet<MEntity>? Entities { get; set; }
        public DbSet<MPricingPlan>? PricingPlans { get; set; }
        public DbSet<MPricingPlanItem>? PricingPlanItems { get; set; }
        public DbSet<MScanItem>? ScanItems { get; set; }
        public DbSet<MJob>? Jobs { get; set; }
        public DbSet<MScanItemTemplate>? ScanItemTemplates { get; set; }
        public DbSet<MScanItemAction>? ScanItemActions { get; set; }
        public DbSet<MAuditLog>? AuditLogs { get; set; }
        public DbSet<MStat>? Stats { get; set; }
        public DbSet<MPointTx>? PointTxs { get; set; }
        public DbSet<MPointBalance>? PointBalances { get; set; }
        public DbSet<MWallet>? Wallets { get; set; }
        public DbSet<MItemTx>? ItemTxs { get; set; }
        public DbSet<MItemBalance>? ItemBalances { get; set; }
    }
}