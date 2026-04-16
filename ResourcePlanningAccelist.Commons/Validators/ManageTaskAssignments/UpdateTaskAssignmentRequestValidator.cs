using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageTaskAssignments;

namespace ResourcePlanningAccelist.Commons.Validators.ManageTaskAssignments;

public class UpdateTaskAssignmentRequestValidator : AbstractValidator<UpdateTaskAssignmentRequest>
{
    public UpdateTaskAssignmentRequestValidator()
    {
        RuleFor(r => r.TaskId)
            .NotEmpty()
            .WithMessage("Task ID is required");

        RuleFor(r => r.Status)
            .NotEmpty()
            .WithMessage("Status is required")
            .Must(s => new[] { "Pending", "InProgress", "Completed" }.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Status must be one of: Pending, InProgress, Completed");

        When(r => r.TaskName is not null, () =>
        {
            RuleFor(r => r.TaskName!)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Task name cannot be empty")
                .MaximumLength(500)
                .WithMessage("Task name must be at most 500 characters");
        });

        When(r => r.Description is not null, () =>
        {
            RuleFor(r => r.Description!)
                .MaximumLength(2000)
                .WithMessage("Description must be at most 2000 characters");
        });

        When(r => !string.IsNullOrEmpty(r.Priority), () =>
        {
            RuleFor(r => r.Priority!)
                .Must(p => new[] { "Low", "Medium", "High" }.Contains(p, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Priority must be one of: Low, Medium, High");
        });

        When(r => r.DueDate.HasValue, () =>
        {
            RuleFor(r => r.DueDate)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Due date must be in the future");
        });

        When(r => r.WorkloadHours.HasValue, () =>
        {
            RuleFor(r => r.WorkloadHours!.Value)
                .InclusiveBetween(1, 80)
                .WithMessage("Workload hours must be between 1 and 80");
        });
    }
}
