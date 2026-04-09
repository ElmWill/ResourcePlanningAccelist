using FluentValidation;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;

namespace ResourcePlanningAccelist.Commons.Validators.ManageEmployees;

public class UpdateEmployeeStatusRequestValidator : AbstractValidator<UpdateEmployeeStatusRequest>
{
    public UpdateEmployeeStatusRequestValidator()
    {
        RuleFor(request => request.EmployeeId)
            .NotEmpty();

        RuleFor(request => request.Status)
            .NotEmpty()
            .Must(status => Enum.TryParse<EmploymentStatus>(status, true, out _))
            .WithMessage("Status must be a valid EmploymentStatus value.");
    }
}