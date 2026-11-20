# src\RevolutionaryStuff.Data.JsonStore.Cosmos\RevolutionaryStuff.Data.JsonStore.Cosmos.csproj

[← Back to the assessment index](../../assessment.md)

## Project Info

- **Current Target Framework:** net10.0
- **Proposed Target Framework:** net11.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 3
- **Dependants**: 1
- **Number of Files**: 18
- **Number of Files with Incidents**: 1
- **Lines of Code**: 937
- **Estimated LOC to modify**: 0+ (at least 0.0% of the project)

## Related Projects

**Depends on (3)** — projects this one references:

- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Core\RevolutionaryStuff.Core.csproj](../projects/RevolutionaryStuff.Core.md)
- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Data.Cosmos\RevolutionaryStuff.Data.Cosmos.csproj](../projects/RevolutionaryStuff.Data.Cosmos.md)
- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Data.JsonStore\RevolutionaryStuff.Data.JsonStore.csproj](../projects/RevolutionaryStuff.Data.JsonStore.md)

**Depended on by (1)** — projects that reference this one:

- [C:\src\jbt00000\RevolutionaryStuff\tests\RevolutionaryStuff.Data.JsonStore.Cosmos.Tests\RevolutionaryStuff.Data.JsonStore.Cosmos.Tests.csproj](../projects/RevolutionaryStuff.Data.JsonStore.Cosmos.Tests.md)

## Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (1)"]
        P24["<b>📦&nbsp;RevolutionaryStuff.Data.JsonStore.Cosmos.Tests.csproj</b><br/><small>net10.0</small>"]
        click P24 "../projects/RevolutionaryStuff.Data.JsonStore.Cosmos.Tests.md"
    end
    subgraph current["RevolutionaryStuff.Data.JsonStore.Cosmos.csproj"]
        MAIN["<b>📦&nbsp;RevolutionaryStuff.Data.JsonStore.Cosmos.csproj</b><br/><small>net10.0</small>"]
        click MAIN "../projects/RevolutionaryStuff.Data.JsonStore.Cosmos.md"
    end
    subgraph downstream["Dependencies (3)"]
        P8["<b>📦&nbsp;RevolutionaryStuff.Core.csproj</b><br/><small>net10.0</small>"]
        P13["<b>📦&nbsp;RevolutionaryStuff.Data.JsonStore.csproj</b><br/><small>net10.0</small>"]
        P11["<b>📦&nbsp;RevolutionaryStuff.Data.Cosmos.csproj</b><br/><small>net10.0</small>"]
        click P8 "../projects/RevolutionaryStuff.Core.md"
        click P13 "../projects/RevolutionaryStuff.Data.JsonStore.md"
        click P11 "../projects/RevolutionaryStuff.Data.Cosmos.md"
    end
    P24 --> MAIN
    MAIN --> P8
    MAIN --> P13
    MAIN --> P11

```

## API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 891 |  |
| ***Total APIs Analyzed*** | ***891*** |  |

