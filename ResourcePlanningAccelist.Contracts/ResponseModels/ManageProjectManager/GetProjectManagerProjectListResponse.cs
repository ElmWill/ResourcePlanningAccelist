namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

public class GetProjectManagerProjectListResponse
{
    public List<ProjectManagerProjectListItemResponse> Projects { get; set; } = new();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }
}