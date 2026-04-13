using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;

public class CreateEmployeeRequest : IRequest<CreateEmployeeResponse>
{
    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? EmployeeCode { get; set; }

    public string? Phone { get; set; }

    public string? Location { get; set; }

    public Guid? DepartmentId { get; set; }

    public string JobTitle { get; set; } = string.Empty;

    public string Status { get; set; } = "Active";

    public DateOnly? HireDate { get; set; }
    public List<string> Skills { get; set; } = new();
}
