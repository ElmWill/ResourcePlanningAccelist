using FluentValidation;
using ResourcePlanningAccelist.Commons.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;

namespace ResourcePlanningAccelist.Commons.Validators.ManageEmployees;

public class UpdateEmployeeAvailabilityRequestValidator : AbstractValidator<UpdateEmployeeAvailabilityRequest>
{
    public UpdateEmployeeAvailabilityRequestValidator()
    {
        RuleFor(request => request.EmployeeId)
            .NotEmpty();

        RuleFor(request => request.AvailabilityPercent)
            .InclusiveBetween(ValidationConstants.PercentageMin, ValidationConstants.PercentageMax);
    }
}