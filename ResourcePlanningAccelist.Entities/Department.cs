using System.ComponentModel.DataAnnotations;
using ResourcePlanningAccelist.Entities.Abstractions;

namespace ResourcePlanningAccelist.Entities;

public class Department : AuditableEntity
{
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public virtual ICollection<AppUser> Users { get; set; } = new List<AppUser>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}