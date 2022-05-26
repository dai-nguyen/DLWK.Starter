using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointRewardModule.Data
{
    public class AppDbContext : AuditableDbContext
    {
        readonly ILoggerFactory _loggerFactory;
        readonly IUserSessionService _userSession;

        public DbSet<Entities.PointReward> PointRewards { get; set; }
        public DbSet<Entities.PointRewardSummary> PointRewardSummaries { get; set; }

        public AppDbContext(
            DbContextOptions options,
            ILoggerFactory loggerFactory,
            IUserSessionService userSession) 
            : base(options)
        {
        }

        public override async Task<int> SaveChangesAsync(
            string userId = "",
            CancellationToken cancellationToken = new())
        {
            foreach (var entry in ChangeTracker.Entries<IAuditableEntity>().ToArray())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.DateCreated = DateTime.UtcNow;
                        entry.Entity.CreatedBy = _userSession.UserId;
                        entry.Entity.DateUpdated = DateTime.UtcNow;
                        entry.Entity.UpdatedBy = _userSession.UserId;
                        break;

                    case EntityState.Modified:
                        entry.Entity.DateUpdated = DateTime.UtcNow;
                        entry.Entity.UpdatedBy = _userSession.UserId;
                        break;
                }
            }

            if (_userSession.UserId == null)
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            else
            {
                return await base.SaveChangesAsync(_userSession.UserId, cancellationToken);
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            var properties = builder.Model.GetEntityTypes().SelectMany(_ => _.GetProperties());

            foreach (var property in properties)
            {
                if (property.Name is "CreatedBy" or "UpdatedBy")
                    property.SetColumnType("varchar(128)");
            }

            base.OnModelCreating(builder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseLoggerFactory(_loggerFactory);
    }

    public class AppDbContextDesignFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
               .SetBasePath(Path.Combine(Directory.GetCurrentDirectory()))
               .AddJsonFile("appsettings.json")
               //.AddEnvironmentVariables()
               .Build();

            var migrationsAssembly = typeof(AppDbContext).Assembly.GetName();

            string connStr = configuration.GetSection("DefaultConnection").Value;

            var builder = new DbContextOptionsBuilder<AppDbContext>();

            builder.UseNpgsql(connStr,
                sql => sql.MigrationsAssembly(migrationsAssembly.Name).UseNodaTime());

            builder.UseOpenIddict();

            return new AppDbContext(builder.Options, null, null);
        }
    }
}
