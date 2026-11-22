# src\RevolutionaryStuff.Applets.WebhookReceiverHost\RevolutionaryStuff.Applets.WebhookReceiverHost.csproj

[← Back to the assessment index](../../assessment.md)

## Project Info

- **Current Target Framework:** net10.0
- **Proposed Target Framework:** net11.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 4
- **Dependants**: 0
- **Number of Files**: 12
- **Number of Files with Incidents**: 3
- **Lines of Code**: 396
- **Estimated LOC to modify**: 3+ (at least 0.8% of the project)

## Related Projects

**Depends on (4)** — projects this one references:

- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.ApiCore\RevolutionaryStuff.ApiCore.csproj](../projects/RevolutionaryStuff.ApiCore.md)
- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Applets\RevolutionaryStuff.Applets.csproj](../projects/RevolutionaryStuff.Applets.md)
- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.AspNetCore\RevolutionaryStuff.AspNetCore.csproj](../projects/RevolutionaryStuff.AspNetCore.md)
- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Azure\RevolutionaryStuff.Azure.csproj](../projects/RevolutionaryStuff.Azure.md)

## Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["RevolutionaryStuff.Applets.WebhookReceiverHost.csproj"]
        MAIN["<b>📦&nbsp;RevolutionaryStuff.Applets.WebhookReceiverHost.csproj</b><br/><small>net10.0</small>"]
        click MAIN "../projects/RevolutionaryStuff.Applets.WebhookReceiverHost.md"
    end
    subgraph downstream["Dependencies (4)"]
        P3["<b>📦&nbsp;RevolutionaryStuff.Applets.csproj</b><br/><small>net10.0</small>"]
        P5["<b>📦&nbsp;RevolutionaryStuff.Azure.csproj</b><br/><small>net10.0</small>"]
        P4["<b>📦&nbsp;RevolutionaryStuff.AspNetCore.csproj</b><br/><small>net10.0</small>"]
        P1["<b>📦&nbsp;RevolutionaryStuff.ApiCore.csproj</b><br/><small>net10.0</small>"]
        click P3 "../projects/RevolutionaryStuff.Applets.md"
        click P5 "../projects/RevolutionaryStuff.Azure.md"
        click P4 "../projects/RevolutionaryStuff.AspNetCore.md"
        click P1 "../projects/RevolutionaryStuff.ApiCore.md"
    end
    MAIN --> P3
    MAIN --> P5
    MAIN --> P4
    MAIN --> P1

```

## API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 2 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 1 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 1134 |  |
| ***Total APIs Analyzed*** | ***1137*** |  |

