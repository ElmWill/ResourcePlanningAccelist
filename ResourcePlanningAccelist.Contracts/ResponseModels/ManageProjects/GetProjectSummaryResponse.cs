using System.Collections.Generic;

namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;

public class GetProjectSummaryResponse
{
    public int TotalProjects { get; set; }
    public Dictionary<string, int> ProjectsByStatus { get; set; } = new Dictionary<string, int>();
}
