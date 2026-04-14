using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageTaskAssignments;

namespace ResourcePlanningAccelist.Commons.Validators.ManageTaskAssignments;

public class CreateTaskAssignmentRequestValidator : AbstractValidator<CreateTaskAssignmentRequest>
{
    public CreateTaskAssignmentRequestValidator()
    {
        RuleFor(r => r.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required");

        RuleFor(r => r.EmployeeId)
            .NotEmpty()
            .WithMessage("Employee ID is required");

        RuleFor(r => r.TaskName)
            .NotEmpty()
            .WithMessage("Task name is required")
            .MaximumLength(500)
            .WithMessage("Task name cannot exceed 500 characters");

        RuleFor(r => r.DueDate)
            .NotEmpty()
            .WithMessage("Due date is required");

        RuleFor(r => r.Priority)
            .Must(priority => new[] { "Low", "Medium", "High" }.Contains(priority, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid priority level");

        When(r => r.WorkloadHours.HasValue, () =>
        {
            RuleFor(r => r.WorkloadHours!.Value)
                .InclusiveBetween(1, 80)
                .WithMessage("Workload hours must be between 1 and 80");
        });

        RuleFor(r => r.AssignedByUserId)
            .NotEmpty()
            .WithMessage("Assigned by user ID is required");
    }
}
