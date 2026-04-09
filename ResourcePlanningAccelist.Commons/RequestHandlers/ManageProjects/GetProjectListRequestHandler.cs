using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Commons.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjects;

public class GetProjectListRequestHandler : IRequestHandler<GetProjectListRequest, GetProjectListResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetProjectListRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetProjectListResponse> Handle(GetProjectListRequest request, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(request.PageNumber ?? PaginationDefaults.PageNumber, PaginationDefaults.PageNumber);
        var requestedPageSize = request.PageSize ?? PaginationDefaults.PageSize;
        var pageSize = Math.Clamp(requestedPageSize, 1, PaginationDefaults.MaxPageSize);

        var query = _dbContext.Projects.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(project => project.Status.ToString() == request.Status);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var projects = await query
            .OrderByDescending(project => project.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(project => new ProjectListItemResponse
            {
                Id = project.Id,
                Name = project.Name,
                Status = project.Status.ToString(),
                StartDate = project.StartDate,
                EndDate = project.EndDate
            })
            .ToListAsync(cancellationToken);

        return new GetProjectListResponse
        {
            Projects = projects,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}