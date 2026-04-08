namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;

public class GetProjectListResponse
{
    public List<ProjectListItemResponse> Projects { get; set; } = new();
}