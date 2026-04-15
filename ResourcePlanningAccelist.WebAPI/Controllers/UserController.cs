using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageUsers;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageUsers;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _dbContext;

    public UserController(IMediator mediator, ApplicationDbContext dbContext)
    {
        _mediator = mediator;
        _dbContext = dbContext;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        if (!result.Success)
        {
            return Unauthorized(result);
        }
        return Ok(result);
    }

    [HttpGet("get-profile")]
    public async Task<ActionResult<GetUserProfileResponse>> GetProfile(
        CancellationToken cancellationToken)
    {
        var userId = await ResolveCurrentUserIdAsync(cancellationToken);
        if (userId == null)
        {
            return Unauthorized("Unable to resolve user identity.");
        }

        var request = new GetUserProfileRequest { UserId = userId.Value };
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("debug-claims")]
    public IActionResult GetDebugClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        return Ok(new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated,
            AuthenticationType = User.Identity?.AuthenticationType,
            Claims = claims
        });
    }

    [HttpPut("update-profile")]
    public async Task<ActionResult<UpdateUserProfileResponse>> UpdateProfile(
        [FromBody] UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = await ResolveCurrentUserIdAsync(cancellationToken);
        if (userId == null)
        {
            return Unauthorized("Unable to resolve user identity.");
        }

        request.UserId = userId.Value;
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Resolves the current user's database ID from the JWT claims.
    /// In development mode, the NameIdentifier is a username string (not a GUID),
    /// so we fall back to looking up the user by FullName.
    /// </summary>
    private async Task<Guid?> ResolveCurrentUserIdAsync(CancellationToken cancellationToken)
    {
        var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(nameIdentifier))
        {
            return null;
        }

        // If the claim is already a valid GUID (production JWT), use it directly
        if (Guid.TryParse(nameIdentifier, out var parsedUserId))
        {
            return parsedUserId;
        }

        // Otherwise, resolve by FullName (development auth mode)
        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(item => item.FullName == nameIdentifier)
            .Select(item => new { item.Id })
            .FirstOrDefaultAsync(cancellationToken);

        return user?.Id;
    }
}
