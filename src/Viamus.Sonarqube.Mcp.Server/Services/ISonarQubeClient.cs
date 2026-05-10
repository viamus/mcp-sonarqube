using Viamus.Sonarqube.Mcp.Server.Models;

namespace Viamus.Sonarqube.Mcp.Server.Services;

public interface ISonarQubeClient
{
    Task<ProjectSearchResponse> SearchProjectsAsync(
        string? query = null, int? page = null, int? pageSize = null, CancellationToken cancellationToken = default);

    Task<IssueSearchResponse> SearchIssuesAsync(
        string? projectKey = null, string? severities = null, string? statuses = null,
        string? types = null, string? tags = null, int? page = null, int? pageSize = null,
        string? pullRequest = null,
        CancellationToken cancellationToken = default);

    Task<QualityGateProjectStatusResponse> GetQualityGateProjectStatusAsync(
        string projectKey, CancellationToken cancellationToken = default);

    Task<MeasureComponentResponse> GetMeasuresAsync(
        string component, string metricKeys, CancellationToken cancellationToken = default);

    Task<ComponentTreeMeasuresResponse> GetComponentTreeMeasuresAsync(
        string component, string metricKeys,
        string? pullRequest = null, string? qualifiers = null,
        string? sort = null, bool? asc = null,
        int? page = null, int? pageSize = null,
        CancellationToken cancellationToken = default);

    Task<DuplicationsShowResponse> GetDuplicationsAsync(
        string fileKey, string? pullRequest = null,
        CancellationToken cancellationToken = default);

    Task<HotspotSearchResponse> SearchHotspotsAsync(
        string projectKey, string? status = null, int? page = null, int? pageSize = null,
        CancellationToken cancellationToken = default);

    Task<HotspotDetailResponse> GetHotspotAsync(
        string hotspotKey, CancellationToken cancellationToken = default);

    Task<QualityGateListResponse> ListQualityGatesAsync(CancellationToken cancellationToken = default);

    Task<SystemHealthResponse> GetSystemHealthAsync(CancellationToken cancellationToken = default);

    Task<RuleSearchResponse> SearchRulesAsync(
        string? languages = null, string? severities = null, string? tags = null,
        string? query = null, int? page = null, int? pageSize = null,
        CancellationToken cancellationToken = default);

    Task<PullRequestAnalysisResponse> GetPullRequestAnalysisAsync(
        string projectKey, string pullRequest, string? metricKeys = null,
        CancellationToken cancellationToken = default);
}
