using ApplicationCore;
using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Policies;
using ApplicationCore.States;
using ApplicationCore.Workers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using NpgsqlTypes;
using Quartz;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.PostgreSQL.ColumnWriters;
using System.Security.Cryptography.X509Certificates;
using Web;
using Web.Data;
using Web.Middleware;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

string certData = builder.Configuration.GetSection("CertData").Value;
string certPath = builder.Configuration.GetSection("CertPath").Value;

string connStr = builder.Configuration.GetSection("DefaultConnection").Value;
string tableName = "Logs";

var columnWriters = new Dictionary<string, ColumnWriterBase>
{
    {"message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
    {"message_template", new MessageTemplateColumnWriter(NpgsqlDbType.Text) },
    {"level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
    {"raise_date", new TimestampColumnWriter(NpgsqlDbType.Timestamp) },
    {"exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
    {"properties", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
    {"machine_name", new SinglePropertyColumnWriter("MachineName", PropertyWriteMethod.ToString, NpgsqlDbType.Text, "l") },
    {"user_id", new SinglePropertyColumnWriter("UserId", PropertyWriteMethod.ToString, NpgsqlDbType.Text) }
};

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.File("Logs/log.txt")
    .WriteTo.Console()
    .WriteTo.PostgreSQL(
        connStr,
        tableName,
        columnWriters,
        needAutoCreateTable: true)    
    .CreateLogger();

builder.Host.UseSerilog();

builder.WebHost.ConfigureServices((context, services) =>
{
    HostConfig.CertPath = context.Configuration["CertPath"];
    HostConfig.CertPassword = context.Configuration["CertPassword"];
    HostConfig.CertData = context.Configuration["CertData"];
    HostConfig.OpenIddictKeyEncipherment = context.Configuration["OpenIddictKeyEncipherment"];
    HostConfig.OpenIddictKeyEnciphermentPass = context.Configuration["OpenIddictKeyEnciphermentPass"];
    HostConfig.OpenIddictDigitalSignature = context.Configuration["OpenIddictDigitalSignature"];
    HostConfig.OpenIddictDigitalSignaturePass = context.Configuration["OpenIddictDigitalSignaturePass"];
})
.ConfigureKestrel((context, options) =>
{
    options.ListenAnyIP(5000);

    if (!string.IsNullOrEmpty(HostConfig.CertData) ||
        !string.IsNullOrEmpty(HostConfig.CertPath))
    {
        options.ListenAnyIP(5001, listOpt =>
        {
            if (!string.IsNullOrEmpty(HostConfig.CertData))
            {
                try
                {
                    var bytes = Convert.FromBase64String(HostConfig.CertData);
                    var cert = new X509Certificate2(bytes, HostConfig.CertPassword);

                    listOpt.UseHttps(cert);
                }
                catch (Exception e)
                {
                    Log.Error("Error at UseHttps using X509Certificate2");
                }
            }
            else if (!string.IsNullOrEmpty(HostConfig.CertPath)
                && !string.IsNullOrEmpty(HostConfig.CertPassword))
            {
                try
                {
                    listOpt.UseHttps(HostConfig.CertPath, HostConfig.CertPassword);
                }
                catch (Exception e)
                {
                    Log.Error("Error at UseHttps using filename and password", e);
                }
            }
        });
    }
});

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<AppUser>>();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddMudServices();

builder.Services.UseApplicationCore(builder.Configuration);

builder.Services.AddQuartz(options =>
{
    options.UseMicrosoftDependencyInjectionJobFactory();
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore();    
    //options.UsePersistentStore(s =>
    //{
    //    s.UseProperties = true;
    //    s.RetryInterval = TimeSpan.FromSeconds(15);

    //    s.UsePostgres(sql =>
    //    {
    //        sql.ConnectionString = connStr;
    //        sql.TablePrefix = "QRTZ_";
    //    });
    //    s.UseJsonSerializer();

    //});
});

// Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<AppDbContext>();
        options.UseQuartz();        
    })
    .AddServer(options =>
    {
        // Enable the token endpoint.
        options.SetTokenEndpointUris("/connect/token");

        // Enable the password flow.
        options.AllowPasswordFlow();
        options.AllowRefreshTokenFlow();

        // Accept anonymous clients (i.e clients that don't send a client_id).
        options.AcceptAnonymousClients();        

        var keyBytes = Convert.FromBase64String(HostConfig.OpenIddictKeyEncipherment);
        var keyCert = new X509Certificate2(
            keyBytes,
            HostConfig.OpenIddictKeyEnciphermentPass,
            X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet);

        var signBytes = Convert.FromBase64String(HostConfig.OpenIddictDigitalSignature);
        var signCert = new X509Certificate2(
            signBytes,
            HostConfig.OpenIddictDigitalSignaturePass,
            X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet);

        options.AddEncryptionCertificate(keyCert);
        options.AddSigningCertificate(signCert);

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough();

    })
    .AddValidation(options =>
    {
        // Import the configuration from the local OpenIddict server instance.
        options.UseLocalServer();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });

builder.Services.AddHostedService<DefaultWorker>();

builder.Services.AddScoped<IUserSessionService, UserSessionService>();
builder.Services.AddScoped<UserProfileState>();
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<IAuthorizationHandler, ClaimRequirementHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Constants.ClaimNames.roles,
        policy => policy.RequireClaim(Constants.ClaimNames.roles));

    options.AddPolicy(Constants.ClaimNames.users,
        policy => policy.RequireClaim(Constants.ClaimNames.users));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (!string.IsNullOrEmpty(certData) || !string.IsNullOrEmpty(certPath))
    app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<LoginMiddleware>();
app.UseMiddleware<LogoutMiddleware>();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.UseEndpoints(options =>
{
    options.MapControllers();
    options.MapDefaultControllerRoute();
});

app.Run();

public static class HostConfig
{
    public static string CertPath { get; set; } = string.Empty;
    public static string CertPassword { get; set; } = string.Empty;
    public static string CertData { get; set; } = string.Empty;

    public static string OpenIddictKeyEncipherment { get; set; } = string.Empty;
    public static string OpenIddictKeyEnciphermentPass { get; set; } = string.Empty;
    public static string OpenIddictDigitalSignature { get; set; } = string.Empty;
    public static string OpenIddictDigitalSignaturePass { get; set; } = string.Empty;
}