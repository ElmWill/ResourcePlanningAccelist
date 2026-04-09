namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageLookups;

public class GetSkillListResponse
{
    public List<SkillListItemResponse> Skills { get; set; } = new();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }
}