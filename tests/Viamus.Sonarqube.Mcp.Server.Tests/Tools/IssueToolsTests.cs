using System.Text.Json;
using Microsoft.Extensions.Logging;
using Viamus.Sonarqube.Mcp.Server.Models;
using Viamus.Sonarqube.Mcp.Server.Services;
using Viamus.Sonarqube.Mcp.Server.Tools;

namespace Viamus.Sonarqube.Mcp.Server.Tests.Tools;

public class IssueToolsTests
{
    private readonly ISonarQubeClient _client = Substitute.For<ISonarQubeClient>();
    private readonly IssueTools _tools;

    public IssueToolsTests()
    {
        _tools = new IssueTools(_client, Substitute.For<ILogger<IssueTools>>());
    }

    [Fact]
    public async Task SearchIssues_ShouldReturnSerializedResponse()
    {
        var response = new IssueSearchResponse(
            new Paging(1, 100, 1),
            [new Issue("issue-1", "cs:S1234", "MAJOR", "my-project:File.cs", "my-project",
                42, null, "OPEN", "Fix this", null, null, null, null, "BUG", null, null)],
            null, null);

        _client.SearchIssuesAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>(),
                Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _tools.search_issues("my-project");

        var deserialized = JsonSerializer.Deserialize<IssueSearchResponse>(result);
        deserialized.Should().NotBeNull();
        deserialized!.Issues.Should().HaveCount(1);
        deserialized.Issues[0].Key.Should().Be("issue-1");
    }

    [Fact]
    public async Task SearchIssues_WithPullRequest_ShouldForwardPullRequestToClient()
    {
        var response = new IssueSearchResponse(new Paging(1, 100, 0), [], null, null);

        _client.SearchIssuesAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>(),
                Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _tools.search_issues("my-project", pullRequest: "42");

        await _client.Received(1).SearchIssuesAsync(
            "my-project", null, null, null, null, null, null,
            "42", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchIssues_WhenClientThrows_ShouldPropagateException()
    {
        _client.SearchIssuesAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>(),
                Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var act = () => _tools.search_issues("my-project");

        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
