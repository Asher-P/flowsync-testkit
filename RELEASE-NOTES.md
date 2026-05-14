# Release Notes

## Version 2.0.0 - December 8, 2024

### 🚨 BREAKING CHANGES

This is a major version release with breaking changes. The `MessageHookService` project has been renamed to `MessageHook.Orchestration` to better reflect its purpose as an orchestration framework for integration testing.

#### Package & Namespace Rename

**Before (v1.x)**
```csharp
// Package
<PackageReference Include="MessageHookService" Version="1.x.x" />

// Namespaces
using MessageHookService.Configurations;
using MessageHookService.Factories;
using MessageHookService.Host;
using MessageHookService.Entities.Interfaces;
```

**After (v2.0.0)**
```csharp
// Package
<PackageReference Include="MessageHook.Orchestration" Version="2.0.0" />

// Namespaces
using MessageHook.Orchestration.Configurations;
using MessageHook.Orchestration.Factories;
using MessageHook.Orchestration.Host;
using MessageHook.Orchestration.Entities.Interfaces;
```

### ✨ What's New

#### Better Naming Convention
- **Framework vs Service**: The name now correctly identifies the package as an orchestration framework rather than a service
- **Consistency**: Aligns with other packages in the MessageHook suite (`MessageHook.Core`, `MessageHook.Kafka`)
- **Industry Standard**: Uses the term "Orchestration" which is standard in microservices and testing frameworks

#### Improved Documentation
- **Variable Naming**: README examples now use `MessageHookStep` instead of confusing `MessageHookService`
- **Code Clarity**: Variable names now match their actual types (`IMessageHookStep` from `CreateMessageHookStepAsync`)
- **Migration Guide**: Comprehensive `MIGRATION.md` with step-by-step instructions

#### Code Cleanup
- Removed unused/commented-out files:
  - `IMessageHookService.cs` - replaced by `IMessageHookStep` pattern
  - `MessageHookManager.cs` - replaced by `BaseMessageHookStep` and implementations
- Cleaner codebase with no dead code

### 📦 Package Information

- **Package ID**: `MessageHook.Orchestration`
- **Version**: `2.0.0`
- **Assembly Name**: `MessageHook.Orchestration.dll`
- **Target Framework**: .NET 8.0
- **NuGet Package**: `MessageHook.Orchestration.2.0.0.nupkg`

### 🔄 Migration Required

All consumers must update their code. See [MIGRATION.md](MIGRATION.md) for detailed instructions.

**Quick Migration Steps:**
1. Update package reference to `MessageHook.Orchestration` v2.0.0
2. Find and replace: `using MessageHookService` → `using MessageHook.Orchestration`
3. Update project references if using direct project reference
4. Rebuild and test

### 📝 Full Changelog

#### Changed
- **[BREAKING]** Project renamed: `MessageHookService` → `MessageHook.Orchestration`
- **[BREAKING]** All namespaces updated: `MessageHookService.*` → `MessageHook.Orchestration.*`
- **[BREAKING]** Package ID changed: `MessageHookService` → `MessageHook.Orchestration`
- Assembly name updated to `MessageHook.Orchestration`
- Root namespace updated to `MessageHook.Orchestration`
- README examples updated with clearer variable naming (`MessageHookStep` vs `MessageHookService`)

#### Added
- NuGet package metadata (Authors, Description)
- Comprehensive migration guide (`MIGRATION.md`)
- This release notes file

#### Removed
- Dead code: `IMessageHookService.cs` (unused interface)
- Dead code: `MessageHookManager.cs` (unused class)
- Confusing variable names in documentation

#### Fixed
- Variable naming in README now matches actual return types
- Project references in dependent projects updated
- Solution file updated with correct paths

### 🔧 Technical Details

#### Files Changed
- 24 files changed in the main refactoring
- All C# files in `MessageHook.Orchestration` namespace updated
- 3 dependent projects updated (`MessageHook.Kafka`, `MessageHook.Tests`, `MessageHook.NUnit`)
- Solution file updated
- README updated

#### API Compatibility
✅ **All public APIs remain identical:**
- `IMessageHookFactory.CreateMessageHookStepAsync()`
- `IMessageHookStep.ExecuteAsync()`
- `MessageHookConfiguration`
- `ConsumingOptionsConfiguration`
- Extension method `AddMessageHookService()`

**No behavioral changes** - only naming/organizational changes.

### 📚 Documentation

- **README.md**: Updated with new namespaces and improved examples
- **MIGRATION.md**: Complete migration guide with examples
- **RELEASE-NOTES.md**: This file

### 🐛 Known Issues

None - this is purely a refactoring/renaming release with no functional changes.

### ⚙️ Dependencies

No changes to dependencies:
- MessageHook.Core
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

Previous versions used the `MessageHookService` naming. These versions are deprecated and consumers should migrate to v2.0.0+.

---

**Released**: December 8, 2024  
**Package Feed**: nuget.org

