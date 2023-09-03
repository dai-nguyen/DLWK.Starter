using ApplicationCore.Data;
using Microsoft.EntityFrameworkCore;
using PointRewardModule.Data;
using PointRewardModule.Entities;

namespace Web.Workers
{
    public class SeedPointRewardModule : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public SeedPointRewardModule(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var pointRewardModuledbContext = scope.ServiceProvider.GetRequiredService<PointRewardModuleDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
            await dbContext.Database.MigrateAsync();

            if (!await pointRewardModuledbContext.Banks.AnyAsync())
            {
                var users = await dbContext.Users.ToListAsync();

                foreach (var user in users)
                {
                    foreach (var bank in GetPreconfiguredBanks())
                    {
                        bank.OwnerId = user.Id;

                        pointRewardModuledbContext.Banks.Add(bank);
                    }

                    await pointRewardModuledbContext.SaveChangesAsync(cancellationToken);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        IEnumerable<Bank> GetPreconfiguredBanks()
        {
            return new Bank[]
            {
                new Bank()
                {
                    Id = Guid.NewGuid().ToString(),
                    BankType = BankTypes.Checking,
                    Balance = 0
                },
                new Bank()
                {
                    Id = Guid.NewGuid().ToString(),
                    BankType = BankTypes.Saving,
                    Balance = 0
                },
                new Bank()
                {
                    Id = Guid.NewGuid().ToString(),
                    BankType = BankTypes.Investing,
                    Balance = 0
                },
            };
        }
    }
}
