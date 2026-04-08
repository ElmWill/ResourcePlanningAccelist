using MediatR;
using Microsoft.EntityFrameworkCore;
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
        var query = _dbContext.Projects.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(project => project.Status.ToString() == request.Status);
        }

        var projects = await query
            .OrderByDescending(project => project.CreatedAt)
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
            Projects = projects
        };
    }
}