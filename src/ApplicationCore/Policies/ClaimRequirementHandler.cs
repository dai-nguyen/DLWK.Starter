using ApplicationCore.Constants;
using Microsoft.AspNetCore.Authorization;

namespace ApplicationCore.Policies
{
    public class ClaimRequirementHandler : AuthorizationHandler<ClaimRequirement>
    {
        List<string> _values = new List<string>()
        {
            Const.Permissions.read,
            Const.Permissions.edit,
            Const.Permissions.create,
            Const.Permissions.delete,
        };

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            ClaimRequirement requirement)
        {
            var claim = context
                .User
                .FindFirst(_ => _.Type == requirement.Claim);

            if (claim is null)
                return Task.CompletedTask;

            if (!string.IsNullOrEmpty(claim.Value)
                && !string.IsNullOrWhiteSpace(claim.Value))
            {
                var values = claim.Value.Split(" ");

                var found = values.Any(_ => _values.Contains(_));

                if (found)
                    context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
