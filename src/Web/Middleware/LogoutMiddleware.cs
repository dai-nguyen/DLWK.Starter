using ApplicationCore.Data;
using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;

namespace Web.Middleware
{    
    //https://github.com/dotnet/aspnetcore/issues/13601
    public class LogoutMiddleware
    {
        public static IDictionary<Guid, string> Logouts { get; private set; }
            = new ConcurrentDictionary<Guid, string>();


        private readonly RequestDelegate _next;

        public LogoutMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(
            HttpContext context, 
            SignInManager<AppUser> signInMgr)
        {
            if (context.Request.Path == "/logout" 
                && context.Request.Query.ContainsKey("key"))
            {
                var key = Guid.Parse(context.Request.Query["key"]);

                if (!Logouts.ContainsKey(key))
                {
                    context.Response.Redirect("/pages/authentication/login");
                }

                Logouts.Remove(key);
                await signInMgr.SignOutAsync();
                context.Response.Redirect("/pages/authentication/login");
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}
