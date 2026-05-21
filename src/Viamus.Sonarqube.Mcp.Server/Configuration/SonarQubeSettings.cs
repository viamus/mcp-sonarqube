namespace Viamus.Sonarqube.Mcp.Server.Configuration;

public class SonarQubeSettings
{
    public const string SectionName = "SonarQube";

    public string BaseUrl { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
}
