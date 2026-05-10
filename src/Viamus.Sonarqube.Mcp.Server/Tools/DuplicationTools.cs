using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Viamus.Sonarqube.Mcp.Server.Services;

namespace Viamus.Sonarqube.Mcp.Server.Tools;

[McpServerToolType]
public class DuplicationTools(ISonarQubeClient sonarQubeClient, ILogger<DuplicationTools> logger)
{
    [McpServerTool, Description("Show duplication blocks for a file (wrapper of /api/duplications/show). Returns the exact line ranges duplicated and the paired files. Get the file key from get_component_tree_measures or search_issues.")]
    public async Task<string> get_duplications(
        [Description("File component key (e.g., 'my-org_my-repo:src/components/Editor.tsx'). Project keys are not accepted by Sonar — use a file key.")] string fileKey,
        [Description("Optional pull request identifier to scope to PR-changed code")] string? pullRequest = null)
    {
        logger.LogInformation("Getting duplications for {FileKey} (PR: {PullRequest})", fileKey, pullRequest ?? "-");
        var result = await sonarQubeClient.GetDuplicationsAsync(fileKey, pullRequest);
        return JsonSerializer.Serialize(result);
    }
}
