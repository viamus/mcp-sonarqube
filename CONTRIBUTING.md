# Contributing Guide

Thank you for your interest in contributing to MCP SonarQube Server!

This project provides a Model Context Protocol (MCP) server that exposes tools for interacting with SonarQube Projects, Issues, Quality Gates, Measures, Hotspots, Rules, and System Health. Contributions of all kinds are welcome, including bug fixes, documentation improvements, new tools, refactors, and tests.

---

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Project Goals](#project-goals)
- [Ways to Contribute](#ways-to-contribute)
- [Development Setup](#development-setup)
- [Branching & Workflow](#branching--workflow)
- [Commit & PR Guidelines](#commit--pr-guidelines)
- [Coding Standards](#coding-standards)
- [Testing](#testing)
- [Adding a New MCP Tool](#adding-a-new-mcp-tool)
- [Architecture Overview](#architecture-overview)
- [Security](#security)
- [Getting Help](#getting-help)

---

## Code of Conduct

Be respectful, constructive, and collaborative.

Harassment, discrimination, or abusive behavior will not be tolerated. All contributors are expected to interact professionally and respectfully. See [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) for details.

---

## Project Goals

- Provide a reliable MCP server for SonarQube integration
- Offer useful, composable tools for Projects, Issues, Quality Gates, Measures, Hotspots, Rules, and System Health
- Keep the server safe-by-default (minimal permissions, no secret leakage)
- Maintain a clean and extensible architecture for future domains

---

## Ways to Contribute

You can contribute by:

- **Reporting bugs** with clear reproduction steps
- **Improving documentation** (README, examples, guides)
- **Adding new MCP tools** or extending existing ones
- **Improving logging**, error handling, and observability
- **Writing or improving tests**
- **Refactoring code** for clarity and maintainability

---

## Development Setup

### Prerequisites

| Requirement | Version | Purpose |
|-------------|---------|---------|
| .NET SDK | 10.0+ | Build and run |
| Docker | Latest | Container deployment (optional) |
| SonarQube Instance | 9.x+ | API access |

**Required:**
- SonarQube user token (generate at: Your SonarQube > My Account > Security > Tokens)

### Clone & Configure

```bash
# 1. Clone the repository
git clone https://github.com/viamus/mcp-sonarqube.git
cd mcp-sonarqube

# 2. Create environment file
cp .env.example .env

# 3. Edit .env with your credentials
```

> **Warning**: Never commit `.env` files or hardcode credentials!

### Run Locally

```bash
# Using .NET CLI
dotnet run --project src/Viamus.Sonarqube.Mcp.Server

# Using Docker
docker compose up -d
```

### Verify Setup

```bash
# .NET CLI (port 5100)
curl http://localhost:5100/health

# Docker (port 8082)
curl http://localhost:8082/health
```

---

## Branching & Workflow

The `main` branch must remain stable. Create feature branches using these patterns:

| Prefix | Purpose | Example |
|--------|---------|---------|
| `feat/` | New features | `feat/add-source-tools` |
| `fix/` | Bug fixes | `fix/issue-query-error` |
| `docs/` | Documentation | `docs/improve-readme` |
| `chore/` | Maintenance | `chore/update-deps` |
| `test/` | Test additions | `test/hotspot-tools` |

### Workflow

1. Create a branch from `main`
2. Make your changes
3. Add or update tests
4. Run tests locally: `dotnet test`
5. Open a Pull Request targeting `main`

---

## Commit & PR Guidelines

### Commits

Use [Conventional Commits](https://www.conventionalcommits.org/) style:

```
feat: add search_rules tool
fix: handle issue query timeout gracefully
docs: clarify token permissions
test: add unit tests for hotspot tools
chore: bump dependencies
```

### Pull Requests

A good PR includes:

- **What** changed and **why**
- Link to related issue (if applicable)
- Notes about breaking changes (avoid if possible)
- Confirmation that no secrets were introduced
- Logs or screenshots when helpful

---

## Coding Standards

### General Principles

- Prefer clarity over cleverness
- Keep MCP tools focused (single responsibility)
- Avoid leaking secrets via logs or exceptions
- Validate inputs and return consistent outputs
- Errors should be actionable and safe

### .NET Guidelines

- Use `async/await` consistently for I/O operations
- Favor dependency injection
- Keep handlers/controllers thin
- Put business logic in services
- Keep models and DTOs explicit and simple
- Use `sealed record` for DTOs when possible (immutability)

---

## Testing

When behavior changes, tests should be added or updated.

### Running Tests

```bash
# All tests
dotnet test

# Specific test class
dotnet test --filter "FullyQualifiedName~ProjectToolsTests"
dotnet test --filter "FullyQualifiedName~IssueToolsTests"
dotnet test --filter "FullyQualifiedName~HotspotToolsTests"
dotnet test --filter "FullyQualifiedName~RuleToolsTests"

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Structure

```
tests/Viamus.Sonarqube.Mcp.Server.Tests/
├── Configuration/  # Settings and middleware tests
├── Models/         # DTO serialization tests
└── Tools/          # Tool behavior tests with mocked services
```

### Testing Layers

- **Unit tests**: Services and mapping logic
- **Contract tests**: MCP tool outputs
- **Integration tests**: HTTP endpoints (optional but encouraged)

---

## Adding a New MCP Tool

### Checklist

Before creating a new tool, ensure it has:

- [ ] Clear and descriptive name (snake_case)
- [ ] Single responsibility
- [ ] Stable inputs and outputs
- [ ] Parameter validation
- [ ] Safe and consistent error handling
- [ ] Unit tests
- [ ] Documentation in README.md

### Steps

1. **Add tool implementation** in `src/.../Tools/`

```csharp
[McpServerToolType]
public class MyTools(ISonarQubeClient sonarQubeClient, ILogger<MyTools> logger)
{
    [McpServerTool, Description("Description of what this tool does")]
    public async Task<string> my_tool(
        [Description("Parameter description")] string param)
    {
        logger.LogInformation("Executing my_tool with param: {Param}", param);
        var result = await sonarQubeClient.MyMethodAsync(param);
        return JsonSerializer.Serialize(result);
    }
}
```

2. **Add service method** in `src/.../Services/`
   - Add signature to `ISonarQubeClient.cs`
   - Implement in `SonarQubeClient.cs`

3. **Add DTOs if needed** in `src/.../Models/`
   - Include `[JsonPropertyName]` attributes for API mapping

4. **Add tests** in `tests/.../Tools/`

5. **Update README.md** with the new tool

> Tools are auto-registered via `.WithToolsFromAssembly()`

---

## Architecture Overview

### Project Structure

```
src/Viamus.Sonarqube.Mcp.Server/
├── Configuration/
│   ├── ServerSecuritySettings.cs     # Server security config
│   └── SonarQubeSettings.cs          # SonarQube connection config
├── Middleware/
│   └── ApiKeyMiddleware.cs           # API key authentication
├── Models/
│   ├── Paging.cs                     # Shared pagination
│   ├── ProjectModels.cs              # Project DTOs
│   ├── IssueModels.cs                # Issue DTOs
│   ├── QualityGateModels.cs          # Quality gate DTOs
│   ├── MeasureModels.cs              # Measure DTOs
│   ├── HotspotModels.cs              # Hotspot DTOs
│   ├── SystemHealthModels.cs         # System health DTOs
│   └── RuleModels.cs                 # Rule DTOs
├── Services/
│   ├── ISonarQubeClient.cs           # Service interface
│   └── SonarQubeClient.cs            # HTTP client implementation
├── Tools/
│   ├── ProjectTools.cs               # Project tools (2)
│   ├── IssueTools.cs                 # Issue tools (1)
│   ├── MeasureTools.cs               # Measure tools (1)
│   ├── QualityGateTools.cs           # Quality gate tools (1)
│   ├── HotspotTools.cs               # Hotspot tools (2)
│   ├── SystemTools.cs                # System tools (1)
│   └── RuleTools.cs                  # Rule tools (1)
└── Program.cs                        # Entry point & DI
```

### Key Patterns

| Pattern | Description |
|---------|-------------|
| Dependency Injection | Services registered via `AddHttpClient` |
| Interface-based design | Enables testing with mocks |
| JSON serialization | Responses with `System.Text.Json` |
| Error handling | Exception propagation, structured logging |
| Logging | Structured logging with `ILogger<T>` |

### SonarQube API Endpoints

| Endpoint | Used For |
|----------|----------|
| `/api/projects/search` | Search projects |
| `/api/issues/search` | Search issues |
| `/api/qualitygates/project_status` | Quality gate status |
| `/api/qualitygates/list` | List quality gates |
| `/api/measures/component` | Component measures |
| `/api/hotspots/search` | Search hotspots |
| `/api/hotspots/show` | Hotspot details |
| `/api/system/health` | System health |
| `/api/rules/search` | Search rules |

---

## Security

- **Never commit secrets** (SonarQube tokens)
- **Avoid logging sensitive data**
- **Validate all external inputs**
- **Use environment variables** or user secrets for credentials

If you discover a security vulnerability:
- **Do NOT open a public issue**
- Contact the maintainers privately (see [SECURITY.md](SECURITY.md))

---

## Getting Help

If you need help:

1. Check the [README](README.md) first
2. Search existing [issues](https://github.com/viamus/mcp-sonarqube/issues)
3. Open a new issue with:
   - Expected behavior
   - Actual behavior
   - Reproduction steps
   - Logs (with secrets removed)
   - Environment details (OS, .NET version, Docker version)

---

**Thank you for contributing to MCP SonarQube Server!**

Your help makes this project better for everyone.
