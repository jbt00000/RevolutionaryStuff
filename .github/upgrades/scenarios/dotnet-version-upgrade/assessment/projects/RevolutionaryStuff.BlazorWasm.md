# src\RevolutionaryStuff.BlazorWasm\RevolutionaryStuff.BlazorWasm.csproj

[← Back to the assessment index](../../assessment.md)

## Project Info

- **Current Target Framework:** net10.0
- **Proposed Target Framework:** net11.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 1
- **Dependants**: 1
- **Number of Files**: 9
- **Number of Files with Incidents**: 2
- **Lines of Code**: 168
- **Estimated LOC to modify**: 2+ (at least 1.2% of the project)

## Related Projects

**Depends on (1)** — projects this one references:

- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Core\RevolutionaryStuff.Core.csproj](../projects/RevolutionaryStuff.Core.md)

**Depended on by (1)** — projects that reference this one:

- [C:\src\jbt00000\RevolutionaryStuff\tests\RevolutionaryStuff.BlazorWasm.Tests\RevolutionaryStuff.BlazorWasm.Tests.csproj](../projects/RevolutionaryStuff.BlazorWasm.Tests.md)

## Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (1)"]
        P23["<b>📦&nbsp;RevolutionaryStuff.BlazorWasm.Tests.csproj</b><br/><small>net10.0</small>"]
        click P23 "../projects/RevolutionaryStuff.BlazorWasm.Tests.md"
    end
    subgraph current["RevolutionaryStuff.BlazorWasm.csproj"]
        MAIN["<b>📦&nbsp;RevolutionaryStuff.BlazorWasm.csproj</b><br/><small>net10.0</small>"]
        click MAIN "../projects/RevolutionaryStuff.BlazorWasm.md"
    end
    subgraph downstream["Dependencies (1)"]
        P8["<b>📦&nbsp;RevolutionaryStuff.Core.csproj</b><br/><small>net10.0</small>"]
        click P8 "../projects/RevolutionaryStuff.Core.md"
    end
    P23 --> MAIN
    MAIN --> P8

```

## API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 2 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 185 |  |
| ***Total APIs Analyzed*** | ***187*** |  |

## NuGet Package Issues

| Package | Current Version | Suggested Version | Severity | Issue |
| :--- | :---: | :---: | :---: | :--- |
| Microsoft.AspNetCore.Components.WebAssembly | 10.0.15 | 11.0.0 | 🟡 Potential | NuGet package upgrade is recommended |
| Microsoft.Extensions.Logging.Debug | 10.0.15 | 11.0.0 | 🟡 Potential | NuGet package upgrade is recommended |

Every project affected by these packages, and the versions the repository settles on: [aggregate NuGet packages](../nuget/aggregate-packages.md).

