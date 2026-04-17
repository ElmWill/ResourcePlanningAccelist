using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.Reports;
using ResourcePlanningAccelist.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.Reports;

public class ExportReportRequestHandler : IRequestHandler<ExportReportRequest, ExportReportResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public ExportReportRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExportReportResponse> Handle(ExportReportRequest request, CancellationToken cancellationToken)
    {
        string fileName;
        string csvContent;

        switch (request.Type)
        {
            case ReportExportType.Utilization:
                fileName = $"Resource_Utilization_{DateTime.Now:yyyyMMdd}.csv";
                csvContent = await GenerateUtilizationCsv(cancellationToken);
                break;
            case ReportExportType.ProjectPerformance:
                fileName = $"Project_Performance_{DateTime.Now:yyyyMMdd}.csv";
                csvContent = await GenerateProjectPerformanceCsv(cancellationToken);
                break;
            case ReportExportType.HiringForecast:
                fileName = $"Hiring_Forecast_{DateTime.Now:yyyyMMdd}.csv";
                csvContent = await GenerateHiringForecastCsv(cancellationToken);
                break;
            case ReportExportType.SkillsGap:
                fileName = $"Skills_Gap_Analysis_{DateTime.Now:yyyyMMdd}.csv";
                csvContent = await GenerateSkillsGapCsv(cancellationToken);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return new ExportReportResponse
        {
            FileName = fileName,
            FileContent = Encoding.UTF8.GetBytes(csvContent),
            ContentType = "text/csv"
        };
    }

    private async Task<string> GenerateUtilizationCsv(CancellationToken cancellationToken)
    {
        var data = await _dbContext.Employees
            .AsNoTracking()
            .Where(e => e.Status == EmploymentStatus.Active)
            .Select(e => new
            {
                Employee = e.User.FullName,
                Department = e.Department != null ? e.Department.Name : "N/A",
                e.JobTitle,
                e.WorkloadPercent,
                e.AvailabilityPercent,
                // Sum actual assigned hours only from ACTIVE project assignments
                LiveAssignedHours = e.Assignments
                    .Where(a => a.Status != AssignmentStatus.Rejected && a.Status != AssignmentStatus.Cancelled)
                    .Where(a => a.Project.Status != ProjectStatus.Completed && a.Project.Status != ProjectStatus.Cancelled)
                    .Sum(a => a.AllocationPercent * 0.4m), // Assuming 100% allocated = 40 hours
                Skills = e.EmployeeSkills.Select(es => es.Skill.Name).ToList(),
                Projects = e.Assignments
                    .Where(a => a.Status != AssignmentStatus.Rejected && a.Status != AssignmentStatus.Cancelled)
                    .Where(a => a.Project.Status != ProjectStatus.Completed && a.Project.Status != ProjectStatus.Cancelled)
                    .Select(a => a.Project.Name)
                    .Distinct()
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Resource,Role & Department,Skills,Allocated Hours (Week),Availability,Workload (%),Current Projects,Status");
        foreach (var item in data)
        {
            // Calculate everything from the same live source for 100% consistency
            var liveWorkloadPercent = item.LiveAssignedHours / 0.4m; // 40h = 100%
            var liveAvailabilityPercent = Math.Max(0, 100 - liveWorkloadPercent);
            
            var status = liveWorkloadPercent > 100 ? "OVERLOADED" : liveWorkloadPercent > 70 ? "BUSY" : "AVAILABLE";
            var projects = string.Join(" | ", item.Projects);
            var skills = string.Join(" | ", item.Skills);
            
            sb.AppendLine($"\"{item.Employee}\",\"{item.JobTitle} ({item.Department})\",\"{skills}\",\"{item.LiveAssignedHours:F2}h / 40h\",{liveAvailabilityPercent:F0}%,{liveWorkloadPercent:F0}%,\"{projects}\",\"{status}\"");
        }
        return sb.ToString();
    }

    private async Task<string> GenerateProjectPerformanceCsv(CancellationToken cancellationToken)
    {
        var data = await _dbContext.Projects
            .AsNoTracking()
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.StartDate,
                p.EndDate,
                ActualEndDate = p.Status == ProjectStatus.Completed ? (DateTimeOffset?)p.UpdatedAt : null,
                p.ProgressPercent,
                Status = p.Status.ToString(),
                Resources = p.Assignments
                    .Where(a => a.Status != AssignmentStatus.Rejected && a.Status != AssignmentStatus.Cancelled)
                    .Select(a => a.Employee.User.FullName)
                    .Distinct()
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("project id,project name,start date,end date,actual end date,progress %,status,resources");
        foreach (var item in data)
        {
            var actualEnd = item.ActualEndDate.HasValue ? item.ActualEndDate.Value.ToString("yyyy-MM-dd") : "N/A";
            var resources = string.Join(" | ", item.Resources);
            sb.AppendLine($"\"{item.Id}\",\"{item.Name}\",{item.StartDate:yyyy-MM-dd},{item.EndDate:yyyy-MM-dd},{actualEnd},{item.ProgressPercent}%,\"{item.Status}\",\"{resources}\"");
        }
        return sb.ToString();
    }

    private async Task<string> GenerateHiringForecastCsv(CancellationToken cancellationToken)
    {
        // 1. Build Role -> Primary Dept map from existing employees
        var employees = await _dbContext.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.EmployeeSkills).ThenInclude(es => es.Skill)
            .Where(e => e.Status == EmploymentStatus.Active)
            .ToListAsync(cancellationToken);

        var roleToDeptMap = employees
            .GroupBy(e => e.JobTitle.ToLower().Trim())
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(e => e.Department?.Name ?? "General")
                      .OrderByDescending(deptGroup => deptGroup.Count())
                      .First().Key
            );

        // 2. Get all active requirements
        var requirements = await _dbContext.ProjectResourceRequirements
            .AsNoTracking()
            .Include(r => r.Project)
            .Include(r => r.RequiredSkills).ThenInclude(rs => rs.Skill)
            .Where(r => r.Project.Status != ProjectStatus.Completed && r.Project.Status != ProjectStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var gapRequirements = new List<dynamic>();

        foreach (var req in requirements)
        {
            var reqRole = req.RoleName.ToLower().Trim();
            var requiredSkillIds = req.RequiredSkills.Select(rs => rs.SkillId).ToList();

            // Find matching available employees (Workload <= 70% and has ALL required skills)
            var availableResources = employees
                .Where(e => e.JobTitle.ToLower().Trim() == reqRole)
                .Where(e => e.WorkloadPercent <= 70)
                .Where(e => requiredSkillIds.All(rsId => e.EmployeeSkills.Any(es => es.SkillId == rsId)))
                .Count();

            int gap = Math.Max(0, req.Quantity - availableResources);
            if (gap > 0)
            {
                var skillsList = string.Join(" | ", req.RequiredSkills.Select(rs => rs.Skill.Name).OrderBy(n => n));
                var targetDept = roleToDeptMap.TryGetValue(reqRole, out var d) ? d : "General";
                
                gapRequirements.Add(new {
                    Dept = targetDept,
                    Role = req.RoleName,
                    Skills = string.IsNullOrEmpty(skillsList) ? "General Profile" : skillsList,
                    GapCount = gap,
                    StartDate = req.Project.StartDate,
                    ProjectName = req.Project.Name
                });
            }
        }

        // 3. Aggregate Gaps by Dept + Role + Skills for a clean report
        var finalGaps = gapRequirements
            .GroupBy(g => new { g.Dept, g.Role, g.Skills })
            .Select(g => new
            {
                g.Key.Dept,
                g.Key.Role,
                g.Key.Skills,
                TotalGap = g.Sum(x => (int)x.GapCount),
                EarliestStart = g.Min(x => (DateOnly)x.StartDate),
                Projects = string.Join(" | ", g.Select(x => (string)x.ProjectName).Distinct())
            })
            .OrderBy(x => x.Dept).ThenBy(x => x.Role)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("department name,role,required skills,gap count,forecast date,contributing projects");
        
        foreach (var gap in finalGaps)
        {
            var forecastDate = gap.EarliestStart.ToDateTime(TimeOnly.MinValue).AddDays(-15).ToString("yyyy-MM-dd");
            sb.AppendLine($"\"{gap.Dept}\",\"{gap.Role}\",\"{gap.Skills}\",{gap.TotalGap},{forecastDate},\"{gap.Projects}\"");
        }
        return sb.ToString();
    }

    private async Task<string> GenerateSkillsGapCsv(CancellationToken cancellationToken)
    {
        var employees = await _dbContext.Employees
            .AsNoTracking()
            .Include(e => e.User)
            .Include(e => e.EmployeeSkills).ThenInclude(es => es.Skill)
            .Include(e => e.Assignments).ThenInclude(a => a.Project).ThenInclude(p => p.ProjectSkills).ThenInclude(ps => ps.Skill)
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("employee id,employee name,current skill,required skill,gap");

        foreach (var e in employees)
        {
            var currentSkills = e.EmployeeSkills.Select(es => es.Skill.Name).ToList();
            
            // Required skills are derived from the projects they are assigned to
            var requiredSkills = e.Assignments
                .Where(a => a.Status != AssignmentStatus.Rejected && a.Status != AssignmentStatus.Cancelled)
                .SelectMany(a => a.Project.ProjectSkills)
                .Select(ps => ps.Skill.Name)
                .Distinct()
                .ToList();

            string gapStatus;
            if (!requiredSkills.Any())
            {
                gapStatus = "no requirement";
            }
            else
            {
                var missingCount = requiredSkills.Except(currentSkills).Count();
                gapStatus = missingCount == 0 ? "match" : "below requirement";
            }

            var currentStr = string.Join(" | ", currentSkills);
            var requireStr = string.Join(" | ", requiredSkills);

            sb.AppendLine($"\"{e.Id}\",\"{e.User.FullName}\",\"{currentStr}\",\"{requireStr}\",\"{gapStatus}\"");
        }
        return sb.ToString();
    }
}
