using MudBlazor.Services;
using NpgsqlTypes;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.PostgreSQL.ColumnWriters;
using System.Security.Cryptography.X509Certificates;
using Web.Data;
using ApplicationCore;
using ApplicationCore.Interfaces;
using Web.Services;
using ApplicationCore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ApplicationCore.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using Web;
using Microsoft.AspNetCore.Components.Server;
using Web.Middleware;

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
                catch (Exception)
                { }
            }
            else if (!string.IsNullOrEmpty(HostConfig.CertPath)
                && !string.IsNullOrEmpty(HostConfig.CertPassword))
            {
                try
                {
                    listOpt.UseHttps(HostConfig.CertPath, HostConfig.CertPassword);
                }
                catch (Exception)
                { }
            }
        });
    }
});

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<AppUser>>();
builder.Services.AddScoped<IHostEnvironmentAuthenticationStateProvider>(sp =>
{
    var provider = (ServerAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>();
    return provider;
});
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddMudServices();

builder.Services.UseApplicationCore(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserSessionService, UserSessionService>();


var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
    dbContext.Database.Migrate();

    var logFactory = services.GetRequiredService<ILoggerFactory>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    AsyncHelper.RunSync(() => AppDbContextSeed.SeedAsync(dbContext, userManager, roleManager, logFactory));
}

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

app.UseMiddleware<BlazorCookieLoginMiddleware>();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

public static class HostConfig
{
    public static string CertPath { get; set; }
    public static string CertPassword { get; set; }
    public static string CertData { get; set; }
}