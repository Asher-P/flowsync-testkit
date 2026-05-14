# Migration Guide: FlowSyncService → FlowSync.Orchestration

## Overview

Version 2.0.0 introduces a **breaking change**: the `FlowSyncService` project has been renamed to `FlowSync.Orchestration` to better reflect its purpose as an orchestration framework for integration testing.

## What Changed?

### Package Name
- **Old**: `FlowSyncService`
- **New**: `FlowSync.Orchestration`

### Version
- **Old**: 1.x.x
- **New**: 2.0.0 (major version bump due to breaking changes)

### Namespaces
All namespaces have been updated:

| Old Namespace | New Namespace |
|---------------|---------------|
| `FlowSyncService` | `FlowSync.Orchestration` |
| `FlowSyncService.Configurations` | `FlowSync.Orchestration.Configurations` |
| `FlowSyncService.Entities` | `FlowSync.Orchestration.Entities` |
| `FlowSyncService.Entities.Enums` | `FlowSync.Orchestration.Entities.Enums` |
| `FlowSyncService.Entities.Interfaces` | `FlowSync.Orchestration.Entities.Interfaces` |
| `FlowSyncService.Factories` | `FlowSync.Orchestration.Factories` |
| `FlowSyncService.Host` | `FlowSync.Orchestration.Host` |

## Migration Steps

### Step 1: Update Project References

If you're referencing the project directly:

```xml
<!-- OLD -->
<ProjectReference Include="../FlowSyncService/FlowSyncService.csproj" />

<!-- NEW -->
<ProjectReference Include="../FlowSync.Orchestration/FlowSync.Orchestration.csproj" />
```

### Step 2: Update NuGet Package Reference

If you're consuming via NuGet:

```xml
<!-- OLD -->
<PackageReference Include="FlowSyncService" Version="1.x.x" />

<!-- NEW -->
<PackageReference Include="FlowSync.Orchestration" Version="2.0.0" />
```

### Step 3: Update Using Statements

Update all using statements in your code:

```csharp
// OLD
using FlowSyncService.Configurations;
using FlowSyncService.Factories;
using FlowSyncService.Host;
using FlowSyncService.Entities.Interfaces;

// NEW
using FlowSync.Orchestration.Configurations;
using FlowSync.Orchestration.Factories;
using FlowSync.Orchestration.Host;
using FlowSync.Orchestration.Entities.Interfaces;
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
2. Find: `using FlowSyncService`
3. Replace with: `using FlowSync.Orchestration`
4. Click "Replace All"

### Command Line (Unix/Mac)
```bash
# Update using statements
find . -name "*.cs" -type f -exec sed -i '' 's/using FlowSyncService/using FlowSync.Orchestration/g' {} +

# Update project references in .csproj files
find . -name "*.csproj" -type f -exec sed -i '' 's/FlowSyncService\.csproj/FlowSync.Orchestration.csproj/g' {} +
find . -name "*.csproj" -type f -exec sed -i '' 's/Include="FlowSyncService"/Include="FlowSync.Orchestration"/g' {} +
```

### Command Line (Linux)
```bash
# Update using statements
find . -name "*.cs" -type f -exec sed -i 's/using FlowSyncService/using FlowSync.Orchestration/g' {} +

# Update project references in .csproj files
find . -name "*.csproj" -type f -exec sed -i 's/FlowSyncService\.csproj/FlowSync.Orchestration.csproj/g' {} +
find . -name "*.csproj" -type f -exec sed -i 's/Include="FlowSyncService"/Include="FlowSync.Orchestration"/g' {} +
```

## Example Migration

### Before (v1.x)

```csharp
using FlowSync.Kafka.Extensions;
using FlowSyncService.Configurations;
using FlowSyncService.Factories;
using FlowSyncService.Host;

public class GeneralTests
{
    private IServiceProvider _provider;
    
    public void SetUp()
    {
        var services = new ServiceCollection();
        
        services.AddKafkaFlowSync(builder =>
        {
            // Kafka configuration
        });
        
        var FlowSyncFactory = _provider.GetRequiredService<IFlowSyncFactory>();
        var FlowSyncStep = await FlowSyncFactory.CreateFlowSyncStepAsync(
            new FlowSyncConfiguration()
            {
                // Configuration
            });
    }
}
```

### After (v2.0)

```csharp
using FlowSync.Kafka.Extensions;
using FlowSync.Orchestration.Configurations;
using FlowSync.Orchestration.Factories;
using FlowSync.Orchestration.Host;

public class GeneralTests
{
    private IServiceProvider _provider;
    
    public void SetUp()
    {
        var services = new ServiceCollection();
        
        services.AddKafkaFlowSync(builder =>
        {
            // Kafka configuration (unchanged)
        });
        
        var FlowSyncFactory = _provider.GetRequiredService<IFlowSyncFactory>();
        var FlowSyncStep = await FlowSyncFactory.CreateFlowSyncStepAsync(
            new FlowSyncConfiguration()
            {
                // Configuration (unchanged)
            });
    }
}
```

## What Hasn't Changed?

### API Surface
All public APIs remain the same:
- `IFlowSyncFactory`
- `IFlowSyncStep`
- `FlowSyncConfiguration`
- `ConsumingOptionsConfiguration`
- Extension methods like `AddFlowSyncService()`

### Functionality
All functionality is identical - this is purely a naming/organizational change.

### Dependencies
FlowSync.Orchestration still depends on:
- `FlowSync.Core`
- Other dependencies remain unchanged

## Troubleshooting

### Build Error: "The type or namespace name 'FlowSyncService' could not be found"
**Solution**: Update all using statements from `FlowSyncService.*` to `FlowSync.Orchestration.*`

### Build Error: "The referenced project '../FlowSyncService/FlowSyncService.csproj' does not exist"
**Solution**: Update project references in your `.csproj` files to point to `FlowSync.Orchestration.csproj`

### NuGet Restore Error: "Package 'FlowSyncService' not found"
**Solution**: Update package reference to `FlowSync.Orchestration` version 2.0.0

## Need Help?

If you encounter issues during migration:
1. Check that all using statements are updated
2. Verify all project/package references are updated
3. Clean and rebuild the solution
4. Review [RELEASE-NOTES.md](RELEASE-NOTES.md) for details

## Version Support

| Version | Status | Notes |
|---------|--------|-------|
| 2.0.0+ | ✅ Current | New name: FlowSync.Orchestration |
| 1.x.x | ⚠️ Deprecated | Old name: FlowSyncService |

---

**Last Updated**: December 8, 2024  
**Affected Versions**: 2.0.0+




