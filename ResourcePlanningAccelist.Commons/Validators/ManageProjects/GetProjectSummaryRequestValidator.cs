using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;

namespace ResourcePlanningAccelist.Commons.Validators.ManageProjects;

public class GetProjectSummaryRequestValidator : AbstractValidator<GetProjectSummaryRequest>
{
    public GetProjectSummaryRequestValidator()
    {
        // No specific validation required since this request doesn't accept parameters
    }
}
