# tests\RevolutionaryStuff.Data.JsonStore.Cosmos.Tests\RevolutionaryStuff.Data.JsonStore.Cosmos.Tests.csproj

[← Back to the assessment index](../../assessment.md)

## Project Info

- **Current Target Framework:** net10.0
- **Proposed Target Framework:** net11.0
- **SDK-style**: True
- **Project Kind:** DotNetCoreApp
- **Dependencies**: 1
- **Dependants**: 0
- **Number of Files**: 4
- **Number of Files with Incidents**: 2
- **Lines of Code**: 353
- **Estimated LOC to modify**: 1+ (at least 0.3% of the project)

## Related Projects

**Depends on (1)** — projects this one references:

- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Data.JsonStore.Cosmos\RevolutionaryStuff.Data.JsonStore.Cosmos.csproj](../projects/RevolutionaryStuff.Data.JsonStore.Cosmos.md)

## Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["RevolutionaryStuff.Data.JsonStore.Cosmos.Tests.csproj"]
        MAIN["<b>📦&nbsp;RevolutionaryStuff.Data.JsonStore.Cosmos.Tests.csproj</b><br/><small>net10.0</small>"]
        click MAIN "../projects/RevolutionaryStuff.Data.JsonStore.Cosmos.Tests.md"
    end
    subgraph downstream["Dependencies (1)"]
        P12["<b>📦&nbsp;RevolutionaryStuff.Data.JsonStore.Cosmos.csproj</b><br/><small>net10.0</small>"]
        click P12 "../projects/RevolutionaryStuff.Data.JsonStore.Cosmos.md"
    end
    MAIN --> P12

```

## API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 1 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 530 |  |
| ***Total APIs Analyzed*** | ***531*** |  |

