using FluentValidation;
using ResourcePlanningAccelist.Commons.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageLookups;

namespace ResourcePlanningAccelist.Commons.Validators.ManageLookups;

public class GetDepartmentListRequestValidator : AbstractValidator<GetDepartmentListRequest>
{
    public GetDepartmentListRequestValidator()
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
    }
}