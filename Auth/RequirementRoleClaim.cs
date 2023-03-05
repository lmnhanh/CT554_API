using Microsoft.AspNetCore.Authorization;

namespace CT554_API.Auth
{
    public class RequirementRoleClaim : IAuthorizationRequirement
    {
        public string RoleName { get; set; } = string.Empty;

        public RequirementRoleClaim(string roleName)
        {
            RoleName = roleName;
        }
    }
}
