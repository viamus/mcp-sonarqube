using System.Text.Json;
using Microsoft.Extensions.Logging;
using Viamus.Sonarqube.Mcp.Server.Models;
using Viamus.Sonarqube.Mcp.Server.Services;
using Viamus.Sonarqube.Mcp.Server.Tools;

namespace Viamus.Sonarqube.Mcp.Server.Tests.Tools;

public class DuplicationToolsTests
{
    private readonly ISonarQubeClient _client = Substitute.For<ISonarQubeClient>();
    private readonly DuplicationTools _tools;

    public DuplicationToolsTests()
    {
        _tools = new DuplicationTools(_client, Substitute.For<ILogger<DuplicationTools>>());
    }

    [Fact]
    public async Task GetDuplications_ShouldReturnSerializedResponse()
    {
        var response = new DuplicationsShowResponse(
            [
                new DuplicationGroup([
                    new DuplicationBlock(42, 8, "1"),
                    new DuplicationBlock(17, 8, "2")
                ])
            ],
            new Dictionary<string, DuplicationFile>
            {
                ["1"] = new("my-project:src/A.tsx", "A.tsx", null, "my-project", "My Project"),
                ["2"] = new("my-project:src/B.tsx", "B.tsx", null, "my-project", "My Project")
            });

        _client.GetDuplicationsAsync("my-project:src/A.tsx", "42", Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _tools.get_duplications("my-project:src/A.tsx", "42");

        var deserialized = JsonSerializer.Deserialize<DuplicationsShowResponse>(result);
        deserialized.Should().NotBeNull();
        deserialized!.Duplications.Should().HaveCount(1);
        deserialized.Duplications[0].Blocks.Should().HaveCount(2);
        deserialized.Duplications[0].Blocks[0].From.Should().Be(42);
        deserialized.Files.Should().ContainKey("1");
        deserialized.Files!["1"].Name.Should().Be("A.tsx");
    }

    [Fact]
    public async Task GetDuplications_WithoutPullRequest_ShouldForwardNull()
    {
        var response = new DuplicationsShowResponse([], null);

        _client.GetDuplicationsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _tools.get_duplications("my-project:src/A.tsx");

        await _client.Received(1).GetDuplicationsAsync(
            "my-project:src/A.tsx", null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetDuplications_WhenClientThrows_ShouldPropagateException()
    {
        _client.GetDuplicationsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var act = () => _tools.get_duplications("my-project:src/A.tsx");

        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
