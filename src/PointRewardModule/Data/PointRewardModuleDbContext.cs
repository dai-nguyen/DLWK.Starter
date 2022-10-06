using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PointRewardModule.Configurations;

namespace PointRewardModule.Data
{
    public class PointRewardModuleDbContext : AudiableDbContextBase
    {
        readonly ILoggerFactory _loggerFactory;
        readonly IUserSessionService _userSession;
        
        public DbSet<Entities.Bank> Banks { get; set; }
        public DbSet<Entities.Transaction> Transactions { get; set; }        

        public PointRewardModuleDbContext(
            DbContextOptions options,
            ILoggerFactory loggerFactory,
            IUserSessionService userSession) 
            : base(options)
        {
            _loggerFactory = loggerFactory;
            _userSession = userSession;
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
            builder.HasDefaultSchema("PointReward");

            var properties = builder.Model.GetEntityTypes().SelectMany(_ => _.GetProperties());

            foreach (var property in properties)
            {
                if (property.Name is "CreatedBy" or "UpdatedBy")
                    property.SetColumnType("varchar(128)");
            }

            builder.ApplyConfiguration(new BankConfiguration());
            builder.ApplyConfiguration(new TransactionConfiguration());

            base.OnModelCreating(builder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseLoggerFactory(_loggerFactory);
    }

    public class PointRewardModuleDbContextDesignFactory : IDesignTimeDbContextFactory<PointRewardModuleDbContext>
    {
        public PointRewardModuleDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
               .SetBasePath(Path.Combine(Directory.GetCurrentDirectory()))
               .AddJsonFile("appsettings.json")
               //.AddEnvironmentVariables()
               .Build();

            var migrationsAssembly = typeof(PointRewardModuleDbContext).Assembly.GetName();

            string connStr = configuration.GetSection("DefaultConnection").Value;

            var builder = new DbContextOptionsBuilder<PointRewardModuleDbContext>();

            builder.UseNpgsql(connStr,
                sql => sql.MigrationsAssembly(migrationsAssembly.Name).UseNodaTime());

            //builder.UseOpenIddict();

            return new PointRewardModuleDbContext(builder.Options, null, null);
        }
    }
}
