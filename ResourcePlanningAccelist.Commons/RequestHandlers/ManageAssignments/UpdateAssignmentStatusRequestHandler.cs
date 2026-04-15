using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;
using ResourcePlanningAccelist.Entities;
using ResourcePlanningAccelist.Commons.Helpers;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageAssignments;

public class UpdateAssignmentStatusRequestHandler : IRequestHandler<UpdateAssignmentStatusRequest, UpdateAssignmentStatusResponse>
{
    private readonly ApplicationDbContext _dbContext;

    private static readonly AssignmentStatus[] ActiveAssignmentStatuses =
    {
        AssignmentStatus.Pending,
        AssignmentStatus.Approved,
        AssignmentStatus.Accepted,
        AssignmentStatus.InProgress,
    };

    public UpdateAssignmentStatusRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateAssignmentStatusResponse> Handle(UpdateAssignmentStatusRequest request, CancellationToken cancellationToken)
    {
        var assignment = await _dbContext.Assignments.FirstOrDefaultAsync(item => item.Id == request.AssignmentId, cancellationToken)
            ?? throw new KeyNotFoundException("Assignment not found.");

        var previousStatus = assignment.Status;

        if (!Enum.TryParse<AssignmentStatus>(request.Status, true, out var parsedStatus))
        {
            throw new InvalidOperationException("Invalid assignment status.");
        }

        assignment.Status = parsedStatus;

        if (parsedStatus == AssignmentStatus.Accepted)
        {
            assignment.AcceptedAt = DateTimeOffset.UtcNow;
        }

        if (parsedStatus == AssignmentStatus.Rejected)
        {
            assignment.RejectedAt = DateTimeOffset.UtcNow;

            if (previousStatus == AssignmentStatus.Pending)
            {
                var project = await _dbContext.Projects.FirstOrDefaultAsync(item => item.Id == assignment.ProjectId, cancellationToken);

                if (project is not null && project.TotalRequiredResources > 0)
                {
                    var activeDistinctMembersAfterRejection = await _dbContext.Assignments
                        .AsNoTracking()
                        .Where(item => item.ProjectId == assignment.ProjectId)
                        .Where(item => item.Id != assignment.Id)
                        .Where(item => ActiveAssignmentStatuses.Contains(item.Status))
                        .Select(item => item.EmployeeId)
                        .Distinct()
                        .CountAsync(cancellationToken);

                    var rolledBackRequiredResources = project.TotalRequiredResources - 1;
                    project.TotalRequiredResources = Math.Max(activeDistinctMembersAfterRejection, rolledBackRequiredResources);
                }
            }
        }

        // SAVE FIRST so the recalculation query can see the new status
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Check for Project Kickoff (Promotion to InProgress)
        // If all assignments are now Accepted/InProgress/Completed, move project to InProgress
        if (parsedStatus == AssignmentStatus.Accepted)
        {
            var project = await _dbContext.Projects
                .Include(p => p.Assignments)
                .FirstOrDefaultAsync(p => p.Id == assignment.ProjectId, cancellationToken);

            if (project != null && (project.Status == ProjectStatus.Assigned || project.Status == ProjectStatus.Approved))
            {
                var hasUnacceptedAssignments = project.Assignments.Any(a => 
                    a.Status == AssignmentStatus.Pending || 
                    a.Status == AssignmentStatus.GmApproved || 
                    a.Status == AssignmentStatus.Approved
                );

                if (!hasUnacceptedAssignments)
                {
                    project.Status = ProjectStatus.InProgress;
                    // Optional: If progress was null/zero, initialize it
                    if (project.ProgressPercent == 0) project.ProgressPercent = 5; 
                }
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await AssignmentWorkloadUpdater.RecalculateEmployeeWorkloadAsync(_dbContext, assignment.EmployeeId, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateAssignmentStatusResponse
        {
            AssignmentId = assignment.Id,
            Status = assignment.Status.ToString()
        };
    }
}