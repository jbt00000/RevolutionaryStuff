# src\revolutionarystuff.com\revolutionarystuff.com.csproj

[← Back to the assessment index](../../assessment.md)

## Project Info

- **Current Target Framework:** net10.0
- **Proposed Target Framework:** net11.0
- **SDK-style**: True
- **Project Kind:** AspNetCore
- **Dependencies**: 2
- **Dependants**: 0
- **Number of Files**: 23
- **Number of Files with Incidents**: 2
- **Lines of Code**: 604
- **Estimated LOC to modify**: 1+ (at least 0.2% of the project)

## Related Projects

**Depends on (2)** — projects this one references:

- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.AspNetCore\RevolutionaryStuff.AspNetCore.csproj](../projects/RevolutionaryStuff.AspNetCore.md)
- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Core\RevolutionaryStuff.Core.csproj](../projects/RevolutionaryStuff.Core.md)

## Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["revolutionarystuff.com.csproj"]
        MAIN["<b>📦&nbsp;revolutionarystuff.com.csproj</b><br/><small>net10.0</small>"]
        click MAIN "../projects/revolutionarystuff.com.md"
    end
    subgraph downstream["Dependencies (2)"]
        P8["<b>📦&nbsp;RevolutionaryStuff.Core.csproj</b><br/><small>net10.0</small>"]
        P4["<b>📦&nbsp;RevolutionaryStuff.AspNetCore.csproj</b><br/><small>net10.0</small>"]
        click P8 "../projects/RevolutionaryStuff.Core.md"
        click P4 "../projects/RevolutionaryStuff.AspNetCore.md"
    end
    MAIN --> P8
    MAIN --> P4

```

## API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 1 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 1136 |  |
| ***Total APIs Analyzed*** | ***1137*** |  |

