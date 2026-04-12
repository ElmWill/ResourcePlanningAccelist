using System;
using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageHumanResources;
namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageHumanResources;

public class UpdateHiringStageRequest : IRequest<UpdateHiringStageResponse>
{
    public Guid HiringRequestId { get; set; }
    public string NewStatus { get; set; } = string.Empty;
}
