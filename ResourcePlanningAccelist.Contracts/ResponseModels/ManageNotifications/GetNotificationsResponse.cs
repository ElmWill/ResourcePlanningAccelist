namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageNotifications;

public class GetNotificationsResponse
{
    public List<NotificationItemResponse> Notifications { get; set; } = new();
}

public class NotificationItemResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string? SourceEntityType { get; set; }
    public Guid? SourceEntityId { get; set; }
}
