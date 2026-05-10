using System.Net;
using System.Net.Http;
using System.Text;
using Viamus.Sonarqube.Mcp.Server.Services;

namespace Viamus.Sonarqube.Mcp.Server.Tests.Services;

public class SonarQubeClientTests
{
    private static SonarQubeClient ClientWith(RecordingHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://sonar.example") });

    [Fact]
    public async Task GetComponentTreeMeasures_WhenSortNotInMetricKeys_ShouldThrowArgumentException()
    {
        var handler = new RecordingHandler();
        var client = ClientWith(handler);

        var act = () => client.GetComponentTreeMeasuresAsync(
            "my-project", "new_coverage", null, null,
            sort: "new_duplicated_lines", asc: false, page: null, pageSize: null,
            cancellationToken: CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*new_duplicated_lines*new_coverage*");
        handler.LastRequestUri.Should().BeNull("validation must short-circuit before any HTTP call");
    }

    [Fact]
    public async Task GetComponentTreeMeasures_WhenSortIsNewCodeMetric_ShouldEmitMetricPeriodSchema()
    {
        var handler = new RecordingHandler("""
            { "paging": { "pageIndex": 1, "pageSize": 50, "total": 0 }, "components": [] }
            """);
        var client = ClientWith(handler);

        await client.GetComponentTreeMeasuresAsync(
            "my-project", "new_duplicated_lines,new_coverage",
            pullRequest: "42", qualifiers: "FIL",
            sort: "new_duplicated_lines", asc: false,
            page: null, pageSize: null,
            cancellationToken: CancellationToken.None);

        handler.LastRequestUri.Should().NotBeNull();
        var query = handler.LastRequestUri!.Query;
        query.Should().Contain("component=my-project");
        query.Should().Contain("metricKeys=new_duplicated_lines%2Cnew_coverage");
        query.Should().Contain("pullRequest=42");
        query.Should().Contain("qualifiers=FIL");
        query.Should().Contain("s=metricPeriod");
        query.Should().Contain("metricSort=new_duplicated_lines");
        query.Should().Contain("metricPeriodSort=1");
        query.Should().Contain("asc=false");
    }

    [Fact]
    public async Task GetComponentTreeMeasures_WhenSortIsAbsoluteMetric_ShouldEmitMetricSchema()
    {
        var handler = new RecordingHandler("""
            { "paging": { "pageIndex": 1, "pageSize": 50, "total": 0 }, "components": [] }
            """);
        var client = ClientWith(handler);

        await client.GetComponentTreeMeasuresAsync(
            "my-project", "coverage,bugs",
            pullRequest: null, qualifiers: "FIL",
            sort: "coverage", asc: true,
            page: null, pageSize: null,
            cancellationToken: CancellationToken.None);

        var query = handler.LastRequestUri!.Query;
        query.Should().Contain("s=metric");
        query.Should().NotContain("s=metricPeriod");
        query.Should().Contain("metricSort=coverage");
        query.Should().NotContain("metricPeriodSort");
        query.Should().Contain("asc=true");
    }

    [Fact]
    public async Task GetDuplications_ShouldBuildExpectedUrl()
    {
        var handler = new RecordingHandler("""{ "duplications": [], "files": {} }""");
        var client = ClientWith(handler);

        await client.GetDuplicationsAsync("my-project:src/A.tsx", pullRequest: "42", cancellationToken: CancellationToken.None);

        handler.LastRequestUri.Should().NotBeNull();
        handler.LastRequestUri!.AbsolutePath.Should().Be("/api/duplications/show");
        handler.LastRequestUri.Query.Should().Contain("key=my-project%3Asrc%2FA.tsx");
        handler.LastRequestUri.Query.Should().Contain("pullRequest=42");
    }

    [Fact]
    public async Task SearchIssues_WithPullRequest_ShouldIncludePullRequestQueryParam()
    {
        var handler = new RecordingHandler("""
            { "paging": { "pageIndex": 1, "pageSize": 100, "total": 0 }, "issues": [] }
            """);
        var client = ClientWith(handler);

        await client.SearchIssuesAsync("my-project", null, null, null, null, null, null,
            pullRequest: "42", cancellationToken: CancellationToken.None);

        handler.LastRequestUri.Should().NotBeNull();
        handler.LastRequestUri!.Query.Should().Contain("pullRequest=42");
        handler.LastRequestUri.Query.Should().Contain("projects=my-project");
    }

    [Fact]
    public async Task FailedResponse_ShouldIncludeSonarErrorBodyInException()
    {
        var handler = new RecordingHandler(
            """{"errors":[{"msg":"Value of parameter 's' must be one of: metric, metricPeriod, name, path, qualifier"}]}""",
            HttpStatusCode.BadRequest);
        var client = ClientWith(handler);

        var act = () => client.GetDuplicationsAsync("file-key", null, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<HttpRequestException>();
        exception.Which.Message.Should().Contain("400");
        exception.Which.Message.Should().Contain("Value of parameter 's'");
        exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed class RecordingHandler(string responseJson = "{}", HttpStatusCode status = HttpStatusCode.OK) : HttpMessageHandler
    {
        public Uri? LastRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
                RequestMessage = request
            });
        }
    }
}
