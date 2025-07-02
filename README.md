# 🚀 Greg.Xrm.Mcp

**A Comprehensive Framework for Building Model Context Protocol (MCP) Servers for Microsoft Dataverse**

[![.NET 9](https://img.shields.io/badge/.NET-9-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![License](https://img.shields.io/github/license/neronotte/Greg.Xrm.Mcp)](https://github.com/neronotte/Greg.Xrm.Mcp/blob/main/LICENSE)
[![GitHub Issues](https://img.shields.io/github/issues/neronotte/Greg.Xrm.Mcp)](https://github.com/neronotte/Greg.Xrm.Mcp/issues)

---

## 👁️ Vision

Transform your Dataverse development experience with AI-powered tools that understand your business context. **Greg.Xrm.Mcp** is not just another development tool—it's a **foundational framework** designed to revolutionize how developers interact with Microsoft Dataverse through intelligent AI assistants like GitHub Copilot.

## 🏗️ Framework Architecture

At its core, **Greg.Xrm.Mcp** provides a robust foundation for building specialized MCP servers that seamlessly integrate with Dataverse ecosystems:

### **Greg.Xrm.Mcp.Core** - The Foundation

- 🔐 **Unified Authentication**: Standardized Dataverse connection and token management
- 🔧 **Common Services**: Reusable components for metadata, queries, and operations
- � **MCP Integration**: Built-in Model Context Protocol server capabilities
- 🛡️ **Error Handling**: Comprehensive error management and logging
- ⚡ **Performance**: Optimized for real-time AI assistant interactions

### **Specialized MCP Servers** - Domain-Specific Solutions

Build targeted solutions for specific Dataverse scenarios using the core framework, enabling:

- 🎯 **Domain Expertise**: Focused tools for forms, security, workflows, and more
- 🔗 **Consistent Patterns**: Standardized authentication and connection handling
- 📚 **Rapid Development**: Leverage proven components and patterns
- 🌟 **Extensible Design**: Add new capabilities without starting from scratch

## 📦 Current Implementation

### 🎨 Greg.Xrm.Mcp.FormEngineer

**AI-Powered Dataverse Form Engineering**

The flagship implementation of the framework, **Greg.Xrm.Mcp.FormEngineer** demonstrates the power of specialized MCP servers for Dataverse development. This server revolutionizes form development by providing:

- **🔍 Intelligent Form Discovery**: Smart retrieval and filtering of Dataverse forms
- **🎨 Natural Language Form Editing**: Modify forms using conversational AI commands
- **📋 Schema Validation**: Real-time FormXML validation against official schemas
- **🔄 Automated Publishing**: Seamless form updates with intelligent publishing
- **💡 Best Practices**: Automated form cleanup and standardization

> 👉 **For detailed installation instructions, usage examples, and advanced features, see [Greg.Xrm.Mcp.FormEngineer/README.md](src/Greg.Xrm.Mcp.FormEngineer/README.md)**

## 🤝 Contributing

We welcome contributions! Whether you're:

- 🐛 **Reporting bugs** or requesting features
- 💡 **Building new specialized MCP servers** using our framework
- 📖 **Improving documentation** and examples
- 🔧 **Enhancing the core framework** capabilities

## 🚀 Quick Start

### Prerequisites

- **.NET 9 SDK** or later
- **Visual Studio Code** with GitHub Copilot extension
- **Microsoft Dataverse environment** access

### Installation

Install the FormEngineer MCP server as a global tool:

```powershell
dotnet tool install --global Greg.Xrm.Mcp.FormEngineer
```

For detailed setup and configuration instructions, see the [FormEngineer documentation](src/Greg.Xrm.Mcp.FormEngineer/README.md).

## 🏷️ License

This project is licensed under the terms specified in the [LICENSE](LICENSE) file.

## 🏳️ Important Disclaimer 🏳️

The tool is in preview and provided as it is.
Always backup your existing customizations before making any changes.
The brain of the tool is your favorite LLM companion, that by definition is not deterministic.
The author is not responsible of any issue that can be generated on your form customization by the misuse of the tool.

---

Made with ❤️ for the Power Platform community.
