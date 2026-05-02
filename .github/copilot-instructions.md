# Copilot Instructions

## Project Guidelines
- When new code is created, associated unit tests should always be created.

### Versioning
- After a successful set of changes, increment the minor version (second segment) by 1 in all RSLLC assembly .csproj files for the FileVersion, AssemblyVersion, and Version properties.
- Ensure all assemblies share the same version number.
- If versions are mismatched across .csproj files, find the highest existing version and use that as the base before incrementing the minor segment.
- Example: 1.2.3.4 → 1.3.3.4

## C# Code Style
- Do not use `using static` in C#; it obscures the origin of members and makes code harder to read.

## Naming & Code Structure Conventions
- Use PascalCase for public properties and constants (e.g., MyProperty; public const string ConfigSectionName = "...").
- Use PascalCase for private fields with no underscore prefix (e.g., private readonly ILogger Logger;).
- Use camelCase for local variables and method parameters (e.g., var myVar; string myParam).
- Suffix enumerations with "Enum" (e.g., CrmLeadFieldEnum).
- Prefix interfaces with "I" (e.g., IStorageProvider).
- Use the "Base" suffix for abstract base classes (Microsoft convention), e.g., LoggingDisposableBase, ModifyableBase.
- When modifying existing code that uses a "Base" prefix, do not rename existing types unless a rename is explicitly in scope; new code should use the "Base" suffix.
- Prefix abstract base classes with "Base" (e.g., BaseLoggingDisposable, BaseModifyable).
- Suffix configuration/options classes with "Config" (NOT "Options") (e.g., StorageProviderTypeNameSelectorConfig).
- Suffix async methods with "Async" (e.g., ExecuteAsync).
- Prefer explicit interface implementations to hide interface members from the public surface.
- Use nested static classes to group related string constants (e.g., FieldNames, ConfigSectionName).
- Use #region blocks to group related members (e.g., #region IModifyable Members).

## Requirements for Merging & Maintenance
- Check for redundant instructions before adding new ones; enhance existing instructions with more specific or comprehensive guidance rather than duplicating.
- Group semantically related instructions together; place general guidance before specific rules.
- Keep instructions concise, actionable, and in the imperative mood.
- Maintain consistent formatting and spacing; use bullet lists for instruction sets.