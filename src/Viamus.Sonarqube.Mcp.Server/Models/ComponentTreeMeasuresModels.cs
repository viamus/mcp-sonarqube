using System.Text.Json.Serialization;

namespace Viamus.Sonarqube.Mcp.Server.Models;

public record ComponentTreeMeasuresResponse(
    [property: JsonPropertyName("paging")] Paging Paging,
    [property: JsonPropertyName("baseComponent")] ComponentTreeNode? BaseComponent,
    [property: JsonPropertyName("components")] List<ComponentTreeNode> Components
);

public record ComponentTreeNode(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("qualifier")] string? Qualifier,
    [property: JsonPropertyName("path")] string? Path,
    [property: JsonPropertyName("language")] string? Language,
    [property: JsonPropertyName("measures")] List<Measure>? Measures
);
