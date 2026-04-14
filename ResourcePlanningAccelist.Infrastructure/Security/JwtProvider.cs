using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ResourcePlanningAccelist.Commons.Security;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Infrastructure.Security;

public class JwtProvider : IJwtProvider
{
    private readonly IConfiguration _configuration;

    public JwtProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Generate(AppUser user)
    {
        var secretKey = _configuration["Jwt:SecretKey"] ?? "AccelistResourcePlanningProjectSecretKey";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString().ToLowerInvariant())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "ResourcePlanningAccelist",
            audience: _configuration["Jwt:Audience"] ?? "ResourcePlanningAccelist",
            claims: claims,
            expires: DateTime.Now.AddHours(24),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
