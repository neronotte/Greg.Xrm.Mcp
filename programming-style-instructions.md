# Greg.Xrm.Mcp Programming Style Instructions

## 📋 Overview
This document outlines the programming patterns, conventions, and best practices used throughout the Greg.Xrm.Mcp codebase. Follow these guidelines when contributing new features or modifying existing code.

## 🏗️ Architecture Principles

### Layered Architecture
The solution follows a clear separation of concerns:
- **Greg.Xrm.Mcp.Core**: Foundation layer with authentication, logging, and common services
- **Greg.Xrm.Mcp.Engine**: Business logic, data models, services, and MCP tools
- **Greg.Xrm.Mcp.AppMaker**: Application entry point and hosting configuration
- **Greg.Xrm.Mcp.Monitor**: Monitoring and telemetry
- **Greg.Xrm.Mcp.AppMaker.SseServer**: SSE-based server transport

### Dependency Flow
- Engine depends on Core
- AppMaker depends on Engine and Core
- Never create circular dependencies

## 🔧 MCP Tools Pattern

### Tool Structure
All MCP tools follow a consistent structure:

1. **File Organization**: Place tools in logical folders under `Greg.Xrm.Mcp.Engine/Server/Tools/`
   - Forms, Views, SiteMaps, AppModules, Charts, etc.

2. **Class Declaration**:
```csharp
[McpServerToolType]
public class ToolName(
    ILogger<ToolName> logger,
    IDataverseClientProvider clientProvider,
    IOtherDependencies...)
{
    // Tool implementation
}
```

3. **Method Signature**:
```csharp
[McpServerTool(
    Name = "dataverse_entity_operation",
    Destructive = false,  // true if modifies data
    ReadOnly = true,      // false if writes data
    Idempotent = true),   // true if repeated calls have same effect
Description("Clear description of what the tool does")]
public async Task<string> Execute(
    [Description("Parameter description")] Type paramName,
    ...)
{
    // Implementation
}
```

### Tool Naming Convention
- Format: `dataverse_<entity>_<operation>`
- Examples:
  - `dataverse_form_retrieve_formxml`
  - `dataverse_view_get_definition`
  - `dataverse_chart_read`
  - `dataverse_chart_create`
  - `dataverse_chart_update`

### Tool Implementation Pattern
```csharp
logger.LogTrace("{ToolName} called with parameters: Param1={Param1}, Param2={Param2}",
    nameof(ToolName), param1, param2);

try
{
    // 1. Validate parameters
    if (string.IsNullOrWhiteSpace(param))
    {
        return "❌ Parameter is required.";
    }

    // 2. Authenticate
    logger.LogTrace("🔐 Authenticating to Dataverse");
    var client = await clientProvider.GetDataverseClientAsync();

    // 3. Perform operation
    logger.LogTrace("🔍 Performing operation...");
    var context = new DataverseContext(client);
    // ... operation logic

    // 4. Return success
    logger.LogTrace("✅ Operation completed successfully");
    return "✅ Success message";
}
catch (Exception ex)
{
    logger.LogError(ex, "❌ Error during operation: {Message}", ex.Message);
    return $"❌ Error: {ex.Message}";
}
```

## 📦 Data Models

### Entity Model Generation
- Models are auto-generated using Dataverse Model Builder
- Located in `Greg.Xrm.Mcp.Engine/Model/Entities/`
- Files have `#pragma warning disable CS1591` to suppress documentation warnings
- Include auto-generated header comment

### DataverseContext
- Central context for all Dataverse operations
- Provides strongly-typed queryable sets for each entity
- Example: `SavedQuerySet`, `SystemFormSet`, `SavedQueryVisualizationSet`

### Model Naming
- Entity classes: PascalCase (e.g., `SystemForm`, `SavedQuery`)
- Enum types: lowercase_underscore prefix (e.g., `systemform_type`, `savedquery_statecode`)
- Properties: PascalCase matching SDK conventions

## 🔍 Services Pattern

### Service Interface
```csharp
public interface IServiceName
{
    /// <summary>
    /// XML documentation for the method
    /// </summary>
    Task<ReturnType> MethodAsync(
        IOrganizationServiceAsync2 client,
        Type parameter,
        CancellationToken cancellationToken = default);
}
```

### Service Implementation
```csharp
public class ServiceName(ILogger<ServiceName> logger) : IServiceName
{
    public async Task<ReturnType> MethodAsync(
        IOrganizationServiceAsync2 client,
        Type parameter,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Operation description: {Param}", parameter);

            // Implementation

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during operation");
            throw new McpException(ex.Message, ex);
        }
    }
}
```

### Service Registration
Services are registered in `ApplicationBuilderExtensions.cs` or similar initialization code.

## 🎨 Coding Conventions

### Namespaces
- Core: `Greg.Xrm.Mcp.Core.*`
- Engine: `Greg.Xrm.Mcp.FormEngineer.*` or `Greg.Xrm.Mcp.*`
- Keep namespaces aligned with folder structure

### Using Statements
- Use `global using` for commonly used namespaces in project files
- Implicit usings enabled (`ImplicitUsings` in .csproj)

### Nullable Reference Types
- Enabled project-wide (`<Nullable>enable</Nullable>`)
- Use `?` for nullable reference types
- Use `null!` when you guarantee non-null but compiler can't infer

### Primary Constructors (C# 12)
Preferred for dependency injection:
```csharp
public class MyClass(
    ILogger<MyClass> logger,
    IService service)
{
    // No need to declare private fields or assign in body
    // Just use logger and service directly
}
```

### Logging

#### Log Levels
- `LogTrace`: Detailed execution flow, parameters, intermediate values
- `LogDebug`: Detailed diagnostic information
- `LogInformation`: General informational messages (use sparingly)
- `LogWarning`: Unexpected but recoverable situations
- `LogError`: Error events that still allow execution to continue

#### Emoji Conventions in Logs and Messages
- 🔐 Authentication
- 🔍 Searching/Querying
- 📋 Listing/Retrieving
- ✅ Success
- ❌ Error/Failure
- 🔄 Updating
- ➕ Creating/Adding
- 🧹 Cleaning/Deleting
- 🎯 Target/Focus
- 📦 Package/Bundle
- 🧬 Definition/Structure
- 🛡️ Validation/Security

#### Structured Logging
```csharp
logger.LogTrace("Operation started with {Parameter}", parameterValue);
// Not: logger.LogTrace($"Operation started with {parameterValue}");
```

### XML Documentation
- Minimal XML comments for auto-generated code
- Comprehensive XML documentation for:
  - Public interfaces
  - Service methods
  - Complex tool implementations
- Use `<summary>`, `<param>`, `<returns>`, `<remarks>`, `<example>` tags appropriately

### Error Handling

#### Return Error Messages
For MCP tools, return user-friendly error messages:
```csharp
return $"❌ Error: {ex.Message}";
```

#### Throw Exceptions
For services, throw meaningful exceptions:
```csharp
throw new McpException($"Failed to retrieve entity: {ex.Message}", ex);
throw new ArgumentException("entity_logical_name is required");
```

### Async/Await
- All Dataverse operations are async
- Use `async Task<T>` for all async methods
- Suffix async methods with `Async`
- Always await async calls; don't use `.Result` or `.Wait()`

## 🗃️ Data Access Patterns

### QueryExpression Pattern
```csharp
var query = new QueryExpression("entitylogicalname")
{
    ColumnSet = new ColumnSet("column1", "column2", "column3"),
    Criteria = new FilterExpression(LogicalOperator.And)
};

query.Criteria.AddCondition("fieldname", ConditionOperator.Equal, value);
query.AddOrder("fieldname", OrderType.Ascending);

var result = await client.RetrieveMultipleAsync(query);
```

### LINQ with DataverseContext
```csharp
var context = new DataverseContext(client);

var entity = context.EntitySet
    .Where(x => x.Id == id)
    .Select(x => new
    {
        x.Id,
        x.Name,
        x.Property
    })
    .FirstOrDefault();
```

### Create/Update Pattern
```csharp
var entity = new EntityType();
entity.Property1 = value1;
entity.Property2 = value2;

// Create
entity.Id = await client.CreateAsync(entity);

// Update
await client.UpdateAsync(entity);
```

### Publishing Changes
After modifying forms, views, charts, or other customizations:
```csharp
IPublishXmlBuilder publishXmlBuilder;

publishXmlBuilder.AddTable(tableName);
var request = publishXmlBuilder.Build();
await client.ExecuteAsync(request);
```

## 📊 Output Formatting

### Dual Format Support
Many tools support both human-readable and JSON output:

```csharp
if ("json".Equals(outputType, StringComparison.OrdinalIgnoreCase))
{
    var result = JsonConvert.SerializeObject(data, Formatting.Indented);
    return result;
}
else
{
    var sb = new StringBuilder();
    sb.Append("Entity Details:").AppendLine();
    sb.Append("- ID: ").Append(id).AppendLine();
    sb.Append("- Name: ").Append(name).AppendLine();
    return sb.ToString();
}
```

### Extension Methods for Formatting
Create extension methods for complex formatting:
```csharp
public static class EntityExtensions
{
    public static string FormatJsonOutput(this List<Entity> entities, string tableName)
    {
        // Format as JSON
    }

    public static string FormatXmlOutput(this List<Entity> entities, string tableName)
    {
        // Format as readable text
    }
}
```

## 📝 XML Schema Resources

### Embedded Resources
XML schemas are embedded as resources in the Engine project:
- `FormXml.xsd`
- `LayoutXml.xsd`
- `FetchXml.xsd`
- `SiteMap.xsd`
- Add new schemas (e.g., `VisualizationDataDescription.xsd`) following same pattern

### Resource Registration
Register in `.csproj`:
```xml
<EmbeddedResource Include="Resources\NewSchema.xsd">
    <CopyToOutputDirectory>Never</CopyToOutputDirectory>
    <SubType>Designer</SubType>
</EmbeddedResource>
```

### Exposing via MCP Resources
```csharp
[McpServerResource]
public static async Task<string> GetResourceAsync(
    [Description("The URI of the resource")] string uri,
    CancellationToken cancellationToken = default)
{
    return uri switch
    {
        "schema://schemaname" => await GetEmbeddedResourceAsync("Greg.Xrm.Mcp.Resources.SchemaFile.xsd"),
        _ => throw new ArgumentException($"Unknown resource: {uri}")
    };
}
```

## 🧪 Validation Pattern

### Schema Validation
For XML-based customizations (forms, views, charts):

1. Create validator interface:
```csharp
public interface IXmlValidator
{
    ValidationResult TryValidateXmlAgainstSchema(string xml);
}
```

2. Implement validator:
```csharp
public class XmlValidator : IXmlValidator
{
    public ValidationResult TryValidateXmlAgainstSchema(string xml)
    {
        var result = new ValidationResult();

        // Load schema from embedded resource
        // Validate XML
        // Populate result with errors/warnings

        return result;
    }
}
```

3. Register validator in DI container

4. Use in update operations:
```csharp
var validationResult = validator.TryValidateXmlAgainstSchema(xml);
if (!validationResult.IsValid)
{
    logger.LogError("Validation failed");
    return "❌ XML validation failed: " + string.Join(", ", validationResult);
}
```

## 🔄 Progress Notification

For long-running operations:
```csharp
public async Task<string> Execute(
    IMcpServer mcpServer,
    // other parameters
    )
{
    var token = await mcpServer.NotifyProgressAsync(nameof(ToolName), "Starting operation...");

    // Do work

    await mcpServer.NotifyProgressAsync(token, "Step 1 completed...", 33);

    // More work

    await mcpServer.NotifyProgressAsync(token, "Completed!", 100);

    return "✅ Success";
}
```

## 🎯 Best Practices Summary

1. **Consistency**: Follow established patterns for similar operations
2. **Logging**: Use structured logging with emojis for visual clarity
3. **Error Handling**: Return user-friendly messages from tools, throw exceptions from services
4. **Validation**: Validate early and provide clear error messages
5. **Documentation**: Document complex logic and public APIs
6. **Type Safety**: Leverage strong typing and nullable reference types
7. **Async**: Use async/await consistently
8. **Dependency Injection**: Use constructor injection with primary constructors
9. **Naming**: Use descriptive, consistent names following conventions
10. **XML/JSON**: Support both human-readable and machine-parseable output formats

## 🚀 Adding New Functionality Checklist

When adding new functionality (e.g., Chart support):

- [ ] Create entity model in `Model/Entities/` (or generate with Model Builder)
- [ ] Add entity set to `DataverseContext.cs`
- [ ] Create service interface in `Services/IServiceName.cs`
- [ ] Implement service in `Services/ServiceName.cs`
- [ ] Create tool classes in `Server/Tools/EntityName/`
  - [ ] Read/Get tool
  - [ ] Create tool
  - [ ] Update tool
  - [ ] List tool (if applicable)
  - [ ] Validation tool (if applicable)
- [ ] Add XML schemas as embedded resources (if applicable)
- [ ] Expose schemas via MCP Resources (if applicable)
- [ ] Create extension methods for output formatting
- [ ] Register services in DI container
- [ ] Add to ComponentType enum (if applicable)
- [ ] Update README.md with new capabilities
- [ ] Test with MCP Inspector

## 📚 Technology Stack

- **.NET 9.0**: Target framework
- **C# 12**: Language features (primary constructors, collection expressions, etc.)
- **Microsoft.PowerPlatform.Dataverse.Client**: Dataverse SDK
- **ModelContextProtocol**: MCP server implementation
- **Serilog**: Logging framework
- **Application Insights**: Optional telemetry
- **Newtonsoft.Json**: JSON serialization

## 🔗 References

- [Microsoft Dataverse SDK](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/)
- [Model Context Protocol](https://modelcontextprotocol.io/)
- [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
