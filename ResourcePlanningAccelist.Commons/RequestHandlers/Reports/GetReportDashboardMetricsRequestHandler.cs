using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.Reports;
using ResourcePlanningAccelist.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.Reports;

public class GetReportDashboardMetricsRequestHandler : IRequestHandler<GetReportDashboardMetricsRequest, GetReportDashboardMetricsResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetReportDashboardMetricsRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetReportDashboardMetricsResponse> Handle(GetReportDashboardMetricsRequest request, CancellationToken cancellationToken)
    {
        // 1. Utilization By Department
        var utilizationQuery = _dbContext.Employees
            .AsNoTracking()
            .Where(e => e.Status == EmploymentStatus.Active && e.DepartmentId != null);

        if (request.DepartmentId.HasValue)
        {
            utilizationQuery = utilizationQuery.Where(e => e.DepartmentId == request.DepartmentId);
        }

        var utilizationByDept = await utilizationQuery
            .GroupBy(e => e.Department!.Name)
            .Select(g => new UtilizationByDeptResponse
            {
                Dept = g.Key,
                Utilization = Math.Round(g.Average(e => e.WorkloadPercent), 2)
            })
            .ToListAsync(cancellationToken);

        // 2. Skill Distribution
        var skillQuery = _dbContext.EmployeeSkills.AsNoTracking();

        if (request.DepartmentId.HasValue)
        {
            skillQuery = skillQuery.Where(es => es.Employee.DepartmentId == request.DepartmentId);
        }

        var skillDistribution = await skillQuery
            .GroupBy(es => es.Skill.Name)
            .Select(g => new SkillDistributionResponse
            {
                Name = g.Key,
                Value = g.Select(es => es.EmployeeId).Distinct().Count()
            })
            .OrderByDescending(s => s.Value)
            .Take(10) // Top 10 skills
            .ToListAsync(cancellationToken);

        // Assign colors to skills for the pie chart
        var colors = new[] { "#3B82F6", "#10B981", "#F59E0B", "#8B5CF6", "#EF4444", "#6366F1", "#EC4899", "#8B5CF6", "#14B8A6", "#F97316" };
        for (int i = 0; i < skillDistribution.Count; i++)
        {
            skillDistribution[i].Color = colors[i % colors.Length];
        }

        // 3. Project Completion (Last 6 Months Trend)
        var today = DateOnly.FromDateTime(DateTime.Today);
        var sixMonthsAgo = today.AddMonths(-5);
        sixMonthsAgo = new DateOnly(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

        var projects = await _dbContext.Projects
            .AsNoTracking()
            .Where(p => p.EndDate >= sixMonthsAgo)
            .Select(p => new { p.EndDate, p.Status })
            .ToListAsync(cancellationToken);

        var projectCompletion = new List<ProjectCompletionResponse>();
        for (int i = 0; i < 6; i++)
        {
            var targetMonth = sixMonthsAgo.AddMonths(i);
            var monthLabel = targetMonth.ToString("MMM");
            
            var monthProjects = projects.Where(p => p.EndDate.Year == targetMonth.Year && p.EndDate.Month == targetMonth.Month).ToList();
            
            projectCompletion.Add(new ProjectCompletionResponse
            {
                Month = monthLabel,
                Total = monthProjects.Count,
                Completed = monthProjects.Count(p => p.Status == ProjectStatus.Completed)
            });
        }

        return new GetReportDashboardMetricsResponse
        {
            UtilizationByDept = utilizationByDept,
            SkillDistribution = skillDistribution,
            ProjectCompletion = projectCompletion
        };
    }
}
