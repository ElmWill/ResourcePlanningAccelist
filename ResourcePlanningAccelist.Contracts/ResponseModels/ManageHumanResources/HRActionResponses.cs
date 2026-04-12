namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageHumanResources;

public class ExecuteGmDecisionResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class ExecuteContractActionResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class StartHiringResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class UpdateHiringStageResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class RehireEmployeeResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
