using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;

public class UpdateEmployeeAvailabilityRequest : IRequest<UpdateEmployeeAvailabilityResponse>
{
    public Guid EmployeeId { get; set; }

    public decimal AvailabilityPercent { get; set; }
}