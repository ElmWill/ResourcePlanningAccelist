using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;

namespace ResourcePlanningAccelist.Commons.Validators.ManageAssignments;

public class CreateAssignmentRequestValidator : AbstractValidator<CreateAssignmentRequest>
{
    public CreateAssignmentRequestValidator()
    {
        RuleFor(request => request.ProjectId)
            .NotEmpty();

        RuleFor(request => request.EmployeeId)
            .NotEmpty();

        RuleFor(request => request.AssignedByUserId)
            .NotEmpty();

        RuleFor(request => request.RoleName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.AllocationPercent)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);

        RuleFor(request => request.StartDate)
            .LessThanOrEqualTo(request => request.EndDate)
            .WithMessage("StartDate must be less than or equal to EndDate.");
    }
}