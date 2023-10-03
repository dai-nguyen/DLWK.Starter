using ApplicationCore.Constants;
using ApplicationCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    public class FakeUserSessionService : IUserSessionService
    {
        public string UserId => System.Environment.UserName;

        public string UserName => System.Environment.UserName;

        public IEnumerable<Claim> Claims => new[]
        {
            new Claim(Const.ClaimNames.roles, Const.ClaimNames.roles),
            new Claim(Const.ClaimNames.users, Const.ClaimNames.users),
            new Claim(Const.ClaimNames.customers, Const.ClaimNames.customers)
        };
    }
}
