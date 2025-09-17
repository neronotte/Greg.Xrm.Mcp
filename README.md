# 🚀 Greg.Xrm.Mcp

**A Comprehensive Framework for Building Model Context Protocol (MCP) Servers for Microsoft Dataverse**

[![.NET 9](https://img.shields.io/badge/.NET-9-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![License](https://img.shields.io/github/license/neronotte/Greg.Xrm.Mcp)](https://github.com/neronotte/Greg.Xrm.Mcp/blob/main/LICENSE)
[![GitHub Issues](https://img.shields.io/github/issues/neronotte/Greg.Xrm.Mcp)](https://github.com/neronotte/Greg.Xrm.Mcp/issues)
[![Build and Release](https://github.com/neronotte/Greg.Xrm.Mcp/actions/workflows/build-and-release.yml/badge.svg?branch=main)](https://github.com/neronotte/Greg.Xrm.Mcp/actions/workflows/build-and-release.yml)

---

## 👁️ Vision

Transform your Dataverse development experience with AI-powered tools that understand your business context. **Greg.Xrm.Mcp** is a **foundational framework** designed to revolutionize how developers interact with Microsoft Dataverse through intelligent MCP-enabled AI assistants (VS Code Copilot MCP extension, Claude Desktop, and other MCP clients).

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

The tool uses OAuth authentication to connect to Dataverse. You can simply provide your Dataverse URL as a command-line argument, as described above.
The tool will then prompt you to authenticate via a browser window. The authentication is managed by official `Microsoft.PowerPlatform.Dataverse.Client` library.
Authentication tokens are cached locally for reuse.

---

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
