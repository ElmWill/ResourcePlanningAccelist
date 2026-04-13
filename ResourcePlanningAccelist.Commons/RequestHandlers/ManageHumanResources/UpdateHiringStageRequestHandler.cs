using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageHumanResources;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageHumanResources;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageHumanResources;

public class UpdateHiringStageRequestHandler : IRequestHandler<UpdateHiringStageRequest, UpdateHiringStageResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateHiringStageRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateHiringStageResponse> Handle(UpdateHiringStageRequest request, CancellationToken cancellationToken)
    {
        var hiringRequest = await _dbContext.HiringRequests
            .FirstOrDefaultAsync(item => item.Id == request.HiringRequestId, cancellationToken);

        if (hiringRequest == null)
        {
            return new UpdateHiringStageResponse
            {
                Success = false,
                Message = "Hiring request not found."
            };
        }

        if (Enum.TryParse<HiringRequestStatus>(request.NewStatus, out var nextStatus))
        {
            hiringRequest.Status = nextStatus;

            if (nextStatus == HiringRequestStatus.Completed)
            {
                hiringRequest.CompletedAt = DateTimeOffset.UtcNow;
            }
        }
        else
        {
            return new UpdateHiringStageResponse
            {
                Success = false,
                Message = $"Invalid hiring status: {request.NewStatus}"
            };
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateHiringStageResponse
        {
            Success = true,
            Message = $"Hiring request moved to {hiringRequest.Status}."
        };
    }
}
