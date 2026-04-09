using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageEmployees;

public class UpdateEmployeeStatusRequestHandler : IRequestHandler<UpdateEmployeeStatusRequest, UpdateEmployeeStatusResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateEmployeeStatusRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateEmployeeStatusResponse> Handle(UpdateEmployeeStatusRequest request, CancellationToken cancellationToken)
    {
        var employee = await _dbContext.Employees.FirstOrDefaultAsync(item => item.Id == request.EmployeeId, cancellationToken)
            ?? throw new KeyNotFoundException("Employee not found.");

        if (!Enum.TryParse<EmploymentStatus>(request.Status, true, out var parsedStatus))
        {
            throw new InvalidOperationException("Invalid employee status.");
        }

        employee.Status = parsedStatus;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateEmployeeStatusResponse
        {
            EmployeeId = employee.Id,
            Status = employee.Status.ToString()
        };
    }
}