using CT554_Entity.Entity;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CT554_API.Auth
{
    public class PoliciesAuthorizationHandler : AuthorizationHandler<RequirementRoleClaim>
    {
        private readonly UserManager<User> userManager;

        public PoliciesAuthorizationHandler(UserManager<User> userManager): base()
        {
            this.userManager = userManager;
        }

        protected override async Task<Task> HandleRequirementAsync(AuthorizationHandlerContext context, RequirementRoleClaim requirement)
        {
            if (context.User == null || !context.User.Identity!.IsAuthenticated)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var tokenRoleName = context.User.Claims.Where(claim => claim.Type == "scope").FirstOrDefault()!.Value ?? string.Empty;

            bool isValid = false;
            if (tokenRoleName != string.Empty)
            {
                var user = await userManager.FindByIdAsync(context.User.Claims.FirstOrDefault(claim => claim.Type == "client_id")!.Value);
                isValid = (await userManager.GetRolesAsync(user ?? new User())).Contains(tokenRoleName);
            }

            if (isValid)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            //context.Succeed(requirement);
            context.Fail();
            return Task.CompletedTask;
        }
    }
}
