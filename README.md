# 🚀 Greg.Xrm.Mcp

**A Comprehensive Framework for Building Model Context Protocol (MCP) Servers for Microsoft Dataverse**

[![.NET 9](https://img.shields.io/badge/.NET-9-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![License](https://img.shields.io/github/license/neronotte/Greg.Xrm.Mcp)](https://github.com/neronotte/Greg.Xrm.Mcp/blob/main/LICENSE)
[![GitHub Issues](https://img.shields.io/github/issues/neronotte/Greg.Xrm.Mcp)](https://github.com/neronotte/Greg.Xrm.Mcp/issues)
[![Build and Release](https://github.com/neronotte/Greg.Xrm.Mcp/actions/workflows/build-and-release.yml/badge.svg?branch=main)](https://github.com/neronotte/Greg.Xrm.Mcp/actions/workflows/build-and-release.yml)

---

## 👁️ Vision

Transform your Dataverse development experience with AI-powered tools that understand your business context. **Greg.Xrm.Mcp** is a **foundational framework** designed to revolutionize how developers interact with Microsoft Dataverse through intelligent MCP-enabled AI assistants (VS Code Copilot MCP extension, Claude Desktop, and other MCP clients).


[▶️ Check-out this video](https://www.youtube.com/watch?v=mvtVm2o9YbI) showcasing the tool in action.

---

## 🏗️ Framework Architecture

At its core, **Greg.Xrm.Mcp** provides a robust foundation for building specialized MCP servers that seamlessly integrate with Dataverse ecosystems:

### **Greg.Xrm.Mcp.Core** - The Foundation

- 🔐 **Unified Authentication**: Standardized Dataverse connection and token management
- 🔧 **Common Services**: Reusable components for metadata, queries, and operations
- 🔌 **MCP Integration**: Built-in Model Context Protocol server capabilities (stdio-based and sse-based)
- 🛡️ **Error Handling**: Comprehensive error management and structured logging
- ⚡ **Performance**: Optimized for real-time AI assistant interactions

---

## ✅ Current Capabilities (AppMaker Server)

The flagship implementation **Greg.Xrm.Mcp.AppMaker** currently offers:

- 🛠️ **Tools**
   - 📦 **Dataverse Metadata Access**:
       - 📂 **List all tables in a given environment**
       - 📂 **List all columns of a given table**
   - 📦 **System Form Manipulation**:
       - 📂 **Form Inventory**: List all forms for a Dataverse table (formatted text or JSON)
       - 🧬 **Form Definition Retrieval**: Fetch form definition (XML or JSON) with metadata
       - 🧹 **Form Updater**: Updates the structure of a form using AI-generated layout (LLM-assisted, non-deterministic)
       - ✅ **FormXML Validation**: Validate FormXML structure and report issues
   - 📦 **Saved Query (view) Management**:
       - 📂 **Saved Query Inventory**: List all saved queries for a table (formatted text or JSON)
       - 🧬 **Saved Query Definition Retrieval**: Fetch saved query definition (both FetchXML and LayoutXML)
       - 🧹 **Saved Query Updater**: Updates the structure of a view using AI-generated layout and filters (LLM-assisted, non-deterministic)
       - 🧹 **Saved Query Maker**: Creates new views using AI-generated layout and filters (LLM-assisted, non-deterministic)
       - 📝 **Saved Query Renamer**: Allows to change the name of an existing view
   - 📦 **AppModules and Sitemaps**
       - 🧭 **App Module Inventory**: Enumerate all model-driven apps with version, managed status, default flag, configuration XML, and associated security roles
       - ➕ **Add/Remove App Components**: Adds or removes table definitions from an app.
       - 🧹 **Create new AppModules**: Creates new Apps, with tables and sitemap.
       - ✅ **AppModule Validation**: Validate the structure and contents of given AppModule and report issues
       - 🧬 **Sitemap Definition Retrieval**: retrieves the XML defining the structure of a given app Sitemap
       - 🧹 **Sitemap Updater**: Updates the structure of a form using AI-generated layout (LLM-assisted, non-deterministic)
- 📄 **Resources**
    - `docs://instructions_for_formxml`: Instructions to be aware of when manipulating Dataverse FormXml
    - `schema://formxml`: Returns a set of Xml schemas defining the structure of Dataverse forms.
    - `schema://layoutxml`: Returns the XML schema describing the structure of Dataverse views in terms of columns.
    - `schema://fetchxml`: Returns the XML schema of the query that runs Dataverse views. 
    - `schema://sitemapxml`: Returns a set of Xml schemas defining the structure of Dataverse sitemap (navigation bar), and instructions on how to properly generate a SiteMap XML document.

---

## 🔌 MCP Usage

The server runs as a **Model Context Protocol** (stdio) endpoint. You can connect via:

- **VS Code** (GitHub Copilot MCP extension)
- **Claude Desktop** (custom MCP server config)

### Installation

```powershell
dotnet tool install --global Greg.Xrm.Mcp.AppMaker
```

### VS Code (.vscode/mcp.json snippet)

```json
{
    "servers": {
        "AppMaker": {
            "command": "Greg.Xrm.Mcp.AppMaker",
            "args": [
                "--dataverseUrl",
                "https://yourorg.crm.dynamics.com"
            ],
            "cwd": "${workspaceFolder}"
        }
    }
}
```

### Claude Desktop (config fragment)

```json
{
    "mcpServers": {
        "AppMaker": {
            "command": "Greg.Xrm.Mcp.AppMaker",
            "args": [
                "--dataverseUrl",
                "https://yourorg.crm.dynamics.com"
            ],
        }
    }
}
```
---

## 🔐 Authentication

This tool supports two authentication modes, tried in order:

### 1. Device-code (default — works everywhere)

When no cached token exists, the server starts a device-code flow via MSAL. The user code and sign-in URL appear immediately in the MCP server's stderr output (visible in VS Code's **Output → MCP** panel or terminal):

```
========================================
Device code active: AJMAKZ4TH
Waiting at: https://login.microsoft.com/device
Status: Pending your MFA completion
Once you enter that code and sign in, the MCP server will complete authentication automatically.
========================================
```

Open [https://login.microsoft.com/device](https://login.microsoft.com/device) in any browser, enter the code, and complete MFA. The server polls silently and resumes automatically — no timeout, no restart needed.

### 2. OAuth loopback (automatic fallback)

If device-code fails (e.g. network policy blocks the device endpoint), the server falls back to `AuthType=OAuth;LoginPrompt=Auto`, which opens a browser window and completes auth via a local redirect at `http://localhost`.

### Token cache

After successful authentication, the access token is cached at:

```
%LocalAppData%\Greg.Xrm.Mcp.Core\tokenCache\tokens.json
```

Subsequent tool calls reuse the cached token with no re-prompt.

### Optional environment variables

| Variable | Default | Description |
|---|---|---|
| `DATAVERSE_CLIENT_ID` | `51f81489-...` (Power Platform public AppId) | Override the Azure AD app registration |
| `DATAVERSE_TENANT_ID` | `organizations` (multi-tenant) | Override for single-tenant scenarios |

---

## 🔌 MCP Client Configuration

### VS Code — `.vscode/mcp.json` (stdio, global dotnet tool)

Install the tool globally first:

```powershell
dotnet tool install --global Greg.Xrm.Mcp.AppMaker
```

Then add to `.vscode/mcp.json`:

```json
{
  "servers": {
    "AppMaker": {
      "type": "stdio",
      "command": "Greg.Xrm.Mcp.AppMaker",
      "args": [
        "--dataverseUrl",
        "https://yourorg.crm.dynamics.com"
      ]
    }
  }
}
```

### VS Code — `.vscode/mcp.json` (stdio, local source build)

```json
{
  "servers": {
    "AppMaker": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\path\\to\\Greg.Xrm.Mcp\\src\\Greg.Xrm.Mcp.AppMaker\\Greg.Xrm.Mcp.AppMaker.csproj",
        "--",
        "--dataverseUrl",
        "https://yourorg.crm.dynamics.com"
      ]
    }
  }
}
```

### VS Code — `.vscode/mcp.json` (SSE server)

Start the SSE server first (`dotnet run` in `Greg.Xrm.Mcp.AppMaker.SseServer`, listens on `http://localhost:22000` by default), then connect:

```json
{
  "servers": {
    "AppMaker": {
      "type": "sse",
      "url": "http://localhost:22000/sse",
      "headers": {
        "X-Dataverse-Url": "https://yourorg.crm.dynamics.com"
      }
    }
  }
}
```

### Claude Desktop — `claude_desktop_config.json`

```json
{
  "mcpServers": {
    "AppMaker": {
      "command": "Greg.Xrm.Mcp.AppMaker",
      "args": [
        "--dataverseUrl",
        "https://yourorg.crm.dynamics.com"
      ]
    }
  }
}
```

> **First run:** Watch the terminal or VS Code Output panel for the device-code prompt. Enter the code at [https://login.microsoft.com/device](https://login.microsoft.com/device). Subsequent runs use the cached token automatically.


## 🤝 Contributing

We welcome contributions:
- 🐛 Bug reports & feature requests
- 🧩 New specialized MCP servers
- 📖 Documentation improvements
- 🛠️ Core enhancements / performance tuning

---

## 🏷️ License

This project is licensed under the terms specified in the [LICENSE](LICENSE) file.

---

## 🏳️ Important Disclaimer 🏳️

- The toolset is in preview and provided as-is.
- Always export / backup solutions before applying modifications.
- LLM-assisted operations (form editing/cleanup) are inherently non-deterministic.
- Read-only tools (listing & inspection) are safe for production observation.
- The author is not responsible for misuse leading to unintended customization changes.

---

Made with ❤️ for the Power Platform community.
