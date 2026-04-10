using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageEmployees;

public class UpdateEmployeeHandler : IRequestHandler<UpdateEmployeeRequest, UpdateEmployeeResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateEmployeeHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateEmployeeResponse> Handle(UpdateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var employee = await _dbContext.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(item => item.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
        {
            return new UpdateEmployeeResponse
            {
                Success = false,
                Message = "Employee not found."
            };
        }

        // Update User info
        employee.User.FullName = request.FullName;
        employee.User.DepartmentId = request.DepartmentId;

        // Update Employee profile
        employee.EmployeeCode = request.EmployeeCode;
        employee.Phone = request.Phone;
        employee.Location = request.Location;
        employee.DepartmentId = request.DepartmentId;
        employee.JobTitle = request.JobTitle;
        if (Enum.TryParse<EmploymentStatus>(request.Status, true, out var status))
        {
            employee.Status = status;
        }
        employee.HireDate = request.HireDate;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateEmployeeResponse
        {
            Success = true,
            Message = "Employee profile updated successfully."
        };
    }
}
