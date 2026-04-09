using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Commons.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjects;

public class GetProjectAssignmentsRequestHandler : IRequestHandler<GetProjectAssignmentsRequest, GetProjectAssignmentsResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetProjectAssignmentsRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetProjectAssignmentsResponse> Handle(GetProjectAssignmentsRequest request, CancellationToken cancellationToken)
    {
        var projectExists = await _dbContext.Projects.AnyAsync(item => item.Id == request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            throw new KeyNotFoundException("Project not found.");
        }

        var pageNumber = Math.Max(request.PageNumber ?? PaginationDefaults.PageNumber, PaginationDefaults.PageNumber);
        var requestedPageSize = request.PageSize ?? PaginationDefaults.PageSize;
        var pageSize = Math.Clamp(requestedPageSize, 1, PaginationDefaults.MaxPageSize);

        var query = _dbContext.Assignments
            .AsNoTracking()
            .Where(item => item.ProjectId == request.ProjectId)
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
                AllocationPercent = item.AllocationPercent
            })
            .ToListAsync(cancellationToken);

        return new GetProjectAssignmentsResponse
        {
            ProjectId = request.ProjectId,
            Assignments = assignments,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}