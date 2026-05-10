using System.Text.Json;
using Microsoft.Extensions.Logging;
using Viamus.Sonarqube.Mcp.Server.Models;
using Viamus.Sonarqube.Mcp.Server.Services;
using Viamus.Sonarqube.Mcp.Server.Tools;

namespace Viamus.Sonarqube.Mcp.Server.Tests.Tools;

public class MeasureToolsTests
{
    private readonly ISonarQubeClient _client = Substitute.For<ISonarQubeClient>();
    private readonly MeasureTools _tools;

    public MeasureToolsTests()
    {
        _tools = new MeasureTools(_client, Substitute.For<ILogger<MeasureTools>>());
    }

    [Fact]
    public async Task GetMeasures_ShouldReturnSerializedResponse()
    {
        var response = new MeasureComponentResponse(
            new MeasureComponent("my-project", "My Project", "TRK",
                [new Measure("coverage", "85.5", null), new Measure("bugs", "3", null)]));

        _client.GetMeasuresAsync("my-project", "coverage,bugs", Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _tools.get_measures("my-project", "coverage,bugs");

        var deserialized = JsonSerializer.Deserialize<MeasureComponentResponse>(result);
        deserialized.Should().NotBeNull();
        deserialized!.Component.Measures.Should().HaveCount(2);
        deserialized.Component.Measures![0].Metric.Should().Be("coverage");
    }

    [Fact]
    public async Task GetMeasures_WhenClientThrows_ShouldPropagateException()
    {
        _client.GetMeasuresAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var act = () => _tools.get_measures("my-project", "coverage");

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetComponentTreeMeasures_ShouldReturnSerializedResponse()
    {
        var response = new ComponentTreeMeasuresResponse(
            new Paging(1, 50, 1),
            new ComponentTreeNode("my-project", "My Project", "TRK", null, null, null),
            [
                new ComponentTreeNode(
                    "my-project:src/A.tsx", "A.tsx", "FIL", "src/A.tsx", "ts",
                    [new Measure("new_duplicated_lines", null, new MeasurePeriod("14", false))])
            ]);

        _client.GetComponentTreeMeasuresAsync(
                "my-project", "new_duplicated_lines",
                "42", "FIL", null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _tools.get_component_tree_measures(
            "my-project", "new_duplicated_lines", pullRequest: "42");

        var deserialized = JsonSerializer.Deserialize<ComponentTreeMeasuresResponse>(result);
        deserialized.Should().NotBeNull();
        deserialized!.Components.Should().HaveCount(1);
        deserialized.Components[0].Path.Should().Be("src/A.tsx");
        deserialized.Components[0].Measures![0].Period!.Value.Should().Be("14");
    }

    [Fact]
    public async Task GetComponentTreeMeasures_WithSort_ShouldForwardSortAndAsc()
    {
        var response = new ComponentTreeMeasuresResponse(
            new Paging(1, 50, 0), null, []);

        _client.GetComponentTreeMeasuresAsync(
                Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<bool?>(),
                Arg.Any<int?>(), Arg.Any<int?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _tools.get_component_tree_measures(
            "p", "new_duplicated_lines,new_uncovered_lines",
            sort: "new_duplicated_lines", asc: false);

        await _client.Received(1).GetComponentTreeMeasuresAsync(
            "p", "new_duplicated_lines,new_uncovered_lines",
            null, "FIL",
            "new_duplicated_lines", false,
            null, null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetComponentTreeMeasures_WhenClientThrows_ShouldPropagateException()
    {
        _client.GetComponentTreeMeasuresAsync(
                Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<bool?>(),
                Arg.Any<int?>(), Arg.Any<int?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var act = () => _tools.get_component_tree_measures("p", "new_coverage");

        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
