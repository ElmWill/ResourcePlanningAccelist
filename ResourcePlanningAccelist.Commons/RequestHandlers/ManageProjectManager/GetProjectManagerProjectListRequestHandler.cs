using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Commons.Constants;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjectManager;

public class GetProjectManagerProjectListRequestHandler : IRequestHandler<GetProjectManagerProjectListRequest, GetProjectManagerProjectListResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetProjectManagerProjectListRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetProjectManagerProjectListResponse> Handle(GetProjectManagerProjectListRequest request, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(request.PageNumber ?? PaginationDefaults.PageNumber, PaginationDefaults.PageNumber);
        var requestedPageSize = request.PageSize ?? PaginationDefaults.PageSize;
        var pageSize = Math.Clamp(requestedPageSize, 1, PaginationDefaults.MaxPageSize);

        var query = _dbContext.Projects
            .AsNoTracking()
            .Where(item => item.PmOwnerUserId == request.PmUserId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ProjectStatus>(request.Status, true, out var parsedStatus))
        {
            query = query.Where(item => item.Status == parsedStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var projects = await query
            .OrderByDescending(item => item.UpdatedAt ?? item.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new ProjectManagerProjectListItemResponse
            {
                ProjectId = item.Id,
                Name = item.Name,
                ClientName = item.ClientName,
                Status = item.Status.ToString(),
                ProgressPercent = item.ProgressPercent,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                TeamSize = item.Assignments.Select(assignment => assignment.EmployeeId).Distinct().Count()
            })
            .ToListAsync(cancellationToken);

        return new GetProjectManagerProjectListResponse
        {
            Projects = projects,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}