using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Viamus.Sonarqube.Mcp.Server.Services;

namespace Viamus.Sonarqube.Mcp.Server.Tools;

[McpServerToolType]
public class MeasureTools(ISonarQubeClient sonarQubeClient, ILogger<MeasureTools> logger)
{
    [McpServerTool, Description("Get measures/metrics for a SonarQube component. Retrieves values for specified metrics like coverage, bugs, vulnerabilities, code_smells, ncloc, etc.")]
    public async Task<string> get_measures(
        [Description("The component key (usually the project key)")] string component,
        [Description("Comma-separated list of metric keys (e.g., 'coverage,bugs,vulnerabilities,code_smells,ncloc,duplicated_lines_density')")] string metricKeys)
    {
        logger.LogInformation("Getting measures for component: {Component}, metrics: {MetricKeys}", component, metricKeys);
        var result = await sonarQubeClient.GetMeasuresAsync(component, metricKeys);
        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Get measures broken down by file or directory under a SonarQube component (wrapper of /api/measures/component_tree). Use this to locate which files concentrate metrics like new_duplicated_lines or new_uncovered_lines on a pull request.")]
    public async Task<string> get_component_tree_measures(
        [Description("Project key (or any component key) to expand")] string component,
        [Description("Comma-separated metric keys (e.g., 'new_duplicated_lines,new_uncovered_lines,new_coverage')")] string metricKeys,
        [Description("Optional pull request identifier to scope the tree to PR-changed code")] string? pullRequest = null,
        [Description("Optional comma-separated component qualifiers to return. Common: 'FIL' (files, default behavior of this tool), 'DIR' (directories), 'TRK' (project)")] string? qualifiers = "FIL",
        [Description("Optional metric key to sort by. Must be one of metricKeys. Defaults to descending order.")] string? sort = null,
        [Description("Optional sort direction. true = ascending, false = descending. Default: false (worst first when sorting on a count metric)")] bool? asc = null,
        [Description("Page number (1-based)")] int? page = null,
        [Description("Page size (1-500). Default: 50")] int? pageSize = null)
    {
        logger.LogInformation(
            "Getting component tree measures for {Component} (PR: {PullRequest}) metrics: {MetricKeys} sort: {Sort}",
            component, pullRequest ?? "-", metricKeys, sort ?? "-");
        var result = await sonarQubeClient.GetComponentTreeMeasuresAsync(
            component, metricKeys, pullRequest, qualifiers, sort, asc, page, pageSize);
        return JsonSerializer.Serialize(result);
    }
}
