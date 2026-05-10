using System.Text.Json;
using Viamus.Sonarqube.Mcp.Server.Models;

namespace Viamus.Sonarqube.Mcp.Server.Tests.Models;

public class ModelSerializationTests
{
    [Fact]
    public void ProjectSearchResponse_ShouldDeserialize()
    {
        var json = """
        {
            "paging": { "pageIndex": 1, "pageSize": 100, "total": 1 },
            "components": [
                { "key": "my-project", "name": "My Project", "qualifier": "TRK", "visibility": "public", "lastAnalysisDate": "2024-01-01T00:00:00+0000" }
            ]
        }
        """;

        var result = JsonSerializer.Deserialize<ProjectSearchResponse>(json);

        result.Should().NotBeNull();
        result!.Paging.Total.Should().Be(1);
        result.Components.Should().HaveCount(1);
        result.Components[0].Key.Should().Be("my-project");
        result.Components[0].Name.Should().Be("My Project");
    }

    [Fact]
    public void IssueSearchResponse_ShouldDeserialize()
    {
        var json = """
        {
            "paging": { "pageIndex": 1, "pageSize": 100, "total": 1 },
            "issues": [
                {
                    "key": "issue-1", "rule": "cs:S1234", "severity": "MAJOR",
                    "component": "my-project:src/File.cs", "project": "my-project",
                    "line": 42, "status": "OPEN", "message": "Fix this",
                    "textRange": { "startLine": 42, "endLine": 42, "startOffset": 0, "endOffset": 10 }
                }
            ],
            "components": [{ "key": "my-project:src/File.cs", "name": "File.cs", "qualifier": "FIL", "path": "src/File.cs" }],
            "rules": [{ "key": "cs:S1234", "name": "Rule Name", "lang": "cs", "status": "READY" }]
        }
        """;

        var result = JsonSerializer.Deserialize<IssueSearchResponse>(json);

        result.Should().NotBeNull();
        result!.Issues.Should().HaveCount(1);
        result.Issues[0].Key.Should().Be("issue-1");
        result.Issues[0].Line.Should().Be(42);
        result.Issues[0].TextRange!.StartLine.Should().Be(42);
        result.Components.Should().HaveCount(1);
        result.Rules.Should().HaveCount(1);
    }

    [Fact]
    public void QualityGateProjectStatusResponse_ShouldDeserialize()
    {
        var json = """
        {
            "projectStatus": {
                "status": "OK",
                "conditions": [
                    { "status": "OK", "metricKey": "coverage", "comparator": "LT", "errorThreshold": "80", "actualValue": "85.5" }
                ]
            }
        }
        """;

        var result = JsonSerializer.Deserialize<QualityGateProjectStatusResponse>(json);

        result.Should().NotBeNull();
        result!.ProjectStatus.Status.Should().Be("OK");
        result.ProjectStatus.Conditions.Should().HaveCount(1);
        result.ProjectStatus.Conditions![0].MetricKey.Should().Be("coverage");
    }

    [Fact]
    public void QualityGateListResponse_ShouldDeserialize()
    {
        var json = """
        {
            "qualitygates": [
                { "id": 1, "name": "Sonar way", "isDefault": true, "isBuiltIn": true, "conditions": [{ "id": 1, "metric": "coverage", "op": "LT", "error": "80" }] }
            ],
            "default": 1
        }
        """;

        var result = JsonSerializer.Deserialize<QualityGateListResponse>(json);

        result.Should().NotBeNull();
        result!.QualityGates.Should().HaveCount(1);
        result.QualityGates[0].Name.Should().Be("Sonar way");
        result.QualityGates[0].IsDefault.Should().BeTrue();
        result.Default.Should().Be(1);
    }

    [Fact]
    public void MeasureComponentResponse_ShouldDeserialize()
    {
        var json = """
        {
            "component": {
                "key": "my-project", "name": "My Project", "qualifier": "TRK",
                "measures": [
                    { "metric": "coverage", "value": "85.5", "period": { "value": "2.3", "bestValue": false } }
                ]
            }
        }
        """;

        var result = JsonSerializer.Deserialize<MeasureComponentResponse>(json);

        result.Should().NotBeNull();
        result!.Component.Key.Should().Be("my-project");
        result.Component.Measures.Should().HaveCount(1);
        result.Component.Measures![0].Metric.Should().Be("coverage");
        result.Component.Measures[0].Value.Should().Be("85.5");
        result.Component.Measures[0].Period!.Value.Should().Be("2.3");
    }

    [Fact]
    public void HotspotSearchResponse_ShouldDeserialize()
    {
        var json = """
        {
            "paging": { "pageIndex": 1, "pageSize": 100, "total": 1 },
            "hotspots": [
                { "key": "hotspot-1", "component": "my-project:src/File.cs", "project": "my-project",
                  "securityCategory": "sql-injection", "vulnerabilityProbability": "HIGH",
                  "status": "TO_REVIEW", "line": 10, "message": "Review this" }
            ],
            "components": [{ "key": "my-project:src/File.cs", "qualifier": "FIL", "name": "File.cs", "path": "src/File.cs" }]
        }
        """;

        var result = JsonSerializer.Deserialize<HotspotSearchResponse>(json);

        result.Should().NotBeNull();
        result!.Hotspots.Should().HaveCount(1);
        result.Hotspots[0].Key.Should().Be("hotspot-1");
        result.Hotspots[0].SecurityCategory.Should().Be("sql-injection");
    }

    [Fact]
    public void HotspotDetailResponse_ShouldDeserialize()
    {
        var json = """
        {
            "key": "hotspot-1",
            "component": { "key": "my-project:src/File.cs", "qualifier": "FIL", "name": "File.cs", "path": "src/File.cs" },
            "project": { "key": "my-project", "name": "My Project" },
            "rule": { "key": "cs:S1234", "name": "Rule Name", "securityCategory": "sql-injection", "vulnerabilityProbability": "HIGH" },
            "status": "TO_REVIEW", "message": "Review this", "line": 10,
            "changelog": [{ "user": "admin", "creationDate": "2024-01-01", "diffs": [{ "key": "status", "newValue": "TO_REVIEW", "oldValue": "OPEN" }] }]
        }
        """;

        var result = JsonSerializer.Deserialize<HotspotDetailResponse>(json);

        result.Should().NotBeNull();
        result!.Key.Should().Be("hotspot-1");
        result.Rule.Key.Should().Be("cs:S1234");
        result.Changelog.Should().HaveCount(1);
        result.Changelog![0].Diffs.Should().HaveCount(1);
    }

    [Fact]
    public void SystemHealthResponse_ShouldDeserialize()
    {
        var json = """
        {
            "health": "GREEN",
            "causes": []
        }
        """;

        var result = JsonSerializer.Deserialize<SystemHealthResponse>(json);

        result.Should().NotBeNull();
        result!.Health.Should().Be("GREEN");
        result.Causes.Should().BeEmpty();
    }

    [Fact]
    public void SystemHealthResponse_WithCauses_ShouldDeserialize()
    {
        var json = """
        {
            "health": "RED",
            "causes": [{ "message": "Elasticsearch is down" }]
        }
        """;

        var result = JsonSerializer.Deserialize<SystemHealthResponse>(json);

        result.Should().NotBeNull();
        result!.Health.Should().Be("RED");
        result.Causes.Should().HaveCount(1);
        result.Causes![0].Message.Should().Be("Elasticsearch is down");
    }

    [Fact]
    public void ComponentTreeMeasuresResponse_ShouldDeserialize()
    {
        var json = """
        {
            "paging": { "pageIndex": 1, "pageSize": 50, "total": 1 },
            "baseComponent": { "key": "my-project", "name": "My Project", "qualifier": "TRK" },
            "components": [
                {
                    "key": "my-project:src/A.tsx",
                    "name": "A.tsx",
                    "qualifier": "FIL",
                    "path": "src/A.tsx",
                    "language": "ts",
                    "measures": [
                        { "metric": "new_duplicated_lines", "period": { "value": "14" } },
                        { "metric": "new_uncovered_lines", "period": { "value": "0" } }
                    ]
                }
            ]
        }
        """;

        var result = JsonSerializer.Deserialize<ComponentTreeMeasuresResponse>(json);

        result.Should().NotBeNull();
        result!.Paging.Total.Should().Be(1);
        result.BaseComponent!.Key.Should().Be("my-project");
        result.Components.Should().HaveCount(1);
        result.Components[0].Path.Should().Be("src/A.tsx");
        result.Components[0].Measures.Should().HaveCount(2);
        result.Components[0].Measures![0].Period!.Value.Should().Be("14");
    }

    [Fact]
    public void DuplicationsShowResponse_ShouldDeserialize()
    {
        var json = """
        {
            "duplications": [
                {
                    "blocks": [
                        { "from": 42, "size": 8, "_ref": "1" },
                        { "from": 17, "size": 8, "_ref": "2" }
                    ]
                }
            ],
            "files": {
                "1": { "key": "my-project:src/A.tsx", "name": "A.tsx", "project": "my-project", "projectName": "My Project" },
                "2": { "key": "my-project:src/B.tsx", "name": "B.tsx", "project": "my-project", "projectName": "My Project" }
            }
        }
        """;

        var result = JsonSerializer.Deserialize<DuplicationsShowResponse>(json);

        result.Should().NotBeNull();
        result!.Duplications.Should().HaveCount(1);
        result.Duplications[0].Blocks.Should().HaveCount(2);
        result.Duplications[0].Blocks[0].From.Should().Be(42);
        result.Duplications[0].Blocks[0].Size.Should().Be(8);
        result.Duplications[0].Blocks[0].Ref.Should().Be("1");
        result.Files.Should().NotBeNull();
        result.Files!["2"].Name.Should().Be("B.tsx");
    }

    [Fact]
    public void RuleSearchResponse_ShouldDeserialize()
    {
        var json = """
        {
            "paging": { "pageIndex": 1, "pageSize": 100, "total": 1 },
            "rules": [
                { "key": "cs:S1234", "name": "Rule Name", "lang": "cs", "langName": "C#",
                  "severity": "MAJOR", "status": "READY", "type": "CODE_SMELL",
                  "tags": ["convention"], "htmlDesc": "<p>Description</p>" }
            ]
        }
        """;

        var result = JsonSerializer.Deserialize<RuleSearchResponse>(json);

        result.Should().NotBeNull();
        result!.Rules.Should().HaveCount(1);
        result.Rules[0].Key.Should().Be("cs:S1234");
        result.Rules[0].LangName.Should().Be("C#");
        result.Rules[0].Tags.Should().Contain("convention");
    }
}
