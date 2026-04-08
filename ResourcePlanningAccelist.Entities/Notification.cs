using System.ComponentModel.DataAnnotations;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Entities.Abstractions;

namespace ResourcePlanningAccelist.Entities;

public class Notification : AuditableEntity
{
    public Guid UserId { get; set; }

    public virtual AppUser User { get; set; } = default!;

    public NotificationType Type { get; set; }

    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTimeOffset? ReadAt { get; set; }

    [StringLength(100)]
    public string? SourceEntityType { get; set; }

    public Guid? SourceEntityId { get; set; }
}