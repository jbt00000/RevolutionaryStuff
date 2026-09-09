# OpenAPI Package Upgrade

## Preferences
- **Flow Mode**: Automatic
- **Scope**: RevolutionaryStuff.slnx, relevant tests, and repository-wide RSLLC assembly version synchronization.
- **Packages**: Microsoft.OpenApi and Microsoft.AspNetCore.OpenApi (verified name).
- **Version Policy**: Latest stable mutually compatible versions supporting existing .NET 10 targets, selected by package assessment; no previews or framework upgrade.
- **Assessment**: Quick API diff plus focused source inspection. Optional full semantic scan offered, not selected.

## Source Control
- **Source Branch**: master
- **Working Branch**: nuget-package-upgrade-openapi
- **Commit Strategy**: After Each Task
- **Branch Sync**: Auto (Merge)

## User Preferences
### Technical Preferences
- Follow .github/copilot-instructions.md. New code requires associated unit tests.
- After successful changes, increment the minor segment by one for FileVersion, AssemblyVersion, and Version in all RSLLC assembly csproj files. Use the highest existing version as base and keep all equal.
- No using static. IServiceCollection extension methods return IServiceCollection.
- PascalCase public properties/constants and private fields without underscore; camelCase locals/parameters; enum suffix Enum; interface prefix I; new abstract types suffix Base; do not rename existing Base-prefixed types unless in scope; configuration types suffix Config; async methods suffix Async.
- Prefer explicit interface implementations, nested static classes for related constants, and region blocks for related members.
### Execution Style
- Keep changes minimal, inspect actual workspace, prioritize Razor Pages where relevant, plan multi-phase work, validate builds, and discover/run relevant tests.
- Deliver workspace fixes and a concise summary of changes and validation.

## Key Decisions Log
- User confirmed all proposed initialization settings, including stable compatible target policy and new working branch.
