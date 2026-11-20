# src\RevolutionaryStuff.Dapr\RevolutionaryStuff.Dapr.csproj

[← Back to the assessment index](../../assessment.md)

## Project Info

- **Current Target Framework:** net10.0
- **Proposed Target Framework:** net11.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 1
- **Dependants**: 0
- **Number of Files**: 20
- **Number of Files with Incidents**: 2
- **Lines of Code**: 598
- **Estimated LOC to modify**: 2+ (at least 0.3% of the project)

## Related Projects

**Depends on (1)** — projects this one references:

- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.ApiCore\RevolutionaryStuff.ApiCore.csproj](../projects/RevolutionaryStuff.ApiCore.md)

## Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["RevolutionaryStuff.Dapr.csproj"]
        MAIN["<b>📦&nbsp;RevolutionaryStuff.Dapr.csproj</b><br/><small>net10.0</small>"]
        click MAIN "../projects/RevolutionaryStuff.Dapr.md"
    end
    subgraph downstream["Dependencies (1)"]
        P1["<b>📦&nbsp;RevolutionaryStuff.ApiCore.csproj</b><br/><small>net10.0</small>"]
        click P1 "../projects/RevolutionaryStuff.ApiCore.md"
    end
    MAIN --> P1

```

## API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 2 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 1255 |  |
| ***Total APIs Analyzed*** | ***1257*** |  |

