using Viamus.Sonarqube.Mcp.Server.Configuration;

namespace Viamus.Sonarqube.Mcp.Server.Tests.Configuration;

public class SonarQubeSettingsTests
{
    [Fact]
    public void SectionName_ShouldBeSonarQube()
    {
        SonarQubeSettings.SectionName.Should().Be("SonarQube");
    }

    [Fact]
    public void DefaultValues_ShouldBeEmpty()
    {
        var settings = new SonarQubeSettings();

        settings.BaseUrl.Should().BeEmpty();
        settings.Token.Should().BeEmpty();
        settings.Organization.Should().BeEmpty();
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        var settings = new SonarQubeSettings
        {
            BaseUrl = "https://sonarqube.example.com",
            Token = "test-token",
            Organization = "test-org"
        };

        settings.BaseUrl.Should().Be("https://sonarqube.example.com");
        settings.Token.Should().Be("test-token");
        settings.Organization.Should().Be("test-org");
    }
}
