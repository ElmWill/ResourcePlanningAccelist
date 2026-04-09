using FluentValidation;
using ResourcePlanningAccelist.Commons.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;

namespace ResourcePlanningAccelist.Commons.Validators.ManageProjectManager;

public class GetProjectManagerProjectActivityRequestValidator : AbstractValidator<GetProjectManagerProjectActivityRequest>
{
    public GetProjectManagerProjectActivityRequestValidator()
    {
        RuleFor(request => request.PmUserId)
            .NotEmpty();

        RuleFor(request => request.ProjectId)
            .NotEmpty();

        RuleFor(request => request.Limit)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(PaginationDefaults.MaxPageSize)
            .When(request => request.Limit.HasValue);
    }
}