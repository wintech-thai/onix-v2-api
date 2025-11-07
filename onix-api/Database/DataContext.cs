namespace Its.Onix.Api.Database;

using Its.Onix.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class DataContext : DbContext, IDataContext
{
    protected readonly IConfiguration Configuration;

    public DataContext(IConfiguration configuration, DbContextOptions<DataContext> options) : base(options)
    {
        Configuration = configuration;
    }

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
    public DbSet<MLimit>? Limits { get; set; }

    //=== Admin tables here =====
    public DbSet<MAdminUser>? AdminUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Admin users tables here
        modelBuilder.Entity<MAdminUser>();

        //Org users tables here
        modelBuilder.Entity<MOrganization>();
        modelBuilder.Entity<MApiKey>();
        modelBuilder.Entity<MRole>();
        modelBuilder.Entity<MUser>();
        modelBuilder.Entity<MOrganizationUser>();
        modelBuilder.Entity<MSystemVariable>();
        modelBuilder.Entity<MItem>();
        modelBuilder.Entity<MItemImage>();
        modelBuilder.Entity<MScanItem>();
        modelBuilder.Entity<MScanItemTemplate>();
        modelBuilder.Entity<MScanItemAction>();
        modelBuilder.Entity<MJob>();

        modelBuilder.Entity<MScanItem>()
            .HasIndex(t => new { t.OrgId, t.Serial, t.Pin }).IsUnique();
        modelBuilder.Entity<MScanItem>()
            .HasIndex(t => new { t.OrgId, t.Serial }).IsUnique();

        modelBuilder.Entity<MMasterRef>()
            .HasIndex(t => new { t.OrgId, t.Code }).IsUnique();

        modelBuilder.Entity<MCycle>()
            .HasIndex(t => new { t.OrgId, t.Code }).IsUnique();

        modelBuilder.Entity<MItem>()
            .HasIndex(t => new { t.OrgId, t.Code }).IsUnique();

        modelBuilder.Entity<MItemImage>()
            .HasOne(ii => ii.Item)
            .WithMany(i => i.Images)
            .HasForeignKey(ii => ii.ItemId)
            .HasPrincipalKey(i => i.Id);

        modelBuilder.Entity<MEntity>()
            .HasIndex(t => new { t.OrgId, t.Code }).IsUnique();
        modelBuilder.Entity<MEntity>()
            .HasIndex(t => new { t.OrgId, t.PrimaryEmail }).IsUnique();

        modelBuilder.Entity<MPricingPlan>()
            .HasIndex(t => new { t.OrgId, t.Code }).IsUnique();

        modelBuilder.Entity<MPricingPlan>()
            .HasOne(pp => pp.Customer)
            .WithMany(cm => cm.PricingPlans)
            .HasForeignKey(pp => pp.CustomerId)
            .HasPrincipalKey(cm => cm.Id);

        modelBuilder.Entity<MPricingPlanItem>()
            .HasOne(pi => pi.PricingPlan)
            .WithMany(i => i.PricingPlanItems)
            .HasForeignKey(pi => pi.PricingPlanId)
            .HasPrincipalKey(i => i.Id);

        modelBuilder.Entity<MAuditLog>();

        modelBuilder.Entity<MStat>();
        modelBuilder.Entity<MStat>()
            .HasIndex(t => new { t.OrgId, t.StatCode, t.BalanceDateKey }).IsUnique();

        modelBuilder.Entity<MPointTx>();
        modelBuilder.Entity<MPointBalance>();
        modelBuilder.Entity<MWallet>();

        modelBuilder.Entity<MItemTx>();
        modelBuilder.Entity<MItemBalance>();

        modelBuilder.Entity<MLimit>();
        modelBuilder.Entity<MLimit>()
            .HasIndex(t => new { t.OrgId, t.StatCode }).IsUnique();
    }
}
