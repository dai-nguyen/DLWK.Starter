using ApplicationCore.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;

namespace ApplicationCore.Workers
{
    public class SeedWorker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public SeedWorker(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
            await dbContext.Database.MigrateAsync();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();


            if (!await dbContext.Roles.AnyAsync())
            {
                foreach (var role in GetPreconfiguredRoles())
                {
                    await roleManager.CreateAsync(role);
                    await roleManager.AddClaimAsync(
                        role,
                        new Claim(Constants.ClaimNames.roles, "read edit create delete"));
                    await roleManager.AddClaimAsync(
                        role,
                        new Claim(Constants.ClaimNames.users, "read edit create delete"));
                }
            }

            if (!await dbContext.Users.AnyAsync())
            {
                var defaultUser = new AppUser()
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "admin",
                    Email = "admin@starter.com",
                };
                await userManager.CreateAsync(defaultUser, "Pa$$w0rd");
                await userManager.AddToRoleAsync(defaultUser, "Admin");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        IEnumerable<AppRole> GetPreconfiguredRoles()
        {
            return new AppRole[]
            {
                new AppRole()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    Description = "Administrator"
                }
            };
        }
    }
}
