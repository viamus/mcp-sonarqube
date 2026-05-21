using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Viamus.Sonarqube.Mcp.Server.Configuration;
using Viamus.Sonarqube.Mcp.Server.Models;

namespace Viamus.Sonarqube.Mcp.Server.Services;

public class SonarQubeClient(HttpClient httpClient, IOptions<SonarQubeSettings>? settings = null) : ISonarQubeClient
{
    private readonly string? organization = NormalizeOrganization(settings?.Value.Organization);

    public async Task<ProjectSearchResponse> SearchProjectsAsync(
        string? query, int? page, int? pageSize, CancellationToken cancellationToken)
    {
        var parameters = new List<string>();
        if (!string.IsNullOrWhiteSpace(query)) parameters.Add($"q={Uri.EscapeDataString(query)}");
        if (page.HasValue) parameters.Add($"p={page.Value}");
        if (pageSize.HasValue) parameters.Add($"ps={pageSize.Value}");

        var url = BuildUrl("/api/projects/search", parameters);
        var response = await httpClient.GetAsync(url, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<ProjectSearchResponse>(cancellationToken))!;
    }

    public async Task<IssueSearchResponse> SearchIssuesAsync(
        string? projectKey, string? severities, string? statuses,
        string? types, string? tags, int? page, int? pageSize,
        string? pullRequest,
        CancellationToken cancellationToken)
    {
        var parameters = new List<string>();
        if (!string.IsNullOrWhiteSpace(projectKey)) parameters.Add($"projects={Uri.EscapeDataString(projectKey)}");
        if (!string.IsNullOrWhiteSpace(severities)) parameters.Add($"severities={Uri.EscapeDataString(severities)}");
        if (!string.IsNullOrWhiteSpace(statuses)) parameters.Add($"statuses={Uri.EscapeDataString(statuses)}");
        if (!string.IsNullOrWhiteSpace(types)) parameters.Add($"types={Uri.EscapeDataString(types)}");
        if (!string.IsNullOrWhiteSpace(tags)) parameters.Add($"tags={Uri.EscapeDataString(tags)}");
        if (!string.IsNullOrWhiteSpace(pullRequest)) parameters.Add($"pullRequest={Uri.EscapeDataString(pullRequest)}");
        if (page.HasValue) parameters.Add($"p={page.Value}");
        if (pageSize.HasValue) parameters.Add($"ps={pageSize.Value}");

        var url = BuildUrl("/api/issues/search", parameters);
        var response = await httpClient.GetAsync(url, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<IssueSearchResponse>(cancellationToken))!;
    }

    public async Task<QualityGateProjectStatusResponse> GetQualityGateProjectStatusAsync(
        string projectKey, CancellationToken cancellationToken)
    {
        var parameters = new List<string> { $"projectKey={Uri.EscapeDataString(projectKey)}" };

        var url = BuildUrl("/api/qualitygates/project_status", parameters);
        var response = await httpClient.GetAsync(url, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<QualityGateProjectStatusResponse>(cancellationToken))!;
    }

    public async Task<MeasureComponentResponse> GetMeasuresAsync(
        string component, string metricKeys, CancellationToken cancellationToken)
    {
        var parameters = new List<string>
        {
            $"component={Uri.EscapeDataString(component)}",
            $"metricKeys={Uri.EscapeDataString(metricKeys)}"
        };

        var url = BuildUrl("/api/measures/component", parameters);
        var response = await httpClient.GetAsync(url, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<MeasureComponentResponse>(cancellationToken))!;
    }

    public async Task<ComponentTreeMeasuresResponse> GetComponentTreeMeasuresAsync(
        string component, string metricKeys,
        string? pullRequest, string? qualifiers,
        string? sort, bool? asc,
        int? page, int? pageSize,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(sort))
        {
            var requested = metricKeys
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (!requested.Contains(sort, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Sort metric '{sort}' must be included in metricKeys ('{metricKeys}').",
                    nameof(sort));
            }
        }

        var parameters = new List<string>
        {
            $"component={Uri.EscapeDataString(component)}",
            $"metricKeys={Uri.EscapeDataString(metricKeys)}"
        };
        if (!string.IsNullOrWhiteSpace(pullRequest)) parameters.Add($"pullRequest={Uri.EscapeDataString(pullRequest)}");
        if (!string.IsNullOrWhiteSpace(qualifiers)) parameters.Add($"qualifiers={Uri.EscapeDataString(qualifiers)}");
        if (!string.IsNullOrWhiteSpace(sort))
        {
            // Sonar's `s` only accepts metric|metricPeriod|name|path|qualifier — to sort by a
            // specific metric, set s=metric (or metricPeriod for new-code) and pass the key via metricSort.
            var isNewCodeMetric = sort.StartsWith("new_", StringComparison.OrdinalIgnoreCase);
            parameters.Add($"s={(isNewCodeMetric ? "metricPeriod" : "metric")}");
            parameters.Add($"metricSort={Uri.EscapeDataString(sort)}");
            if (isNewCodeMetric) parameters.Add("metricPeriodSort=1");
            parameters.Add($"asc={(asc ?? false).ToString().ToLowerInvariant()}");
        }
        if (page.HasValue) parameters.Add($"p={page.Value}");
        if (pageSize.HasValue) parameters.Add($"ps={pageSize.Value}");

        var url = BuildUrl("/api/measures/component_tree", parameters);
        var response = await httpClient.GetAsync(url, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<ComponentTreeMeasuresResponse>(cancellationToken))!;
    }

    public async Task<DuplicationsShowResponse> GetDuplicationsAsync(
        string fileKey, string? pullRequest,
        CancellationToken cancellationToken)
    {
        var parameters = new List<string> { $"key={Uri.EscapeDataString(fileKey)}" };
        if (!string.IsNullOrWhiteSpace(pullRequest)) parameters.Add($"pullRequest={Uri.EscapeDataString(pullRequest)}");

        var url = BuildUrl("/api/duplications/show", parameters);
        var response = await httpClient.GetAsync(url, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<DuplicationsShowResponse>(cancellationToken))!;
    }

    public async Task<HotspotSearchResponse> SearchHotspotsAsync(
        string projectKey, string? status, int? page, int? pageSize,
        CancellationToken cancellationToken)
    {
        var parameters = new List<string> { $"projectKey={Uri.EscapeDataString(projectKey)}" };
        if (!string.IsNullOrWhiteSpace(status)) parameters.Add($"status={Uri.EscapeDataString(status)}");
        if (page.HasValue) parameters.Add($"p={page.Value}");
        if (pageSize.HasValue) parameters.Add($"ps={pageSize.Value}");

        var url = BuildUrl("/api/hotspots/search", parameters);
        var response = await httpClient.GetAsync(url, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<HotspotSearchResponse>(cancellationToken))!;
    }

    public async Task<HotspotDetailResponse> GetHotspotAsync(
        string hotspotKey, CancellationToken cancellationToken)
    {
        var parameters = new List<string> { $"hotspot={Uri.EscapeDataString(hotspotKey)}" };

        var url = BuildUrl("/api/hotspots/show", parameters);
        var response = await httpClient.GetAsync(url, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<HotspotDetailResponse>(cancellationToken))!;
    }

    public async Task<QualityGateListResponse> ListQualityGatesAsync(CancellationToken cancellationToken)
    {
        var parameters = new List<string>();

        var response = await httpClient.GetAsync(BuildUrl("/api/qualitygates/list", parameters), cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<QualityGateListResponse>(cancellationToken))!;
    }

    public async Task<SystemHealthResponse> GetSystemHealthAsync(CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync("/api/system/health", cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<SystemHealthResponse>(cancellationToken))!;
    }

    public async Task<RuleSearchResponse> SearchRulesAsync(
        string? languages, string? severities, string? tags,
        string? query, int? page, int? pageSize,
        CancellationToken cancellationToken)
    {
        var parameters = new List<string>();
        if (!string.IsNullOrWhiteSpace(languages)) parameters.Add($"languages={Uri.EscapeDataString(languages)}");
        if (!string.IsNullOrWhiteSpace(severities)) parameters.Add($"severities={Uri.EscapeDataString(severities)}");
        if (!string.IsNullOrWhiteSpace(tags)) parameters.Add($"tags={Uri.EscapeDataString(tags)}");
        if (!string.IsNullOrWhiteSpace(query)) parameters.Add($"q={Uri.EscapeDataString(query)}");
        if (page.HasValue) parameters.Add($"p={page.Value}");
        if (pageSize.HasValue) parameters.Add($"ps={pageSize.Value}");

        var url = BuildUrl("/api/rules/search", parameters);
        var response = await httpClient.GetAsync(url, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<RuleSearchResponse>(cancellationToken))!;
    }

    public async Task<PullRequestAnalysisResponse> GetPullRequestAnalysisAsync(
        string projectKey, string pullRequest, string? metricKeys,
        CancellationToken cancellationToken)
    {
        var encodedKey = Uri.EscapeDataString(projectKey);
        var encodedPr = Uri.EscapeDataString(pullRequest);
        var metrics = string.IsNullOrWhiteSpace(metricKeys)
            ? "new_coverage,new_duplicated_lines_density,new_duplicated_lines,new_duplicated_blocks,new_bugs,new_vulnerabilities,new_code_smells,new_security_hotspots,new_lines,new_lines_to_cover,new_violations"
            : metricKeys;

        var qualityGateUrl = BuildUrl("/api/qualitygates/project_status", new List<string>
        {
            $"projectKey={encodedKey}",
            $"pullRequest={encodedPr}"
        });
        var measuresUrl = BuildUrl("/api/measures/component", new List<string>
        {
            $"component={encodedKey}",
            $"pullRequest={encodedPr}",
            $"metricKeys={Uri.EscapeDataString(metrics)}"
        });
        var issuesUrl = BuildUrl("/api/issues/search", new List<string>
        {
            $"projects={encodedKey}",
            $"pullRequest={encodedPr}",
            "ps=500"
        });
        var hotspotsUrl = BuildUrl("/api/hotspots/search", new List<string>
        {
            $"projectKey={encodedKey}",
            $"pullRequest={encodedPr}",
            "ps=500"
        });

        var qualityGateTask = httpClient.GetAsync(qualityGateUrl, cancellationToken);
        var measuresTask = httpClient.GetAsync(measuresUrl, cancellationToken);
        var issuesTask = httpClient.GetAsync(issuesUrl, cancellationToken);
        var hotspotsTask = httpClient.GetAsync(hotspotsUrl, cancellationToken);

        await Task.WhenAll(qualityGateTask, measuresTask, issuesTask, hotspotsTask);

        var qualityGateResponse = qualityGateTask.Result;
        var measuresResponse = measuresTask.Result;
        var issuesResponse = issuesTask.Result;
        var hotspotsResponse = hotspotsTask.Result;

        await EnsureSuccessOrThrowAsync(qualityGateResponse, cancellationToken);
        await EnsureSuccessOrThrowAsync(issuesResponse, cancellationToken);
        await EnsureSuccessOrThrowAsync(hotspotsResponse, cancellationToken);

        var qualityGate = (await qualityGateResponse.Content.ReadFromJsonAsync<QualityGateProjectStatusResponse>(cancellationToken))!;
        var issues = (await issuesResponse.Content.ReadFromJsonAsync<IssueSearchResponse>(cancellationToken))!;
        var hotspots = (await hotspotsResponse.Content.ReadFromJsonAsync<HotspotSearchResponse>(cancellationToken))!;

        MeasureComponent? measures = null;
        if (measuresResponse.IsSuccessStatusCode)
        {
            var measuresPayload = await measuresResponse.Content.ReadFromJsonAsync<MeasureComponentResponse>(cancellationToken);
            measures = measuresPayload?.Component;
        }

        return new PullRequestAnalysisResponse(
            projectKey,
            pullRequest,
            qualityGate.ProjectStatus,
            measures,
            issues,
            hotspots);
    }

    private string BuildUrl(string path, List<string> parameters)
    {
        var queryParameters = new List<string>(parameters);
        if (organization is not null)
        {
            queryParameters.Add($"organization={Uri.EscapeDataString(organization)}");
        }

        return queryParameters.Count > 0 ? $"{path}?{string.Join("&", queryParameters)}" : path;
    }

    private static string? NormalizeOrganization(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;

        string? body = null;
        try { body = await response.Content.ReadAsStringAsync(cancellationToken); }
        catch { /* body unavailable; fall through */ }

        var snippet = string.IsNullOrWhiteSpace(body)
            ? string.Empty
            : $" Body: {(body!.Length > 1000 ? body[..1000] + "…" : body)}";

        throw new HttpRequestException(
            $"SonarQube request to {response.RequestMessage?.RequestUri} failed: {(int)response.StatusCode} {response.ReasonPhrase}.{snippet}",
            inner: null,
            statusCode: response.StatusCode);
    }
}
