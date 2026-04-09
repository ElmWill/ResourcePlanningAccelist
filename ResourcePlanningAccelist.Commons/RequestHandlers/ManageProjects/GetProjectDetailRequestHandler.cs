using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjects;

public class GetProjectDetailRequestHandler : IRequestHandler<GetProjectDetailRequest, GetProjectDetailResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetProjectDetailRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetProjectDetailResponse> Handle(GetProjectDetailRequest request, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .AsNoTracking()
            .Where(item => item.Id == request.ProjectId)
            .Select(item => new GetProjectDetailResponse
            {
                Id = item.Id,
                Name = item.Name,
                ClientName = item.ClientName,
                Description = item.Description,
                Status = item.Status.ToString(),
                ProgressPercent = item.ProgressPercent,
                RiskLevel = item.RiskLevel.ToString(),
                StartDate = item.StartDate,
                EndDate = item.EndDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        return project ?? throw new KeyNotFoundException("Project not found.");
    }
}