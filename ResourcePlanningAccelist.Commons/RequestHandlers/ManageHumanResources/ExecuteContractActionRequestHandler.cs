using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageHumanResources;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageHumanResources;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageHumanResources;

public class ExecuteContractActionRequestHandler : IRequestHandler<ExecuteContractActionRequest, ExecuteContractActionResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public ExecuteContractActionRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExecuteContractActionResponse> Handle(ExecuteContractActionRequest request, CancellationToken cancellationToken)
    {
        var decision = await _dbContext.GmDecisions
            .Include(d => d.AffectedEmployees)
                .ThenInclude(ae => ae.Employee)
                    .ThenInclude(e => e.User)
            .FirstOrDefaultAsync(item => item.Id == request.DecisionId, cancellationToken);

        if (decision == null)
        {
            return new ExecuteContractActionResponse
            {
                Success = false,
                Message = "Contract decision not found."
            };
        }

        foreach (var affected in decision.AffectedEmployees)
        {
            var employee = affected.Employee;
            if (employee == null) continue;

            // Get current active or extended contract (the most recent one)
            var currentContract = await _dbContext.EmployeeContracts
                .Where(c => c.EmployeeId == employee.Id && (c.Status == ContractStatus.Active || c.Status == ContractStatus.Extended))
                .OrderByDescending(c => c.EndDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (decision.DecisionType == DecisionType.TerminateContract)
            {
                // 1. Update Employee Status
                employee.Status = EmploymentStatus.Terminated;

                // 2. Deactivate User account
                if (employee.User != null)
                {
                    employee.User.IsActive = false;
                }

                // 3. Update Contract
                if (currentContract != null)
                {
                    currentContract.Status = ContractStatus.Terminated;
                    currentContract.EndDate = DateOnly.FromDateTime(DateTime.Today);
                }
            }
            else if (decision.DecisionType == DecisionType.ExtendContract)
            {
                if (currentContract != null)
                {
                    // 1. Mark existing as Extended (if it was Active)
                    if (currentContract.Status == ContractStatus.Active)
                    {
                        currentContract.Status = ContractStatus.Extended;
                    }

                    // 2. Create New Contract (+1 Year)
                    var newStartDate = currentContract.EndDate ?? DateOnly.FromDateTime(DateTime.Today);
                    var newContract = new EmployeeContract
                    {
                        Id = new Guid(),
                        EmployeeId = employee.Id,
                        StartDate = newStartDate,
                        EndDate = newStartDate.AddYears(1),
                        Status = ContractStatus.Active,
                        CurrentProjectId = currentContract.CurrentProjectId,
                        Notes = $"Extension based on GM Decision: {decision.Title}"
                    };

                    _dbContext.EmployeeContracts.Add(newContract);
                }
            }
        }

        decision.Status = DecisionStatus.Executed;
        decision.ExecutedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ExecuteContractActionResponse
        {
            Success = true,
            Message = $"Contract action ({decision.DecisionType}) executed successfully for {decision.AffectedEmployees.Count} employees."
        };
    }
}
