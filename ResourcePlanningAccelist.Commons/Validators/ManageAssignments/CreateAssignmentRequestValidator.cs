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
            .Must(employeeId => employeeId == Guid.Empty || employeeId != Guid.Empty);

        RuleFor(request => request.AssignedByUserId)
            .NotEmpty();

        RuleFor(request => request.RoleName)
            .NotEmpty()
            .MaximumLength(200);

        RuleForEach(request => request.RequiredSkills)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.AdditionalNeeds)
            .MaximumLength(1000)
            .When(request => !string.IsNullOrWhiteSpace(request.AdditionalNeeds));

        RuleFor(request => request.AllocationPercent)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);

        RuleFor(request => request.StartDate)
            .LessThanOrEqualTo(request => request.EndDate)
            .WithMessage("StartDate must be less than or equal to EndDate.");
    }
}