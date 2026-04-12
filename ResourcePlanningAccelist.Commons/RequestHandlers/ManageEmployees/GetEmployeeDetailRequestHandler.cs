using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;
using ResourcePlanningAccelist.Entities;
using ResourcePlanningAccelist.Constants;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageEmployees;

public class GetEmployeeDetailRequestHandler : IRequestHandler<GetEmployeeDetailRequest, GetEmployeeDetailResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetEmployeeDetailRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetEmployeeDetailResponse> Handle(GetEmployeeDetailRequest request, CancellationToken cancellationToken)
    {
        var employee = await _dbContext.Employees
            .AsNoTracking()
            .Include(item => item.User)
            .Include(item => item.Department)
            .Include(item => item.Assignments)
                .ThenInclude(a => a.Project)
            .Where(item => item.Id == request.EmployeeId)
            .Select(item => new GetEmployeeDetailResponse
            {
                Id = item.Id,
                UserId = item.UserId,
                FullName = item.User.FullName,
                Email = item.User.Email,
                JobTitle = item.JobTitle,
                Department = item.Department != null ? item.Department.Name : null,
                AvailabilityPercent = item.AvailabilityPercent,
                WorkloadPercent = item.WorkloadPercent,
                AssignedHours = item.AssignedHours,
                Phone = item.Phone,
                Status = item.Status.ToString(),
                ContractEndDate = item.Contracts
                    .Where(c => c.Status == ContractStatus.Active)
                    .OrderByDescending(c => c.EndDate)
                    .Select(c => c.EndDate)
                    .FirstOrDefault(),
                Assignments = item.Assignments.Where(a => a.Status == AssignmentStatus.Approved || a.Status == AssignmentStatus.Accepted || a.Status == AssignmentStatus.InProgress || a.Status == AssignmentStatus.Completed).Select(a => new AssignmentListItemResponse
                {
                    Id = a.Id,
                    ProjectId = a.ProjectId,
                    ProjectName = a.Project.Name,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = a.Employee.User.FullName,
                    RoleName = a.RoleName,
                    Status = a.Status.ToString(),
                    AllocationPercent = a.AllocationPercent,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return employee ?? throw new KeyNotFoundException("Employee not found.");
    }
}