using FluentValidation;
using ResourcePlanningAccelist.Commons.Constants;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;

namespace ResourcePlanningAccelist.Commons.Validators.ManageProjects;

public class GetProjectAssignmentsRequestValidator : AbstractValidator<GetProjectAssignmentsRequest>
{
    public GetProjectAssignmentsRequestValidator()
    {
        RuleFor(request => request.ProjectId)
            .NotEmpty();

        RuleFor(request => request.PageNumber)
            .GreaterThanOrEqualTo(1)
            .When(request => request.PageNumber.HasValue);

        RuleFor(request => request.PageSize)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(PaginationDefaults.MaxPageSize)
            .When(request => request.PageSize.HasValue);

        RuleFor(request => request.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) || Enum.TryParse<AssignmentStatus>(status, true, out _))
            .WithMessage("Status must be a valid AssignmentStatus value.");
    }
}