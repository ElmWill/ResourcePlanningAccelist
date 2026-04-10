using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using ResourcePlanningAccelist.Constants;

namespace ResourcePlanningAccelist.WebAPI.AuthorizationPolicies;

public static class RoleAuthorizationPolicyExtensions
{
    public static IServiceCollection AddRoleAuthorizationPolicies(this IServiceCollection services)
    {
        services
            .AddAuthentication(DevelopmentAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, DevelopmentAuthenticationHandler>(
                DevelopmentAuthenticationHandler.SchemeName,
                _ => { });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicyNames.GmOnly, policy =>
                policy.RequireRole(UserRole.Gm.ToString().ToLowerInvariant()));

            options.AddPolicy(AuthorizationPolicyNames.PmOnly, policy =>
                policy.RequireRole(UserRole.Pm.ToString().ToLowerInvariant()));

            options.AddPolicy(AuthorizationPolicyNames.MarketingOnly, policy =>
                policy.RequireRole(UserRole.Marketing.ToString().ToLowerInvariant()));

            options.AddPolicy(AuthorizationPolicyNames.PmOrHr, policy =>
                policy.RequireRole(
                    UserRole.Pm.ToString().ToLowerInvariant(),
                    UserRole.Hr.ToString().ToLowerInvariant()));

            options.AddPolicy(AuthorizationPolicyNames.GmOrPm, policy =>
                policy.RequireRole(
                    UserRole.Gm.ToString().ToLowerInvariant(),
                    UserRole.Pm.ToString().ToLowerInvariant()));

            options.AddPolicy(AuthorizationPolicyNames.HrOrGm, policy =>
                policy.RequireRole(
                    UserRole.Hr.ToString().ToLowerInvariant(),
                    UserRole.Gm.ToString().ToLowerInvariant()));

            options.AddPolicy(AuthorizationPolicyNames.PmHrOrGm, policy =>
                policy.RequireRole(
                    UserRole.Pm.ToString().ToLowerInvariant(),
                    UserRole.Hr.ToString().ToLowerInvariant(),
                    UserRole.Gm.ToString().ToLowerInvariant()));

            options.AddPolicy(AuthorizationPolicyNames.ProjectReadAccess, policy =>
                policy.RequireRole(
                    UserRole.Marketing.ToString().ToLowerInvariant(),
                    UserRole.Pm.ToString().ToLowerInvariant(),
                    UserRole.Gm.ToString().ToLowerInvariant(),
                    UserRole.Hr.ToString().ToLowerInvariant()));
        });

        return services;
    }
}