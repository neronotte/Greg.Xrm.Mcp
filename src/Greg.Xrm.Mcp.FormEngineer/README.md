# 🚀 Greg.Xrm.Mcp.FormEngineer

Greg.Xrm.Mcp.FormEngineer is an **Advanced Model Context Protocol (MCP) Server for Microsoft Dataverse Form Engineering**.

---

## 🎯 Purpose

Greg.Xrm.Mcp.FormEngineer is a specialized **Model Context Protocol (MCP) server** that revolutionizes how developers interact with Microsoft Dataverse forms.
Transform your Dataverse form development experience with AI-powered tools that understand your business needs and deliver precise, schema-compliant form modifications at lightning speed.
By integrating directly with **GitHub Copilot** and other AI assistants, it provides intelligent, context-aware form manipulation capabilities that go far beyond traditional development tools.

### ✨ Key Capabilities

- **🎨 Intelligent Form Manipulation**: Modify Dataverse forms using natural language commands
- **🔍 Schema Validation**: Real-time FormXML validation against official Dataverse schemas  
- **📋 Form Discovery**: Smart form retrieval with type-aware filtering and selection
- **🔄 Automated Publishing**: Seamless form updates with automatic publishing to your environment
- **💡 Smart Defaults**: Intelligent form cleanup and standardization with best practices
- **📝 Multi-Format Support**: Work with forms in XML or JSON formats based on your preference

## 🚀 Installation for VSCode GitHub Copilot

### Prerequisites

- **Visual Studio Code** with GitHub Copilot extension
- **.NET 9 SDK** or later
- **Microsoft Dataverse environment** access
- **GitHub Copilot subscription**

### Step 1: Install the MCP Server

Choose your preferred installation method:

#### Option A: Install as Global Tool (Recommended)

```Powershell
dotnet tool install --global Greg.Xrm.Mcp.FormEngineer
```

#### Option B: Build from Source# Clone the repository

```Powershell
# Clone repo
git clone https://github.com/neronotte/Greg.Xrm.Mcp.git
cd Greg.Xrm.Mcp

# Build and install
dotnet build src/Greg.Xrm.Mcp.FormEngineer
dotnet tool install --global --add-source ./src/Greg.Xrm.Mcp.FormEngineer/bin/Debug Greg.Xrm.Mcp.FormEngineer
```

### Step 2: Configure GitHub Copilot

Add the MCP server configuration to your GitHub Copilot settings:

1. Open VSCode Worspace
2. Add a folder called `.vscode`
3. Within that folder, add a file called `mcp.json` with the following content:

```Json
{
  "servers": {
    "dataverse-form-engineer": {
      "command": "Greg.Xrm.Mcp.FormEngineer",
      "args": ["--dataverseUrl", "https://yourorg.crm.dynamics.com"],
      "cwd": "${workspaceFolder}"
    }
  }
}
```

## 💼 Use Cases & Examples

- 🎨 **Form Customization**: "Add a new tab called 'Project Details' to the account form with sections for timeline and budget information"
- 🔧 **Form Cleanup**: "Clean up the contact form by standardizing tab names and adding an administration section with audit fields"
- 📊 **Form Analysis**: "Show me all available forms for the opportunity table and their current structure"
- ✅ **Schema Validation**: "Validate this FormXML against the Dataverse schema before I deploy it"

---

## 📄 License

This project is licensed under the [MIT License](https://github.com/neronotte/Greg.Xrm.Mcp/blob/main/LICENSE).

## 🏳️ Important Disclaimer 🏳️

The tool is in preview and provided as it is.
Always backup your existing customizations before making any changes.
The brain of the tool is your favorite LLM companion, that by definition is not deterministic.
The author is not responsible of any issue that can be generated on your form customization by the misuse of the tool.

---

*Made with ❤️ for the Power Platform community*