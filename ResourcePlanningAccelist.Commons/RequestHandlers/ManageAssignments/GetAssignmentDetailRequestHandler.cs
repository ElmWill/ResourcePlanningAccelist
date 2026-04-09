using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageAssignments;

public class GetAssignmentDetailRequestHandler : IRequestHandler<GetAssignmentDetailRequest, GetAssignmentDetailResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetAssignmentDetailRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetAssignmentDetailResponse> Handle(GetAssignmentDetailRequest request, CancellationToken cancellationToken)
    {
        var assignment = await _dbContext.Assignments
            .AsNoTracking()
            .Include(item => item.Project)
            .Include(item => item.Employee)
                .ThenInclude(item => item.User)
            .Where(item => item.Id == request.AssignmentId)
            .Select(item => new GetAssignmentDetailResponse
            {
                Id = item.Id,
                ProjectId = item.ProjectId,
                ProjectName = item.Project.Name,
                EmployeeId = item.EmployeeId,
                EmployeeName = item.Employee.User.FullName,
                RoleName = item.RoleName,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                AllocationPercent = item.AllocationPercent,
                ProgressPercent = item.ProgressPercent,
                Status = item.Status.ToString()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return assignment ?? throw new KeyNotFoundException("Assignment not found.");
    }
}