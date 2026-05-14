# Migration Guide: MessageHookService → MessageHook.Orchestration

## Overview

Version 2.0.0 introduces a **breaking change**: the `MessageHookService` project has been renamed to `MessageHook.Orchestration` to better reflect its purpose as an orchestration framework for integration testing.

## What Changed?

### Package Name
- **Old**: `MessageHookService`
- **New**: `MessageHook.Orchestration`

### Version
- **Old**: 1.x.x
- **New**: 2.0.0 (major version bump due to breaking changes)

### Namespaces
All namespaces have been updated:

| Old Namespace | New Namespace |
|---------------|---------------|
| `MessageHookService` | `MessageHook.Orchestration` |
| `MessageHookService.Configurations` | `MessageHook.Orchestration.Configurations` |
| `MessageHookService.Entities` | `MessageHook.Orchestration.Entities` |
| `MessageHookService.Entities.Enums` | `MessageHook.Orchestration.Entities.Enums` |
| `MessageHookService.Entities.Interfaces` | `MessageHook.Orchestration.Entities.Interfaces` |
| `MessageHookService.Factories` | `MessageHook.Orchestration.Factories` |
| `MessageHookService.Host` | `MessageHook.Orchestration.Host` |

## Migration Steps

### Step 1: Update Project References

If you're referencing the project directly:

```xml
<!-- OLD -->
<ProjectReference Include="../MessageHookService/MessageHookService.csproj" />

<!-- NEW -->
<ProjectReference Include="../MessageHook.Orchestration/MessageHook.Orchestration.csproj" />
```

### Step 2: Update NuGet Package Reference

If you're consuming via NuGet:

```xml
<!-- OLD -->
<PackageReference Include="MessageHookService" Version="1.x.x" />

<!-- NEW -->
<PackageReference Include="MessageHook.Orchestration" Version="2.0.0" />
```

### Step 3: Update Using Statements

Update all using statements in your code:

```csharp
// OLD
using MessageHookService.Configurations;
using MessageHookService.Factories;
using MessageHookService.Host;
using MessageHookService.Entities.Interfaces;

// NEW
using MessageHook.Orchestration.Configurations;
using MessageHook.Orchestration.Factories;
using MessageHook.Orchestration.Host;
using MessageHook.Orchestration.Entities.Interfaces;
```

### Step 4: Rebuild and Test

```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
```

## Find and Replace Commands

For quick migration across multiple files:

### Visual Studio / Rider
1. Press `Ctrl+Shift+H` (Find and Replace in Files)
2. Find: `using MessageHookService`
3. Replace with: `using MessageHook.Orchestration`
4. Click "Replace All"

### Command Line (Unix/Mac)
```bash
# Update using statements
find . -name "*.cs" -type f -exec sed -i '' 's/using MessageHookService/using MessageHook.Orchestration/g' {} +

# Update project references in .csproj files
find . -name "*.csproj" -type f -exec sed -i '' 's/MessageHookService\.csproj/MessageHook.Orchestration.csproj/g' {} +
find . -name "*.csproj" -type f -exec sed -i '' 's/Include="MessageHookService"/Include="MessageHook.Orchestration"/g' {} +
```

### Command Line (Linux)
```bash
# Update using statements
find . -name "*.cs" -type f -exec sed -i 's/using MessageHookService/using MessageHook.Orchestration/g' {} +

# Update project references in .csproj files
find . -name "*.csproj" -type f -exec sed -i 's/MessageHookService\.csproj/MessageHook.Orchestration.csproj/g' {} +
find . -name "*.csproj" -type f -exec sed -i 's/Include="MessageHookService"/Include="MessageHook.Orchestration"/g' {} +
```

## Example Migration

### Before (v1.x)

```csharp
using MessageHook.Kafka.Extensions;
using MessageHookService.Configurations;
using MessageHookService.Factories;
using MessageHookService.Host;

public class GeneralTests
{
    private IServiceProvider _provider;
    
    public void SetUp()
    {
        var services = new ServiceCollection();
        
        services.AddKafkaMessageHook(builder =>
        {
            // Kafka configuration
        });
        
        var MessageHookFactory = _provider.GetRequiredService<IMessageHookFactory>();
        var MessageHookStep = await MessageHookFactory.CreateMessageHookStepAsync(
            new MessageHookConfiguration()
            {
                // Configuration
            });
    }
}
```

### After (v2.0)

```csharp
using MessageHook.Kafka.Extensions;
using MessageHook.Orchestration.Configurations;
using MessageHook.Orchestration.Factories;
using MessageHook.Orchestration.Host;

public class GeneralTests
{
    private IServiceProvider _provider;
    
    public void SetUp()
    {
        var services = new ServiceCollection();
        
        services.AddKafkaMessageHook(builder =>
        {
            // Kafka configuration (unchanged)
        });
        
        var MessageHookFactory = _provider.GetRequiredService<IMessageHookFactory>();
        var MessageHookStep = await MessageHookFactory.CreateMessageHookStepAsync(
            new MessageHookConfiguration()
            {
                // Configuration (unchanged)
            });
    }
}
```

## What Hasn't Changed?

### API Surface
All public APIs remain the same:
- `IMessageHookFactory`
- `IMessageHookStep`
- `MessageHookConfiguration`
- `ConsumingOptionsConfiguration`
- Extension methods like `AddMessageHookService()`

### Functionality
All functionality is identical - this is purely a naming/organizational change.

### Dependencies
MessageHook.Orchestration still depends on:
- `MessageHook.Core`
- Other dependencies remain unchanged

## Troubleshooting

### Build Error: "The type or namespace name 'MessageHookService' could not be found"
**Solution**: Update all using statements from `MessageHookService.*` to `MessageHook.Orchestration.*`

### Build Error: "The referenced project '../MessageHookService/MessageHookService.csproj' does not exist"
**Solution**: Update project references in your `.csproj` files to point to `MessageHook.Orchestration.csproj`

### NuGet Restore Error: "Package 'MessageHookService' not found"
**Solution**: Update package reference to `MessageHook.Orchestration` version 2.0.0

## Need Help?

If you encounter issues during migration:
1. Check that all using statements are updated
2. Verify all project/package references are updated
3. Clean and rebuild the solution
4. Review [RELEASE-NOTES.md](RELEASE-NOTES.md) for details

## Version Support

| Version | Status | Notes |
|---------|--------|-------|
| 2.0.0+ | ✅ Current | New name: MessageHook.Orchestration |
| 1.x.x | ⚠️ Deprecated | Old name: MessageHookService |

---

**Last Updated**: December 8, 2024  
**Affected Versions**: 2.0.0+




