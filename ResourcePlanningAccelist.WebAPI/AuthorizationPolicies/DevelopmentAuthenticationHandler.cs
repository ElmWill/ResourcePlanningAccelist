using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using ResourcePlanningAccelist.Constants;

namespace ResourcePlanningAccelist.WebAPI.AuthorizationPolicies;

public sealed class DevelopmentAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "DevelopmentAuth";

    public DevelopmentAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var requestedRole = Request.Headers["X-Debug-Role"].FirstOrDefault();
        var userName = Request.Headers["X-Debug-User"].FirstOrDefault() ?? "dev-user";

        var roles = new[]
        {
            UserRole.Marketing,
            UserRole.Pm,
            UserRole.Gm,
            UserRole.Hr,
            UserRole.Employee
        }
        .Select(role => role.ToString().ToLowerInvariant())
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var effectiveRoles = string.IsNullOrWhiteSpace(requestedRole)
            ? roles
            : new HashSet<string>(new[] { requestedRole.ToLowerInvariant() });

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.NameIdentifier, userName)
        };

        claims.AddRange(effectiveRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}