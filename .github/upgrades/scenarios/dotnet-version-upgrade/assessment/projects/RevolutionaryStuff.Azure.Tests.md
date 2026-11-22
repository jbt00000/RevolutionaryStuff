# tests\RevolutionaryStuff.Azure.Tests\RevolutionaryStuff.Azure.Tests.csproj

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
- **Lines of Code**: 833
- **Estimated LOC to modify**: 18+ (at least 2.2% of the project)

## Related Projects

**Depends on (1)** — projects this one references:

- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Azure\RevolutionaryStuff.Azure.csproj](../projects/RevolutionaryStuff.Azure.md)

## Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["RevolutionaryStuff.Azure.Tests.csproj"]
        MAIN["<b>📦&nbsp;RevolutionaryStuff.Azure.Tests.csproj</b><br/><small>net10.0</small>"]
        click MAIN "../projects/RevolutionaryStuff.Azure.Tests.md"
    end
    subgraph downstream["Dependencies (1)"]
        P5["<b>📦&nbsp;RevolutionaryStuff.Azure.csproj</b><br/><small>net10.0</small>"]
        click P5 "../projects/RevolutionaryStuff.Azure.md"
    end
    MAIN --> P5

```

## API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 18 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 907 |  |
| ***Total APIs Analyzed*** | ***925*** |  |

