﻿using ApplicationCore.Helpers;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Web.Services
{
    public class UserSessionService : IUserSessionService
    {
        readonly AuthenticationStateProvider _authenticationStateProvider;

        public UserSessionService(AuthenticationStateProvider authenticationStateProvider)
        {
            _authenticationStateProvider = authenticationStateProvider;
        }

        public string UserId 
        { 
            get
            {
                var authState = AsyncHelper.RunSync(async () => await _authenticationStateProvider.GetAuthenticationStateAsync());
                var user = authState.User;

                if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                {
                    return "?";
                }

                return user.FindFirstValue(ClaimTypes.NameIdentifier);
            }
        }

        public IEnumerable<AppClaim> Claims 
        { 
            get
            {
                var authState = AsyncHelper.RunSync(async () => await _authenticationStateProvider.GetAuthenticationStateAsync());
                var user = authState.User;

                if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                {
                    return Enumerable.Empty<AppClaim>();
                }

                return user.Claims.AsEnumerable().Select(item => new AppClaim(item.Type, item.Value)).ToArray();
            }
        }
    }
}
