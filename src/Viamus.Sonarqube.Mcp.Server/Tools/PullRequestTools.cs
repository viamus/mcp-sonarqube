using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Viamus.Sonarqube.Mcp.Server.Services;

namespace Viamus.Sonarqube.Mcp.Server.Tools;

[McpServerToolType]
public class PullRequestTools(ISonarQubeClient sonarQubeClient, ILogger<PullRequestTools> logger)
{
    [McpServerTool, Description("Analyze a pull request in SonarQube. Aggregates the quality gate status (pass/fail and broken conditions), new-code measures (coverage, duplications, new bugs/vulnerabilities/code smells), all issues raised on the PR, and security hotspots. Use this to check if a PR breaks the quality gate.")]
    public async Task<string> analyze_pull_request(
        [Description("Project key in SonarQube (e.g., 'my-org_my-repo')")] string projectKey,
        [Description("Pull request identifier as registered in SonarQube (typically the PR number sent during scan)")] string pullRequest,
        [Description("Optional comma-separated metric keys. Defaults to new-code metrics: new_coverage,new_duplicated_lines_density,new_duplicated_lines,new_duplicated_blocks,new_bugs,new_vulnerabilities,new_code_smells,new_security_hotspots,new_lines,new_lines_to_cover,new_violations")] string? metricKeys = null)
    {
        logger.LogInformation("Analyzing pull request {PullRequest} for project {ProjectKey}", pullRequest, projectKey);
        var result = await sonarQubeClient.GetPullRequestAnalysisAsync(projectKey, pullRequest, metricKeys);
        return JsonSerializer.Serialize(result);
    }
}
