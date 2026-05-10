# MCP SonarQube Server

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![MCP](https://img.shields.io/badge/MCP-Compatible-blue)](https://modelcontextprotocol.io/)
[![Tools](https://img.shields.io/badge/Tools-13-orange)](#available-tools)
[![Tests](https://img.shields.io/badge/Tests-52%20passing-success)](#development)

A [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) server that lets AI assistants talk to SonarQube — query projects, drill into issues, audit pull requests, locate duplication, and check quality gates — all through a single HTTP transport.

### Highlights

- **PR analysis in one call** — `analyze_pull_request` aggregates quality gate, new-code measures, issues, and security hotspots in parallel
- **Locate the offending file** — `get_component_tree_measures` breaks aggregated metrics down by file/directory so the agent knows where to fix
- **Exact duplication blocks** — `get_duplications` returns line ranges and paired files
- **Clear errors** — Sonar's error body (`errors[].msg`) is propagated through every wrapper

---

## Quick Start

```bash
# 1. Clone the repository
git clone https://github.com/viamus/mcp-sonarqube.git
cd mcp-sonarqube

# 2. Configure credentials
cp .env.example .env
# Edit .env with your SonarQube URL and token

# 3. Run the server
docker compose up -d
```

---

## Prerequisites

| Requirement | Version | Purpose |
|-------------|---------|---------|
| .NET SDK | 10.0+ | Build and run |
| Docker | Latest | Container deployment (optional) |
| SonarQube Instance | 9.x+ | API access |

**Required:**
- SonarQube base URL (e.g., `https://sonarqube.example.com`)
- SonarQube user token (generate at: Your SonarQube > My Account > Security > Tokens)

---

## Available Tools

13 MCP tools, grouped by domain:

| Domain | Tool | Purpose |
|--------|------|---------|
| **Pull Requests** | `analyze_pull_request` | Aggregated PR view — quality gate, new-code measures, issues, security hotspots (4 calls in parallel) |
| **Projects** | `search_projects` | Search projects by name or key |
| | `get_project_status` | Quality gate status + key measures for a project |
| **Issues** | `search_issues` | Search issues with filters; supports `pullRequest` scoping |
| **Measures** | `get_measures` | Get metrics for a component |
| | `get_component_tree_measures` | Break measures down by file/directory — locate offending files |
| **Duplications** | `get_duplications` | Exact duplication blocks (line ranges + paired files) |
| **Quality Gates** | `list_quality_gates` | List all gates with their conditions |
| **Hotspots** | `search_hotspots` | Search security hotspots |
| | `get_hotspot` | Detailed hotspot info |
| **Rules** | `search_rules` | Search coding rules by language/severity/tags |
| **System** | `get_health` | Health status of the SonarQube instance |

For the underlying SonarQube endpoints each tool wraps, see the [API Reference](#api-reference) below.

---

## Running Options

### Option 1: Docker Compose (Recommended)

```bash
docker compose up -d
```

The server will be available at `http://localhost:8082`.

### Option 2: .NET CLI

```bash
dotnet run --project src/Viamus.Sonarqube.Mcp.Server
```

The server will be available at `http://localhost:5100`.

### Option 3: Self-Contained Executable

```bash
dotnet publish src/Viamus.Sonarqube.Mcp.Server -c Release -o ./publish
./publish/Viamus.Sonarqube.Mcp.Server
```

---

## Client Configuration

### Claude Desktop

Add to your Claude Desktop configuration (`claude_desktop_config.json`):

```json
{
  "mcpServers": {
    "sonarqube": {
      "url": "http://localhost:8082"
    }
  }
}
```

### Claude Code

```bash
claude mcp add sonarqube --scope user --transport http http://localhost:8082
```

---

## Usage Examples

### Audit a pull request end-to-end

```
Analyze pull request 1234 on the "my-app" project. Did it break the gate?
If yes, find which files concentrate the new duplicated lines and show me
the exact duplication blocks.
```

> Under the hood the agent will typically chain `analyze_pull_request` → `get_component_tree_measures` (sorted by `new_duplicated_lines`) → `get_duplications` on the worst file.

### Search for projects

```
Search for all projects containing "backend" in my SonarQube instance.
```

### Check project quality

```
What is the quality gate status for the "my-app" project? Show me the coverage and bug count.
```

### Find critical issues

```
Search for all CRITICAL and BLOCKER severity issues in the "my-app" project.
```

### Review security hotspots

```
Show me all security hotspots that need review in the "my-app" project.
```

### Search coding rules

```
Find all MAJOR severity rules for C# language.
```

### Check system health

```
Is my SonarQube instance healthy?
```

---

## Configuration

### Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `SONARQUBE_BASE_URL` | Yes | SonarQube instance URL |
| `SONARQUBE_TOKEN` | Yes | SonarQube user token |
| `SERVER_REQUIRE_API_KEY` | No | Enable API key authentication (default: `false`) |
| `SERVER_API_KEY` | No | API key for server access |

### appsettings.json

```json
{
  "SonarQube": {
    "BaseUrl": "https://your-sonarqube-instance.com",
    "Token": "your-token-here"
  },
  "ServerSecurity": {
    "RequireApiKey": false,
    "ApiKey": ""
  }
}
```

### User Secrets (Development)

```bash
cd src/Viamus.Sonarqube.Mcp.Server
dotnet user-secrets set "SonarQube:BaseUrl" "https://your-sonarqube-instance.com"
dotnet user-secrets set "SonarQube:Token" "your-token-here"
```

---

## Troubleshooting

### Common Issues

**Connection refused**
- Verify the SonarQube base URL is correct and accessible
- Check that the server is running: `curl http://localhost:8082/health`

**401 Unauthorized from SonarQube**
- Verify your token is valid and not expired
- Generate a new token at: Your SonarQube > My Account > Security > Tokens

**No projects found**
- Ensure your token has sufficient permissions
- Verify the project exists in your SonarQube instance

**Docker container not starting**
- Check logs: `docker compose logs mcp-sonarqube`
- Verify `.env` file exists and contains valid credentials

---

## Project Structure

```
mcp-sonarqube/
├── src/Viamus.Sonarqube.Mcp.Server/
│   ├── Configuration/          # Settings classes
│   ├── Middleware/              # API key authentication
│   ├── Models/                  # SonarQube API DTOs
│   ├── Services/                # HTTP client for SonarQube API
│   ├── Tools/                   # MCP tool implementations (13 tools)
│   └── Program.cs               # Entry point
├── tests/Viamus.Sonarqube.Mcp.Server.Tests/
│   ├── Configuration/           # Settings and middleware tests
│   ├── Models/                  # Serialization tests
│   └── Tools/                   # Tool behavior tests
├── docker-compose.yml
└── Solution.slnx
```

---

## API Reference

### SonarQube API Endpoints Used

| Endpoint | Tool(s) |
|----------|---------|
| `/api/projects/search` | `search_projects` |
| `/api/qualitygates/project_status` | `get_project_status`, `analyze_pull_request` |
| `/api/qualitygates/list` | `list_quality_gates` |
| `/api/measures/component` | `get_project_status`, `get_measures`, `analyze_pull_request` |
| `/api/measures/component_tree` | `get_component_tree_measures` |
| `/api/duplications/show` | `get_duplications` |
| `/api/issues/search` | `search_issues`, `analyze_pull_request` |
| `/api/hotspots/search` | `search_hotspots`, `analyze_pull_request` |
| `/api/hotspots/show` | `get_hotspot` |
| `/api/rules/search` | `search_rules` |
| `/api/system/health` | `get_health` |

---

## Development

### Building

```bash
dotnet build Solution.slnx
```

### Testing

```bash
dotnet test Solution.slnx
```

### Adding New Tools

See [CONTRIBUTING.md](CONTRIBUTING.md#adding-a-new-mcp-tool) for detailed instructions.

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
