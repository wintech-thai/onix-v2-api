﻿using Microsoft.EntityFrameworkCore;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.Database
{
    public interface IDataContext : IDisposable
    {
        int SaveChanges();
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
    }
}