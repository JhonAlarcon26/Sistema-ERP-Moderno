using Microsoft.AspNetCore.Authorization;

namespace Sistema_ERP.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }

    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User == null)
            {
                return Task.CompletedTask;
            }


            if (context.User.IsInRole("Administrador"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }


            var hasPermission = context.User.Claims.Any(c =>
                c.Type == "Permission" && c.Value == requirement.Permission);

            if (hasPermission)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
