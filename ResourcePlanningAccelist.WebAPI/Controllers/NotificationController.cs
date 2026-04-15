using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageNotifications;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageNotifications;

namespace ResourcePlanningAccelist.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<GetNotificationsResponse>> Get([FromQuery] int? limit, CancellationToken cancellationToken)
    {
        var request = new GetNotificationsRequest
        {
            UserId = GetCurrentUserId(),
            Limit = limit
        };

        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id}/read")]
    public async Task<ActionResult<bool>> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var request = new MarkNotificationAsReadRequest
        {
            NotificationId = id,
            UserId = GetCurrentUserId()
        };

        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("mark-all-read")]
    public async Task<ActionResult<bool>> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var request = new MarkAllNotificationsAsReadRequest
        {
            UserId = GetCurrentUserId()
        };

        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    private Guid GetCurrentUserId()
    {
        var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(nameIdentifier, out var userId))
        {
            return userId;
        }

        return Guid.Empty;
    }
}
