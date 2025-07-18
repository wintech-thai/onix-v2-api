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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MOrganization>();
        modelBuilder.Entity<MApiKey>();
        modelBuilder.Entity<MRole>();
        modelBuilder.Entity<MUser>();
        modelBuilder.Entity<MOrganizationUser>();
        modelBuilder.Entity<MSystemVariable>();
        modelBuilder.Entity<MItem>();
        modelBuilder.Entity<MItemImage>();


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
    }
}
