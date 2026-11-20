# src\RevolutionaryStuff.Azure\RevolutionaryStuff.Azure.csproj

[← Back to the assessment index](../../assessment.md)

## Project Info

- **Current Target Framework:** net10.0
- **Proposed Target Framework:** net11.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 1
- **Dependants**: 5
- **Number of Files**: 27
- **Number of Files with Incidents**: 7
- **Lines of Code**: 1071
- **Estimated LOC to modify**: 24+ (at least 2.2% of the project)

## Related Projects

**Depends on (1)** — projects this one references:

- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Core\RevolutionaryStuff.Core.csproj](../projects/RevolutionaryStuff.Core.md)

**Depended on by (5)** — projects that reference this one:

- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Applets.WebhookReceiverHost\RevolutionaryStuff.Applets.WebhookReceiverHost.csproj](../projects/RevolutionaryStuff.Applets.WebhookReceiverHost.md)
- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Data.Cosmos\RevolutionaryStuff.Data.Cosmos.csproj](../projects/RevolutionaryStuff.Data.Cosmos.md)
- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Data.SqlAzure\RevolutionaryStuff.Data.SqlAzure.csproj](../projects/RevolutionaryStuff.Data.SqlAzure.md)
- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Storage.Providers.Azure.Blob\RevolutionaryStuff.Storage.Providers.Azure.Blob.csproj](../projects/RevolutionaryStuff.Storage.Providers.Azure.Blob.md)
- [C:\src\jbt00000\RevolutionaryStuff\tests\RevolutionaryStuff.Azure.Tests\RevolutionaryStuff.Azure.Tests.csproj](../projects/RevolutionaryStuff.Azure.Tests.md)

## Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (5)"]
        P2["<b>📦&nbsp;RevolutionaryStuff.Applets.WebhookReceiverHost.csproj</b><br/><small>net10.0</small>"]
        P11["<b>📦&nbsp;RevolutionaryStuff.Data.Cosmos.csproj</b><br/><small>net10.0</small>"]
        P14["<b>📦&nbsp;RevolutionaryStuff.Data.SqlAzure.csproj</b><br/><small>net10.0</small>"]
        P17["<b>📦&nbsp;RevolutionaryStuff.Storage.Providers.Azure.Blob.csproj</b><br/><small>net10.0</small>"]
        P22["<b>📦&nbsp;RevolutionaryStuff.Azure.Tests.csproj</b><br/><small>net10.0</small>"]
        click P2 "../projects/RevolutionaryStuff.Applets.WebhookReceiverHost.md"
        click P11 "../projects/RevolutionaryStuff.Data.Cosmos.md"
        click P14 "../projects/RevolutionaryStuff.Data.SqlAzure.md"
        click P17 "../projects/RevolutionaryStuff.Storage.Providers.Azure.Blob.md"
        click P22 "../projects/RevolutionaryStuff.Azure.Tests.md"
    end
    subgraph current["RevolutionaryStuff.Azure.csproj"]
        MAIN["<b>📦&nbsp;RevolutionaryStuff.Azure.csproj</b><br/><small>net10.0</small>"]
        click MAIN "../projects/RevolutionaryStuff.Azure.md"
    end
    subgraph downstream["Dependencies (1)"]
        P8["<b>📦&nbsp;RevolutionaryStuff.Core.csproj</b><br/><small>net10.0</small>"]
        click P8 "../projects/RevolutionaryStuff.Core.md"
    end
    P2 --> MAIN
    P11 --> MAIN
    P14 --> MAIN
    P17 --> MAIN
    P22 --> MAIN
    MAIN --> P8

```

## API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 17 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 7 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 1045 |  |
| ***Total APIs Analyzed*** | ***1069*** |  |

