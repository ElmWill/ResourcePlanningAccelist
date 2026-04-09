using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerPredictions;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageGeneralManagerPredictions;

public class GetGeneralManagerWorkforceSummaryRequest : IRequest<GetGeneralManagerWorkforceSummaryResponse>
{
    public Guid? DepartmentId { get; set; }

    public int? TopSkillLimit { get; set; }
}
