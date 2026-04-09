using FluentValidation;
using ResourcePlanningAccelist.Commons.Constants;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageLookups;

namespace ResourcePlanningAccelist.Commons.Validators.ManageLookups;

public class GetSkillListRequestValidator : AbstractValidator<GetSkillListRequest>
{
    public GetSkillListRequestValidator()
    {
        RuleFor(request => request.Query)
            .MaximumLength(ValidationConstants.DepartmentSearchMaxLength);

        RuleFor(request => request.PageNumber)
            .GreaterThanOrEqualTo(1)
            .When(request => request.PageNumber.HasValue);

        RuleFor(request => request.PageSize)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(PaginationDefaults.MaxPageSize)
            .When(request => request.PageSize.HasValue);

        RuleFor(request => request.Category)
            .Must(category => string.IsNullOrWhiteSpace(category) || Enum.TryParse<SkillCategory>(category, true, out _))
            .WithMessage("Category must be a valid SkillCategory value.");
    }
}