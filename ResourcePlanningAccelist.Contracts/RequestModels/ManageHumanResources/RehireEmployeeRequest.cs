using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageHumanResources;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageHumanResources;

public class RehireEmployeeRequest : IRequest<RehireEmployeeResponse>
{
    public Guid EmployeeId { get; set; }
    
    public string JobTitle { get; set; } = string.Empty;
    
    public DateOnly StartDate { get; set; }
    
    public DateOnly EndDate { get; set; }
    
    public string? Notes { get; set; }
}
