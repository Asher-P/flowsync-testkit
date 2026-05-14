# Release Notes

## Version 2.0.0 - December 8, 2024

### 🚨 BREAKING CHANGES

This is a major version release with breaking changes. The `FlowSyncService` project has been renamed to `FlowSync.Orchestration` to better reflect its purpose as an orchestration framework for integration testing.

#### Package & Namespace Rename

**Before (v1.x)**
```csharp
// Package
<PackageReference Include="FlowSyncService" Version="1.x.x" />

// Namespaces
using FlowSyncService.Configurations;
using FlowSyncService.Factories;
using FlowSyncService.Host;
using FlowSyncService.Entities.Interfaces;
```

**After (v2.0.0)**
```csharp
// Package
<PackageReference Include="FlowSync.Orchestration" Version="2.0.0" />

// Namespaces
using FlowSync.Orchestration.Configurations;
using FlowSync.Orchestration.Factories;
using FlowSync.Orchestration.Host;
using FlowSync.Orchestration.Entities.Interfaces;
```

### ✨ What's New

#### Better Naming Convention
- **Framework vs Service**: The name now correctly identifies the package as an orchestration framework rather than a service
- **Consistency**: Aligns with other packages in the FlowSync suite (`FlowSync.Core`, `FlowSync.Kafka`)
- **Industry Standard**: Uses the term "Orchestration" which is standard in microservices and testing frameworks

#### Improved Documentation
- **Variable Naming**: README examples now use `FlowSyncStep` instead of confusing `FlowSyncService`
- **Code Clarity**: Variable names now match their actual types (`IFlowSyncStep` from `CreateFlowSyncStepAsync`)
- **Migration Guide**: Comprehensive `MIGRATION.md` with step-by-step instructions

#### Code Cleanup
- Removed unused/commented-out files:
  - `IFlowSyncService.cs` - replaced by `IFlowSyncStep` pattern
  - `FlowSyncManager.cs` - replaced by `BaseFlowSyncStep` and implementations
- Cleaner codebase with no dead code

### 📦 Package Information

- **Package ID**: `FlowSync.Orchestration`
- **Version**: `2.0.0`
- **Assembly Name**: `FlowSync.Orchestration.dll`
- **Target Framework**: .NET 8.0
- **NuGet Package**: `FlowSync.Orchestration.2.0.0.nupkg`

### 🔄 Migration Required

All consumers must update their code. See [MIGRATION.md](MIGRATION.md) for detailed instructions.

**Quick Migration Steps:**
1. Update package reference to `FlowSync.Orchestration` v2.0.0
2. Find and replace: `using FlowSyncService` → `using FlowSync.Orchestration`
3. Update project references if using direct project reference
4. Rebuild and test

### 📝 Full Changelog

#### Changed
- **[BREAKING]** Project renamed: `FlowSyncService` → `FlowSync.Orchestration`
- **[BREAKING]** All namespaces updated: `FlowSyncService.*` → `FlowSync.Orchestration.*`
- **[BREAKING]** Package ID changed: `FlowSyncService` → `FlowSync.Orchestration`
- Assembly name updated to `FlowSync.Orchestration`
- Root namespace updated to `FlowSync.Orchestration`
- README examples updated with clearer variable naming (`FlowSyncStep` vs `FlowSyncService`)

#### Added
- NuGet package metadata (Authors, Description)
- Comprehensive migration guide (`MIGRATION.md`)
- This release notes file

#### Removed
- Dead code: `IFlowSyncService.cs` (unused interface)
- Dead code: `FlowSyncManager.cs` (unused class)
- Confusing variable names in documentation

#### Fixed
- Variable naming in README now matches actual return types
- Project references in dependent projects updated
- Solution file updated with correct paths

### 🔧 Technical Details

#### Files Changed
- 24 files changed in the main refactoring
- All C# files in `FlowSync.Orchestration` namespace updated
- 3 dependent projects updated (`FlowSync.Kafka`, `FlowSync.Tests`, `FlowSync.NUnit`)
- Solution file updated
- README updated

#### API Compatibility
✅ **All public APIs remain identical:**
- `IFlowSyncFactory.CreateFlowSyncStepAsync()`
- `IFlowSyncStep.ExecuteAsync()`
- `FlowSyncConfiguration`
- `ConsumingOptionsConfiguration`
- Extension method `AddFlowSyncService()`

**No behavioral changes** - only naming/organizational changes.

### 📚 Documentation

- **README.md**: Updated with new namespaces and improved examples
- **MIGRATION.md**: Complete migration guide with examples
- **RELEASE-NOTES.md**: This file

### 🐛 Known Issues

None - this is purely a refactoring/renaming release with no functional changes.

### ⚙️ Dependencies

No changes to dependencies:
- FlowSync.Core
- All other dependencies remain unchanged

### 🔐 Backward Compatibility

**Not backward compatible** - this is a breaking change requiring consumer updates.

**Deprecation Plan:**
- v1.x packages will remain available in the feed
- v1.x is now deprecated and should not be used for new projects
- Support for v1.x will end after consumers migrate

### 📞 Support & Help

If you encounter migration issues:
1. Review [MIGRATION.md](MIGRATION.md)
2. Check the troubleshooting section
3. Contact the Data Integrity team
4. Open an issue in the repository

### 🎯 Upgrade Recommendation

**Recommended for all users**: This rename improves code clarity and aligns with best practices. While it requires code updates, the migration is straightforward and well-documented.

---

## Version 1.x.x (Legacy - Deprecated)

Previous versions used the `FlowSyncService` naming. These versions are deprecated and consumers should migrate to v2.0.0+.

---

**Released**: December 8, 2024  
**Package Feed**: nuget.org

