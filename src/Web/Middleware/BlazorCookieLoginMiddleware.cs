using ApplicationCore.Data;
using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;

namespace Web.Middleware
{
    public class LoginInfo
    {
        public string Username { get; set; }

        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    //https://github.com/dotnet/aspnetcore/issues/13601
    public class BlazorCookieLoginMiddleware
    {
        public static IDictionary<Guid, LoginInfo> Logins { get; private set; }
            = new ConcurrentDictionary<Guid, LoginInfo>();


        private readonly RequestDelegate _next;

        public BlazorCookieLoginMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(
            HttpContext context, 
            SignInManager<AppUser> signInMgr)
        {
            if (context.Request.Path == "/login" 
                && context.Request.Query.ContainsKey("key"))
            {
                var key = Guid.Parse(context.Request.Query["key"]);

                if (!Logins.ContainsKey(key))
                {
                    context.Response.Redirect("/pages/authentication/login");
                    return;
                }

                var info = Logins[key];

                var result = await signInMgr.PasswordSignInAsync(
                    info.Username, 
                    info.Password, 
                    info.RememberMe, 
                    lockoutOnFailure: true);

                info.Password = String.Empty;

                if (result.Succeeded)
                {
                    Logins.Remove(key);
                    context.Response.Redirect("/");
                    return;
                }                
                else
                {                    
                    context.Response.Redirect("/pages/authentication/login");
                    return;
                }
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}
