using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageEmployees;

public class DeleteEmployeeHandler : IRequestHandler<DeleteEmployeeRequest, DeleteEmployeeResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public DeleteEmployeeHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DeleteEmployeeResponse> Handle(DeleteEmployeeRequest request, CancellationToken cancellationToken)
    {
        var employee = await _dbContext.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(item => item.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
        {
            return new DeleteEmployeeResponse
            {
                Success = false,
                Message = "Employee not found."
            };
        }

        // Perform soft delete as requested: change status to terminated
        employee.Status = EmploymentStatus.Terminated;
        
        // Deactivate the associated user account as well
        if (employee.User != null)
        {
            employee.User.IsActive = false;
        }

        // Terminate any active contracts
        var activeContracts = await _dbContext.EmployeeContracts
            .Where(c => c.EmployeeId == employee.Id && (c.Status == ContractStatus.Active || c.Status == ContractStatus.Extended))
            .ToListAsync(cancellationToken);

        foreach (var contract in activeContracts)
        {
            contract.Status = ContractStatus.Terminated;
            contract.EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteEmployeeResponse
        {
            Success = true,
            Message = "Employee status successfully changed to Terminated and user account deactivated."
        };
    }
}
