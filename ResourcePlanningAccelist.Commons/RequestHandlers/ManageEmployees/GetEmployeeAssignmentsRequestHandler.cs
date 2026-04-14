using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Commons.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageEmployees;

public class GetEmployeeAssignmentsRequestHandler : IRequestHandler<GetEmployeeAssignmentsRequest, GetEmployeeAssignmentsResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetEmployeeAssignmentsRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetEmployeeAssignmentsResponse> Handle(GetEmployeeAssignmentsRequest request, CancellationToken cancellationToken)
    {
        var employee = await _dbContext.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.EmployeeId || item.UserId == request.EmployeeId, cancellationToken);
            
        if (employee == null)
        {
            throw new KeyNotFoundException("Employee not found.");
        }

        var pageNumber = Math.Max(request.PageNumber ?? PaginationDefaults.PageNumber, PaginationDefaults.PageNumber);
        var requestedPageSize = request.PageSize ?? PaginationDefaults.PageSize;
        var pageSize = Math.Clamp(requestedPageSize, 1, PaginationDefaults.MaxPageSize);

        var query = _dbContext.Assignments
            .AsNoTracking()
            .Where(item => item.EmployeeId == employee.Id)
            .Include(item => item.Project)
            .Include(item => item.Employee)
                .ThenInclude(item => item.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(item => item.Status.ToString() == request.Status);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var assignments = await query
            .OrderByDescending(item => item.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new AssignmentListItemResponse
            {
                Id = item.Id,
                ProjectId = item.ProjectId,
                ProjectName = item.Project.Name,
                EmployeeId = item.EmployeeId,
                EmployeeName = item.Employee.User.FullName,
                RoleName = item.RoleName,
                Status = item.Status.ToString(),
                ProjectStatus = item.Project.Status.ToString(),
                AllocationPercent = item.AllocationPercent
            })
            .ToListAsync(cancellationToken);

        return new GetEmployeeAssignmentsResponse
        {
            EmployeeId = request.EmployeeId,
            Assignments = assignments,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}