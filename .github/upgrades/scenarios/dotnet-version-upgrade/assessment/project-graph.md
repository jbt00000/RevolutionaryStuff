# Projects Relationship Graph

[← Back to the assessment index](../assessment.md)

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart LR
    P1["<b>📦&nbsp;RevolutionaryStuff.ApiCore.csproj</b><br/><small>net10.0</small>"]
    P2["<b>📦&nbsp;RevolutionaryStuff.Applets.WebhookReceiverHost.csproj</b><br/><small>net10.0</small>"]
    P3["<b>📦&nbsp;RevolutionaryStuff.Applets.csproj</b><br/><small>net10.0</small>"]
    P4["<b>📦&nbsp;RevolutionaryStuff.AspNetCore.csproj</b><br/><small>net10.0</small>"]
    P5["<b>📦&nbsp;RevolutionaryStuff.Azure.csproj</b><br/><small>net10.0</small>"]
    P6["<b>📦&nbsp;RevolutionaryStuff.BlazorWasm.csproj</b><br/><small>net10.0</small>"]
    P7["<b>📦&nbsp;revolutionarystuff.com.csproj</b><br/><small>net10.0</small>"]
    P8["<b>📦&nbsp;RevolutionaryStuff.Core.csproj</b><br/><small>net10.0</small>"]
    P9["<b>📦&nbsp;RevolutionaryStuff.Crm.csproj</b><br/><small>net10.0</small>"]
    P10["<b>📦&nbsp;RevolutionaryStuff.Dapr.csproj</b><br/><small>net10.0</small>"]
    P11["<b>📦&nbsp;RevolutionaryStuff.Data.Cosmos.csproj</b><br/><small>net10.0</small>"]
    P12["<b>📦&nbsp;RevolutionaryStuff.Data.JsonStore.Cosmos.csproj</b><br/><small>net10.0</small>"]
    P13["<b>📦&nbsp;RevolutionaryStuff.Data.JsonStore.csproj</b><br/><small>net10.0</small>"]
    P14["<b>📦&nbsp;RevolutionaryStuff.Data.SqlAzure.csproj</b><br/><small>net10.0</small>"]
    P15["<b>📦&nbsp;RevolutionaryStuff.Maui.csproj</b><br/><small>net10.0-android;net10.0-ios;net10.0-maccatalyst;net10.0-windows10.0.19041.0</small>"]
    P16["<b>📦&nbsp;RevolutionaryStuff.Data.SqlServer.csproj</b><br/><small>net10.0</small>"]
    P17["<b>📦&nbsp;RevolutionaryStuff.Storage.Providers.Azure.Blob.csproj</b><br/><small>net10.0</small>"]
    P18["<b>📦&nbsp;RevolutionaryStuff.Storage.Providers.LocalFileSystem.csproj</b><br/><small>net10.0</small>"]
    P19["<b>📦&nbsp;RevolutionaryStuff.Storage.csproj</b><br/><small>net10.0</small>"]
    P20["<b>📦&nbsp;RevolutionaryStuff.Core.Tests.csproj</b><br/><small>net10.0</small>"]
    P21["<b>📦&nbsp;RevolutionaryStuff.Applets.Tests.csproj</b><br/><small>net10.0</small>"]
    P22["<b>📦&nbsp;RevolutionaryStuff.Azure.Tests.csproj</b><br/><small>net10.0</small>"]
    P23["<b>📦&nbsp;RevolutionaryStuff.BlazorWasm.Tests.csproj</b><br/><small>net10.0</small>"]
    P24["<b>📦&nbsp;RevolutionaryStuff.Data.JsonStore.Cosmos.Tests.csproj</b><br/><small>net10.0</small>"]
    P25["<b>📦&nbsp;RevolutionaryStuff.Data.JsonStore.Tests.csproj</b><br/><small>net10.0</small>"]
    P1 --> P8
    P2 --> P3
    P2 --> P5
    P2 --> P4
    P2 --> P1
    P3 --> P8
    P3 --> P19
    P4 --> P8
    P5 --> P8
    P6 --> P8
    P7 --> P8
    P7 --> P4
    P9 --> P8
    P10 --> P1
    P11 --> P5
    P11 --> P8
    P12 --> P8
    P12 --> P13
    P12 --> P11
    P13 --> P8
    P14 --> P5
    P14 --> P8
    P15 --> P8
    P16 --> P8
    P17 --> P5
    P17 --> P19
    P18 --> P19
    P19 --> P8
    P20 --> P8
    P21 --> P3
    P22 --> P5
    P23 --> P6
    P24 --> P12
    P25 --> P13
    click P1 "projects/RevolutionaryStuff.ApiCore.md"
    click P2 "projects/RevolutionaryStuff.Applets.WebhookReceiverHost.md"
    click P3 "projects/RevolutionaryStuff.Applets.md"
    click P4 "projects/RevolutionaryStuff.AspNetCore.md"
    click P5 "projects/RevolutionaryStuff.Azure.md"
    click P6 "projects/RevolutionaryStuff.BlazorWasm.md"
    click P7 "projects/revolutionarystuff.com.md"
    click P8 "projects/RevolutionaryStuff.Core.md"
    click P9 "projects/RevolutionaryStuff.Crm.md"
    click P10 "projects/RevolutionaryStuff.Dapr.md"
    click P11 "projects/RevolutionaryStuff.Data.Cosmos.md"
    click P12 "projects/RevolutionaryStuff.Data.JsonStore.Cosmos.md"
    click P13 "projects/RevolutionaryStuff.Data.JsonStore.md"
    click P14 "projects/RevolutionaryStuff.Data.SqlAzure.md"
    click P15 "projects/RevolutionaryStuff.Maui.md"
    click P16 "projects/RevolutionaryStuff.Data.SqlServer.md"
    click P17 "projects/RevolutionaryStuff.Storage.Providers.Azure.Blob.md"
    click P18 "projects/RevolutionaryStuff.Storage.Providers.LocalFileSystem.md"
    click P19 "projects/RevolutionaryStuff.Storage.md"
    click P20 "projects/RevolutionaryStuff.Core.Tests.md"
    click P21 "projects/RevolutionaryStuff.Applets.Tests.md"
    click P22 "projects/RevolutionaryStuff.Azure.Tests.md"
    click P23 "projects/RevolutionaryStuff.BlazorWasm.Tests.md"
    click P24 "projects/RevolutionaryStuff.Data.JsonStore.Cosmos.Tests.md"
    click P25 "projects/RevolutionaryStuff.Data.JsonStore.Tests.md"

```

