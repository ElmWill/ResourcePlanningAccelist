using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;

namespace ResourcePlanningAccelist.Commons.Validators.ManageEmployees;

public class GetEmployeeWorkloadSummaryRequestValidator : AbstractValidator<GetEmployeeWorkloadSummaryRequest>
{
    public GetEmployeeWorkloadSummaryRequestValidator()
    {
        RuleFor(request => request.EmployeeId)
            .NotEmpty();
    }
}