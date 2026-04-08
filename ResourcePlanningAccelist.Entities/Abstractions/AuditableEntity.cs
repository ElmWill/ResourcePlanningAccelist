namespace ResourcePlanningAccelist.Entities.Abstractions;

public abstract class AuditableEntity : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}