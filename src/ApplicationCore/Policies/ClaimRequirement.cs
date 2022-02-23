using Microsoft.AspNetCore.Authorization;

namespace ApplicationCore.Policies
{
    public class ClaimRequirement : IAuthorizationRequirement
    {
        public string Claim { get; }

        public ClaimRequirement(string claim) =>
            Claim = claim;
    }
}
