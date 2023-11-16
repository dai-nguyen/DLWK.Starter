using ApplicationCore;
using ApplicationCore.Constants;
using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Jobs;
using ApplicationCore.Policies;
using ApplicationCore.States;
using ApplicationCore.Workers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.OpenApi.Models;
using MudBlazor.Services;
using NpgsqlTypes;
using Quartz;
using Quartz.AspNetCore;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.PostgreSQL.ColumnWriters;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Web;
using Web.Components;
using Web.Data;
using Web.Middleware;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

string certData = builder.Configuration.GetSection("CertData")?.Value ?? "";
string certPath = builder.Configuration.GetSection("CertPath")?.Value ?? "";

string connStr = builder.Configuration.GetSection("DefaultConnection")?.Value ?? "";
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
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
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
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<AppUser>>();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddMudServices();

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions()
            {
                AutoReplenishment = true,
                PermitLimit = httpContext.User.Identity?.IsAuthenticated == true ? 
                    builder.Configuration.GetValue<int>("RateLimiter:AuthPermitLimit") : builder.Configuration.GetValue<int>("RateLimiter:NonAuthPermitLimit"),
                Window = TimeSpan.FromSeconds(httpContext.User.Identity?.IsAuthenticated == true ? 
                    builder.Configuration.GetValue<int>("RateLimiter:AuthWindowInSeconds") : builder.Configuration.GetValue<int>("RateLimiter:NonAuthWindowInSecond")),
            }));
});


builder.Services.UseApplicationCore(builder.Configuration);

builder.Services.AddQuartz(options =>
{
    options.UseMicrosoftDependencyInjectionJobFactory();
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore();
    options.UseDefaultThreadPool(tp =>
    {
        tp.MaxConcurrency = 10;
    });

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

    options.AddJob<TestJob>(_ =>
        _.StoreDurably()
        .WithIdentity("test_job"));

    options.ScheduleJob<WebhookJob>(trigger => trigger
        .WithIdentity("Webhook Job")
        .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(5)))
        .WithDailyTimeIntervalSchedule(x => x.WithInterval(10, IntervalUnit.Second))
        .WithDescription("my awesome trigger configured for a job with single call"));
});

builder.Services.AddQuartzServer(options =>
{
    options.WaitForJobsToComplete = true;
});

// Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
//builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

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
        options.SetTokenEndpointUris("connect/token");

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

builder.Services.AddHostedService<SeedWorker>();
builder.Services.AddTransient<TestJob>();

builder.Services.AddScoped<IUserSessionService, UserSessionService>();
builder.Services.AddScoped<UserProfileState>();
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<IAuthorizationHandler, ClaimRequirementHandler>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DLWK.Starter API", Version = "v1" });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Const.ClaimNames.roles,
        policy => policy.RequireClaim(Const.ClaimNames.roles));

    options.AddPolicy(Const.ClaimNames.users,
        policy => policy.RequireClaim(Const.ClaimNames.users));

    options.AddPolicy(Const.ClaimNames.customers,
        policy => policy.RequireClaim(Const.ClaimNames.customers));
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

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "My API V1");
});

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<LoginMiddleware>();
app.UseMiddleware<LogoutMiddleware>();

app.UseRateLimiter();

//app.MapBlazorHub();
//app.MapFallbackToPage("/_Host");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.UseAntiforgery();

app.UseEndpoints(options =>
{    
    options.MapControllers();
    options.MapDefaultControllerRoute();
    options.MapSwagger();
    //options.MapBlazorHub();
    //options.MapFallbackToPage("/_Host");
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