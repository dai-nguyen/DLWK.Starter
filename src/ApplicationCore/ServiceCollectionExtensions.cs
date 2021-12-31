using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Localization;
using ApplicationCore.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Reflection;

namespace ApplicationCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseApplicationCore(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDatabase(configuration);
            services.AddServerLocalization();
            services.AddEmailSender();

            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddMediatR(Assembly.GetExecutingAssembly());

            return services;
        }

        internal static IServiceCollection AddDatabase(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(builder => GetDbContextOption(builder, configuration));
            return services;
        }

        internal static void GetDbContextOption(
            DbContextOptionsBuilder builder,
            IConfiguration configuration)
        {
            var migrationsAssembly = typeof(AppDbContext).Assembly.GetName();

            string connStr = configuration.GetSection("DefaultConnection").Value;

            builder.UseNpgsql(connStr,
                sql => sql.MigrationsAssembly(migrationsAssembly.Name).UseNodaTime());
        }

        internal static IServiceCollection AddServerLocalization(this IServiceCollection services)
        {
            services.AddTransient(typeof(IStringLocalizer<>), typeof(ServerLocalizer<>));
            return services;
        }

        internal static IServiceCollection AddEmailSender(this IServiceCollection services)
        {
            services.AddTransient<IEmailService, SmtpEmailService>();
            return services;
        }
    }
}