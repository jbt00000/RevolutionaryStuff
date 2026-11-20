# src\RevolutionaryStuff.Storage.Providers.LocalFileSystem\RevolutionaryStuff.Storage.Providers.LocalFileSystem.csproj

[← Back to the assessment index](../../assessment.md)

## Project Info

- **Current Target Framework:** net10.0
- **Proposed Target Framework:** net11.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 1
- **Dependants**: 0
- **Number of Files**: 12
- **Number of Files with Incidents**: 2
- **Lines of Code**: 396
- **Estimated LOC to modify**: 2+ (at least 0.5% of the project)

## Related Projects

**Depends on (1)** — projects this one references:

- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Storage\RevolutionaryStuff.Storage.csproj](../projects/RevolutionaryStuff.Storage.md)

## Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["RevolutionaryStuff.Storage.Providers.LocalFileSystem.csproj"]
        MAIN["<b>📦&nbsp;RevolutionaryStuff.Storage.Providers.LocalFileSystem.csproj</b><br/><small>net10.0</small>"]
        click MAIN "../projects/RevolutionaryStuff.Storage.Providers.LocalFileSystem.md"
    end
    subgraph downstream["Dependencies (1)"]
        P19["<b>📦&nbsp;RevolutionaryStuff.Storage.csproj</b><br/><small>net10.0</small>"]
        click P19 "../projects/RevolutionaryStuff.Storage.md"
    end
    MAIN --> P19

```

## API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 2 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 498 |  |
| ***Total APIs Analyzed*** | ***500*** |  |

