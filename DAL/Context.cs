using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestTask.DAL.Entities;

namespace TestTask.DAL
{
    public class Context : DbContext
    {
        public static string ConnectionString = string.Empty;

        public DbSet<ApartmentBlocksEntity> ApartmentBlocks { get; set; }

        public DbSet<TenantsEntity> Tenants { get; set; }

        public DbSet<AccommodationInfoEntity> AccommodationInfo { get; set; }

        public DbSet<TenToABView> View_TenToABV { get; set; }

        private string _createViewQuery = @"CREATE OR REPLACE VIEW View_TenToAb AS
                                    SELECT 	acinfo.""Id"", acinfo.""ABID"", ab.""City"", ab.""Street"", ab.""Number"",
		                                        acinfo.""TenantId"", ten.""FirstName"", ten.""LastName"", ten.""Age""
                                    FROM ""AccommodationInfo"" as acInfo
                                    INNER JOIN ""ApartmentBlocks"" as ab ON ab.""Id"" = acInfo.""ABID""
                                    INNER JOIN ""Tenants"" as ten ON ten.""Id"" = acInfo.""TenantId""";

        public Context() : base()
        {
            Database.Migrate();
            Database.ExecuteSqlRaw(_createViewQuery);       
        }

        public Context(DbContextOptions<Context> options) : base(options)
        {
            Database.Migrate();
            Database.ExecuteSqlRaw(_createViewQuery);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApartmentBlocksEntity>().HasKey(ab => ab.Id);
            modelBuilder.Entity<TenantsEntity>().HasKey(ab => ab.Id);
            modelBuilder.Entity<AccommodationInfoEntity>().HasKey(ab => ab.Id);
            modelBuilder.Entity<TenToABView>(tenView =>
            {
                tenView.HasNoKey();
                tenView.ToView("view_tentoab");
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(ConnectionString);
        }
    }
}
