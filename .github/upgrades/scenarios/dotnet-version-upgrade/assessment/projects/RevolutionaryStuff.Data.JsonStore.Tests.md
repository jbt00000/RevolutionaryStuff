# tests\RevolutionaryStuff.Data.JsonStore.Tests\RevolutionaryStuff.Data.JsonStore.Tests.csproj

[← Back to the assessment index](../../assessment.md)

## Project Info

- **Current Target Framework:** net10.0
- **Proposed Target Framework:** net11.0
- **SDK-style**: True
- **Project Kind:** DotNetCoreApp
- **Dependencies**: 1
- **Dependants**: 0
- **Number of Files**: 3
- **Number of Files with Incidents**: 1
- **Lines of Code**: 257
- **Estimated LOC to modify**: 0+ (at least 0.0% of the project)

## Related Projects

**Depends on (1)** — projects this one references:

- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Data.JsonStore\RevolutionaryStuff.Data.JsonStore.csproj](../projects/RevolutionaryStuff.Data.JsonStore.md)

## Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["RevolutionaryStuff.Data.JsonStore.Tests.csproj"]
        MAIN["<b>📦&nbsp;RevolutionaryStuff.Data.JsonStore.Tests.csproj</b><br/><small>net10.0</small>"]
        click MAIN "../projects/RevolutionaryStuff.Data.JsonStore.Tests.md"
    end
    subgraph downstream["Dependencies (1)"]
        P13["<b>📦&nbsp;RevolutionaryStuff.Data.JsonStore.csproj</b><br/><small>net10.0</small>"]
        click P13 "../projects/RevolutionaryStuff.Data.JsonStore.md"
    end
    MAIN --> P13

```

## API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 244 |  |
| ***Total APIs Analyzed*** | ***244*** |  |

