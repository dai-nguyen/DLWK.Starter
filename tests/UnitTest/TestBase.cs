using ApplicationCore;
using ApplicationCore.Constants;
using ApplicationCore.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NpgsqlTypes;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace UnitTest
{
    [TestClass]
    public class TestBase
    {
        readonly IHost _host;
        protected readonly IMediator _mediator;
        
        public TestBase()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(cfg =>
                {                    
                    cfg.AddJsonFile("appsettings.json");
                    cfg.AddUserSecrets<TestBase>();                    
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var context = new DefaultHttpContext();
                    var principal = new ClaimsPrincipal();
                    var identity = new GenericIdentity("admin");
                    identity.AddClaims(new[]
                    {
                        new Claim(Const.ClaimNames.roles, Const.ClaimNames.roles),
                        new Claim(Const.ClaimNames.users, Const.ClaimNames.users),
                        new Claim(Const.ClaimNames.customers, Const.ClaimNames.customers),
                    });

                    principal.AddIdentity(identity);
                    context.User = principal;
                    IHttpContextAccessor accessor = new HttpContextAccessor();
                    accessor.HttpContext = context;

                    services.AddSingleton(accessor);

                    services.AddHttpClient();
                    services.AddMemoryCache();

                    services.UseApplicationCore(hostContext.Configuration);

                    services.AddScoped<IUserSessionService, FakeUserSessionService>();

                })
                .ConfigureLogging((context, cfg) =>
                {
                    string connStr = context.Configuration.GetSection("DefaultConnection")?.Value ?? "";
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
                })
                .UseSerilog()
                .Build();

            _mediator = GetRequiredService<IMediator>();            
        }

        public T GetRequiredService<T>()
        {
            try
            {
                return _host.Services.GetRequiredService<T>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
            return default(T);
        }

        public IEnumerable<T> GetServices<T>()
        {
            try
            {
                return _host.Services.GetServices<T>();
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
            return Enumerable.Empty<T>();
        }
    }
}