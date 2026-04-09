namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;

public class GetProjectListResponse
{
    public List<ProjectListItemResponse> Projects { get; set; } = new();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }
}