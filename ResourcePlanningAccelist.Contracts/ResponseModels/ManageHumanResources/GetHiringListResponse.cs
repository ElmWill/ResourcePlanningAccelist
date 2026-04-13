using System;
using System.Collections.Generic;

namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageHumanResources;

public class GetHiringListResponse
{
    public List<HiringRequestItemResponse> HiringRequests { get; set; } = new();
}

public class HiringRequestItemResponse
{
    public Guid Id { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; 
    public DateTimeOffset StartedAt { get; set; }
}
