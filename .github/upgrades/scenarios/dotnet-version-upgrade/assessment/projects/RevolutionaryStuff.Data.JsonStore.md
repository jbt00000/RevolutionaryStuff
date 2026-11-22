# src\RevolutionaryStuff.Data.JsonStore\RevolutionaryStuff.Data.JsonStore.csproj

[← Back to the assessment index](../../assessment.md)

## Project Info

- **Current Target Framework:** net10.0
- **Proposed Target Framework:** net11.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 1
- **Dependants**: 2
- **Number of Files**: 31
- **Number of Files with Incidents**: 2
- **Lines of Code**: 1385
- **Estimated LOC to modify**: 8+ (at least 0.6% of the project)

## Related Projects

**Depends on (1)** — projects this one references:

- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Core\RevolutionaryStuff.Core.csproj](../projects/RevolutionaryStuff.Core.md)

**Depended on by (2)** — projects that reference this one:

- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Data.JsonStore.Cosmos\RevolutionaryStuff.Data.JsonStore.Cosmos.csproj](../projects/RevolutionaryStuff.Data.JsonStore.Cosmos.md)
- [C:\src\jbt00000\RevolutionaryStuff\tests\RevolutionaryStuff.Data.JsonStore.Tests\RevolutionaryStuff.Data.JsonStore.Tests.csproj](../projects/RevolutionaryStuff.Data.JsonStore.Tests.md)

## Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (2)"]
        P12["<b>📦&nbsp;RevolutionaryStuff.Data.JsonStore.Cosmos.csproj</b><br/><small>net10.0</small>"]
        P25["<b>📦&nbsp;RevolutionaryStuff.Data.JsonStore.Tests.csproj</b><br/><small>net10.0</small>"]
        click P12 "../projects/RevolutionaryStuff.Data.JsonStore.Cosmos.md"
        click P25 "../projects/RevolutionaryStuff.Data.JsonStore.Tests.md"
    end
    subgraph current["RevolutionaryStuff.Data.JsonStore.csproj"]
        MAIN["<b>📦&nbsp;RevolutionaryStuff.Data.JsonStore.csproj</b><br/><small>net10.0</small>"]
        click MAIN "../projects/RevolutionaryStuff.Data.JsonStore.md"
    end
    subgraph downstream["Dependencies (1)"]
        P8["<b>📦&nbsp;RevolutionaryStuff.Core.csproj</b><br/><small>net10.0</small>"]
        click P8 "../projects/RevolutionaryStuff.Core.md"
    end
    P12 --> MAIN
    P25 --> MAIN
    MAIN --> P8

```

## API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 8 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 1272 |  |
| ***Total APIs Analyzed*** | ***1280*** |  |

