using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;

namespace ResourcePlanningAccelist.Commons.Validators.ManageAssignments;

public class SplitAssignmentWorkloadRequestValidator : AbstractValidator<SplitAssignmentWorkloadRequest>
{
    public SplitAssignmentWorkloadRequestValidator()
    {
        RuleFor(request => request.ProjectId)
            .NotEmpty();

        RuleFor(request => request.FromEmployeeId)
            .NotEmpty();

        RuleFor(request => request.ToEmployeeId)
            .NotEmpty()
            .NotEqual(request => request.FromEmployeeId)
            .WithMessage("FromEmployeeId and ToEmployeeId must be different.");

        RuleFor(request => request.SplitAllocationPercent)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);

        RuleFor(request => request.RoleName)
            .MaximumLength(200)
            .When(request => !string.IsNullOrWhiteSpace(request.RoleName));

        RuleFor(request => request)
            .Must(request => !request.StartDate.HasValue || !request.EndDate.HasValue || request.StartDate <= request.EndDate)
            .WithMessage("StartDate must be less than or equal to EndDate.");
    }
}
