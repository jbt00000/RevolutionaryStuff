# src\RevolutionaryStuff.AspNetCore\RevolutionaryStuff.AspNetCore.csproj

[← Back to the assessment index](../../assessment.md)

## Project Info

- **Current Target Framework:** net10.0
- **Proposed Target Framework:** net11.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 1
- **Dependants**: 2
- **Number of Files**: 19
- **Number of Files with Incidents**: 2
- **Lines of Code**: 930
- **Estimated LOC to modify**: 1+ (at least 0.1% of the project)

## Related Projects

**Depends on (1)** — projects this one references:

- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Core\RevolutionaryStuff.Core.csproj](../projects/RevolutionaryStuff.Core.md)

**Depended on by (2)** — projects that reference this one:

- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Applets.WebhookReceiverHost\RevolutionaryStuff.Applets.WebhookReceiverHost.csproj](../projects/RevolutionaryStuff.Applets.WebhookReceiverHost.md)
- [C:\src\jbt00000\RevolutionaryStuff\src\revolutionarystuff.com\revolutionarystuff.com.csproj](../projects/revolutionarystuff.com.md)

## Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (2)"]
        P2["<b>📦&nbsp;RevolutionaryStuff.Applets.WebhookReceiverHost.csproj</b><br/><small>net10.0</small>"]
        P7["<b>📦&nbsp;revolutionarystuff.com.csproj</b><br/><small>net10.0</small>"]
        click P2 "../projects/RevolutionaryStuff.Applets.WebhookReceiverHost.md"
        click P7 "../projects/revolutionarystuff.com.md"
    end
    subgraph current["RevolutionaryStuff.AspNetCore.csproj"]
        MAIN["<b>📦&nbsp;RevolutionaryStuff.AspNetCore.csproj</b><br/><small>net10.0</small>"]
        click MAIN "../projects/RevolutionaryStuff.AspNetCore.md"
    end
    subgraph downstream["Dependencies (1)"]
        P8["<b>📦&nbsp;RevolutionaryStuff.Core.csproj</b><br/><small>net10.0</small>"]
        click P8 "../projects/RevolutionaryStuff.Core.md"
    end
    P2 --> MAIN
    P7 --> MAIN
    MAIN --> P8

```

## API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 1 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 1024 |  |
| ***Total APIs Analyzed*** | ***1025*** |  |

