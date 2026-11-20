# tests\RevolutionaryStuff.BlazorWasm.Tests\RevolutionaryStuff.BlazorWasm.Tests.csproj

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
- **Lines of Code**: 56
- **Estimated LOC to modify**: 0+ (at least 0.0% of the project)

## Related Projects

**Depends on (1)** — projects this one references:

- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.BlazorWasm\RevolutionaryStuff.BlazorWasm.csproj](../projects/RevolutionaryStuff.BlazorWasm.md)

## Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["RevolutionaryStuff.BlazorWasm.Tests.csproj"]
        MAIN["<b>📦&nbsp;RevolutionaryStuff.BlazorWasm.Tests.csproj</b><br/><small>net10.0</small>"]
        click MAIN "../projects/RevolutionaryStuff.BlazorWasm.Tests.md"
    end
    subgraph downstream["Dependencies (1)"]
        P6["<b>📦&nbsp;RevolutionaryStuff.BlazorWasm.csproj</b><br/><small>net10.0</small>"]
        click P6 "../projects/RevolutionaryStuff.BlazorWasm.md"
    end
    MAIN --> P6

```

## API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 70 |  |
| ***Total APIs Analyzed*** | ***70*** |  |

