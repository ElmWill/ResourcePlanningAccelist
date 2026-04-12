using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageHumanResources;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageHumanResources;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageHumanResources;

public class RehireEmployeeHandler : IRequestHandler<RehireEmployeeRequest, RehireEmployeeResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public RehireEmployeeHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RehireEmployeeResponse> Handle(RehireEmployeeRequest request, CancellationToken cancellationToken)
    {
        var employee = await _dbContext.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
        {
            return new RehireEmployeeResponse
            {
                Success = false,
                Message = "Employee not found."
            };
        }

        if (employee.Status != EmploymentStatus.Terminated && employee.Status != EmploymentStatus.Resigned)
        {
             return new RehireEmployeeResponse
            {
                Success = false,
                Message = $"Employee is currently in '{employee.Status}' status and does not need rehiring."
            };
        }

        // 1. Update Employee Status & Details
        employee.Status = EmploymentStatus.Active;
        employee.JobTitle = request.JobTitle;
        employee.HireDate = request.StartDate;
        
        // 2. Re-activate User Account
        if (employee.User != null)
        {
            employee.User.IsActive = true;
        }

        // 3. Create a New Contract Record
        var now = DateTime.UtcNow;
        var contract = new EmployeeContract
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee.Id,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Notes = request.Notes ?? "Employee Rehired",
            Status = ContractStatus.Active,
            CreatedAt = now,
            CreatedBy = "73101e8c-98a8-489d-ae4d-2e549eec1d85", //hardcoded HR
            UpdatedAt = now,
            UpdatedBy = "73101e8c-98a8-489d-ae4d-2e549eec1d85"
        };
        _dbContext.EmployeeContracts.Add(contract);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RehireEmployeeResponse
        {
            Success = true,
            Message = $"{employee.User?.FullName ?? "Employee"} has been successfully rehired."
        };
    }
}
