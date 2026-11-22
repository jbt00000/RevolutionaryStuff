# src\RevolutionaryStuff.ApiCore\RevolutionaryStuff.ApiCore.csproj

[← Back to the assessment index](../../assessment.md)

## Project Info

- **Current Target Framework:** net10.0
- **Proposed Target Framework:** net11.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 1
- **Dependants**: 2
- **Number of Files**: 33
- **Number of Files with Incidents**: 6
- **Lines of Code**: 1496
- **Estimated LOC to modify**: 15+ (at least 1.0% of the project)

## Related Projects

**Depends on (1)** — projects this one references:

- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Core\RevolutionaryStuff.Core.csproj](../projects/RevolutionaryStuff.Core.md)

**Depended on by (2)** — projects that reference this one:

- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Applets.WebhookReceiverHost\RevolutionaryStuff.Applets.WebhookReceiverHost.csproj](../projects/RevolutionaryStuff.Applets.WebhookReceiverHost.md)
- [C:\src\jbt00000\RevolutionaryStuff\src\RevolutionaryStuff.Dapr\RevolutionaryStuff.Dapr.csproj](../projects/RevolutionaryStuff.Dapr.md)

## Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (2)"]
        P2["<b>📦&nbsp;RevolutionaryStuff.Applets.WebhookReceiverHost.csproj</b><br/><small>net10.0</small>"]
        P10["<b>📦&nbsp;RevolutionaryStuff.Dapr.csproj</b><br/><small>net10.0</small>"]
        click P2 "../projects/RevolutionaryStuff.Applets.WebhookReceiverHost.md"
        click P10 "../projects/RevolutionaryStuff.Dapr.md"
    end
    subgraph current["RevolutionaryStuff.ApiCore.csproj"]
        MAIN["<b>📦&nbsp;RevolutionaryStuff.ApiCore.csproj</b><br/><small>net10.0</small>"]
        click MAIN "../projects/RevolutionaryStuff.ApiCore.md"
    end
    subgraph downstream["Dependencies (1)"]
        P8["<b>📦&nbsp;RevolutionaryStuff.Core.csproj</b><br/><small>net10.0</small>"]
        click P8 "../projects/RevolutionaryStuff.Core.md"
    end
    P2 --> MAIN
    P10 --> MAIN
    MAIN --> P8

```

## API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 10 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 5 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 2143 |  |
| ***Total APIs Analyzed*** | ***2158*** |  |

## NuGet Package Issues

| Package | Current Version | Suggested Version | Severity | Issue |
| :--- | :---: | :---: | :---: | :--- |
| Microsoft.AspNetCore.Components.WebAssembly | 10.0.15 | 11.0.0 | 🟡 Potential | NuGet package upgrade is recommended |
| Microsoft.AspNetCore.OpenApi | 10.0.15 | 11.0.0 | 🟡 Potential | NuGet package upgrade is recommended |
| Microsoft.Extensions.Http.Resilience | 10.11.0 | 11.0.0 | 🟡 Potential | NuGet package upgrade is recommended |
| Microsoft.Extensions.ServiceDiscovery | 10.11.0 | 11.0.0 | 🟡 Potential | NuGet package upgrade is recommended |

Every project affected by these packages, and the versions the repository settles on: [aggregate NuGet packages](../nuget/aggregate-packages.md).

