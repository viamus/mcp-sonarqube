using System.Text.Json.Serialization;

namespace Viamus.Sonarqube.Mcp.Server.Models;

public record DuplicationsShowResponse(
    [property: JsonPropertyName("duplications")] List<DuplicationGroup> Duplications,
    [property: JsonPropertyName("files")] Dictionary<string, DuplicationFile>? Files
);

public record DuplicationGroup(
    [property: JsonPropertyName("blocks")] List<DuplicationBlock> Blocks
);

public record DuplicationBlock(
    [property: JsonPropertyName("from")] int From,
    [property: JsonPropertyName("size")] int Size,
    [property: JsonPropertyName("_ref")] string Ref
);

public record DuplicationFile(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("uuid")] string? Uuid,
    [property: JsonPropertyName("project")] string? Project,
    [property: JsonPropertyName("projectName")] string? ProjectName
);
