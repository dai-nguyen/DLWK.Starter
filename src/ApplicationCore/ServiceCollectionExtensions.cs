using ApplicationCore.Configurations;
using ApplicationCore.Data;
using ApplicationCore.Features.Users.Commands;
using ApplicationCore.Interfaces;
using ApplicationCore.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ApplicationCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseApplicationCore(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDatabase(configuration);
            services.AddIdentity();
            services.AddServerLocalization();
            services.AddEmailSender(configuration);

            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddMediatR(Assembly.GetExecutingAssembly());            
            services.AddValidations();

            services.AddTransient<IFileService, FileService>();

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

            builder.UseOpenIddict();
        }        

        internal static IServiceCollection AddIdentity(this IServiceCollection services)
        {
            services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = Claims.Role;
                options.ClaimsIdentity.EmailClaimType = Claims.Email;
            });

            return services;
        }

        internal static IServiceCollection AddServerLocalization(this IServiceCollection services)
        {
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            //services.AddTransient(typeof(IStringLocalizer<>), typeof(ServerLocalizer<>));
            return services;
        }

        internal static IServiceCollection AddEmailSender(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<EmailConfiguration>(options =>
            {
                options.From = configuration.GetValue<string>("SmtpFrom");
                options.Host = configuration.GetValue<string>("SmtpHost");
                options.Port = configuration.GetValue<int>("SmtpPort");
                options.UserName = configuration.GetValue<string>("SmtpUserName");
                options.Password = configuration.GetValue<string>("SmtpPassword");
                options.DisplayName = configuration.GetValue<string>("SmtpDisplayName");
            });

            services.AddTransient<IEmailService, SmtpEmailService>();
            return services;
        }

        internal static IServiceCollection AddValidations(this IServiceCollection services)
        {
            services.AddTransient<IValidator<RegisterUserCommand>, RegisterUserCommandValidator>();
            services.AddTransient<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
            services.AddTransient<IValidator<UpdateUserCommand>, UpdateUserCommandValidator>();
            services.AddTransient<IValidator<UpdateUserProfileCommand>, UpdateUserProfileCommandValidator>();

            return services;
        }
    }
}