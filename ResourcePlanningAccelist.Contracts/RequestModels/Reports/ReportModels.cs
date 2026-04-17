using MediatR;
using System;
using System.Collections.Generic;

namespace ResourcePlanningAccelist.Contracts.RequestModels.Reports;

public class GetReportDashboardMetricsRequest : IRequest<GetReportDashboardMetricsResponse>
{
    public Guid? DepartmentId { get; set; }
}

public class GetReportDashboardMetricsResponse
{
    public List<UtilizationByDeptResponse> UtilizationByDept { get; set; } = new();
    public List<SkillDistributionResponse> SkillDistribution { get; set; } = new();
    public List<ProjectCompletionResponse> ProjectCompletion { get; set; } = new();
}

public class UtilizationByDeptResponse
{
    public string Dept { get; set; } = string.Empty;
    public decimal Utilization { get; set; }
}

public class SkillDistributionResponse
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class ProjectCompletionResponse
{
    public string Month { get; set; } = string.Empty;
    public int Completed { get; set; }
    public int Total { get; set; }
}

public enum ReportExportType
{
    Utilization,
    ProjectPerformance,
    HiringForecast,
    SkillsGap
}

public class ExportReportRequest : IRequest<ExportReportResponse>
{
    public ReportExportType Type { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? DepartmentId { get; set; }
}

public class ExportReportResponse
{
    public string FileName { get; set; } = string.Empty;
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "text/csv";
}
