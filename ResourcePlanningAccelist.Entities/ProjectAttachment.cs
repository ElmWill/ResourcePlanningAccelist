using System.ComponentModel.DataAnnotations;
using ResourcePlanningAccelist.Entities.Abstractions;

namespace ResourcePlanningAccelist.Entities;

public class ProjectAttachment : AuditableEntity
{
    public Guid ProjectId { get; set; }

    public virtual Project Project { get; set; } = default!;

    public Guid? UploadedByUserId { get; set; }

    public virtual AppUser? UploadedByUser { get; set; }

    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [StringLength(500)]
    public string StorageKey { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ContentType { get; set; }

    public long? FileSizeBytes { get; set; }
}