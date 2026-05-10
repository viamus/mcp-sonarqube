using System.Text.Json;
using Microsoft.Extensions.Logging;
using Viamus.Sonarqube.Mcp.Server.Models;
using Viamus.Sonarqube.Mcp.Server.Services;
using Viamus.Sonarqube.Mcp.Server.Tools;

namespace Viamus.Sonarqube.Mcp.Server.Tests.Tools;

public class PullRequestToolsTests
{
    private readonly ISonarQubeClient _client = Substitute.For<ISonarQubeClient>();
    private readonly PullRequestTools _tools;

    public PullRequestToolsTests()
    {
        _tools = new PullRequestTools(_client, Substitute.For<ILogger<PullRequestTools>>());
    }

    [Fact]
    public async Task AnalyzePullRequest_ShouldReturnSerializedAggregatedResponse()
    {
        var qualityGate = new ProjectQualityGateStatus(
            "ERROR",
            [new QualityGateCondition("ERROR", "new_coverage", "LT", "80", "62.5")]);

        var measures = new MeasureComponent(
            "my-project", "My Project", "TRK",
            [new Measure("new_coverage", "62.5", null)]);

        var issues = new IssueSearchResponse(
            new Paging(1, 100, 1),
            [new Issue("issue-1", "csharpsquid:S1234", "MAJOR", "my-project:File.cs", "my-project",
                42, null, "OPEN", "Fix this", null, null, null, null, "BUG", null, null)],
            null, null);

        var hotspots = new HotspotSearchResponse(
            new Paging(1, 100, 0),
            [],
            null);

        var response = new PullRequestAnalysisResponse(
            "my-project", "123", qualityGate, measures, issues, hotspots);

        _client.GetPullRequestAnalysisAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _tools.analyze_pull_request("my-project", "123");

        var deserialized = JsonSerializer.Deserialize<PullRequestAnalysisResponse>(result);
        deserialized.Should().NotBeNull();
        deserialized!.ProjectKey.Should().Be("my-project");
        deserialized.PullRequest.Should().Be("123");
        deserialized.QualityGate.Status.Should().Be("ERROR");
        deserialized.QualityGate.Conditions.Should().HaveCount(1);
        deserialized.QualityGate.Conditions![0].MetricKey.Should().Be("new_coverage");
        deserialized.Measures!.Measures.Should().HaveCount(1);
        deserialized.Issues.Issues.Should().HaveCount(1);
        deserialized.Hotspots.Hotspots.Should().BeEmpty();
    }

    [Fact]
    public async Task AnalyzePullRequest_WhenClientThrows_ShouldPropagateException()
    {
        _client.GetPullRequestAnalysisAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var act = () => _tools.analyze_pull_request("my-project", "123");

        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
