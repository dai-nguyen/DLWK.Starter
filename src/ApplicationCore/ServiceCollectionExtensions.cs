using ApplicationCore.Configurations;
using ApplicationCore.Data;
using ApplicationCore.Features.Contacts.Commands;
using ApplicationCore.Features.Customers.Commands;
using ApplicationCore.Features.Projects.Commands;
using ApplicationCore.Features.Users.Commands;
using ApplicationCore.Interfaces;
using ApplicationCore.Services;
using ApplicationCore.UserDefinedMigrator;
using FluentMigrator.Runner;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
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
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(typeof(ServiceCollectionExtensions).Assembly);
            });
            services.AddValidations();
            services.AddFluentMigrator(configuration);

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

            NpgsqlConnection.GlobalTypeMapper.UseNodaTime();
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
            services.AddScoped<IValidator<RegisterUserCommand>, RegisterUserCommandValidator>();
            services.AddScoped<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
            services.AddScoped<IValidator<UpdateUserCommand>, UpdateUserCommandValidator>();
            services.AddScoped<IValidator<UpdateUserProfileCommand>, UpdateUserProfileCommandValidator>();

            // customer
            services.AddScoped<IValidator<CreateCustomerCommand>, CreateCustomerCommandValidator>();
            services.AddScoped<IValidator<UpdateCustomerCommand>, UpdateCustomerCommandValidator>();
            services.AddScoped<IValidator<CreateCustomerUdDefinitionCommand>, CreateCustomerUdDefinitionCommandValidator>();
            services.AddScoped<IValidator<UpdateCustomerUdDefinitionCommand>, UpdateCustomerUdDefinitionCommandValidator>();

            //contact
            services.AddScoped<IValidator<CreateContactCommand>, CreateContactCommandValidator>();
            services.AddScoped<IValidator<CreateContactUdDefinitionCommand>, CreateContactUdDefinitionCommandValidator>();
            services.AddScoped<IValidator<UpdateContactCommand>, UpdateContactCommandValidator>();
            services.AddScoped<IValidator<UpdateContactUdDefinitionCommand>, UpdateContactUdDefinitionCommandValidator>();

            // project
            services.AddScoped<IValidator<CreateProjectCommand>, CreateProjectCommandValidator>();
            services.AddScoped<IValidator<UpdateProjectCommand>, UpdateProjectCommandValidator>();

            

            return services;
        }

        internal static IServiceCollection AddFluentMigrator(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddPostgres()
                    .WithGlobalConnectionString(configuration.GetSection("DefaultConnection").Value)
                    .ScanIn(typeof(ContactUdMigrator).Assembly).For.Migrations());

            return services;
        }
    }
}