using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;

public class UpdateEmployeeRequest : IRequest<UpdateEmployeeResponse>
{
    public Guid EmployeeId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? EmployeeCode { get; set; }

    public string? Phone { get; set; }

    public string? Location { get; set; }

    public Guid? DepartmentId { get; set; }

    public string JobTitle { get; set; } = string.Empty;

    public string Status { get; set; } = "Active";

    public DateOnly? HireDate { get; set; }
}
