using System.Text.Json.Serialization;

namespace Viamus.Sonarqube.Mcp.Server.Models;

public record PullRequestAnalysisResponse(
    [property: JsonPropertyName("projectKey")] string ProjectKey,
    [property: JsonPropertyName("pullRequest")] string PullRequest,
    [property: JsonPropertyName("qualityGate")] ProjectQualityGateStatus QualityGate,
    [property: JsonPropertyName("measures")] MeasureComponent? Measures,
    [property: JsonPropertyName("issues")] IssueSearchResponse Issues,
    [property: JsonPropertyName("hotspots")] HotspotSearchResponse Hotspots
);
