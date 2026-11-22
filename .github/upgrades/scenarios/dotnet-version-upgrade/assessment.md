# Projects and dependencies analysis

This document provides a comprehensive overview of the projects and their dependencies in the context of upgrading to .NETCoreApp,Version=v11.0.

Detailed findings live alongside this file in `assessment/`. This page is the index: read it first, then open only the documents you need.

## Table of Contents

- [Executive Summary](#executive-summary)
  - [Highlevel Metrics](#highlevel-metrics)
  - [Projects Compatibility](#projects-compatibility)
  - [Package Compatibility](#package-compatibility)
  - [API Compatibility](#api-compatibility)
- [Top API Migration Challenges](#top-api-migration-challenges)
  - [Technologies and Features](#technologies-and-features)
  - [Most Frequent API Issues](#most-frequent-api-issues)
- [Detailed Reports](#detailed-reports)
  - [Projects Relationship Graph](assessment/project-graph.md)
  - [Aggregate NuGet packages details](assessment/nuget/aggregate-packages.md)
  - [Most Frequent API Issues (complete list)](assessment/api-issues/most-frequent-api-issues.md)
  - [Project Details](#project-details)

## Executive Summary

### Highlevel Metrics

| Metric | Count | Status |
| :--- | :---: | :--- |
| Total Projects | 25 | All require upgrade |
| Total NuGet Packages | 285 | 15 need upgrade |
| Total Code Files | 496 |  |
| Total Code Files with Incidents | 101 |  |
| Total Lines of Code | 42044 |  |
| Total Number of Issues | 377 |  |
| Proposed Target Framework | net10.0-android;net10.0-ios;net10.0-maccatalyst;net10.0-windows10.0.19041.0;net11.0-android;net11.0, net11.0 |  |
| Estimated LOC to modify | 334+ | at least 0.8% of codebase |

### Projects Compatibility

| Project | Target Framework | Difficulty | Package Issues | API Issues | Binding Issues | Est. LOC Impact | Description |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :--- |
| [src\RevolutionaryStuff.ApiCore\RevolutionaryStuff.ApiCore.csproj](assessment/projects/RevolutionaryStuff.ApiCore.md) | net10.0 | 🟢 Low | 4 | 15 | 0 | 15+ | ClassLibrary, Sdk Style = True |
| [src\RevolutionaryStuff.Applets.WebhookReceiverHost\RevolutionaryStuff.Applets.WebhookReceiverHost.csproj](assessment/projects/RevolutionaryStuff.Applets.WebhookReceiverHost.md) | net10.0 | 🟢 Low | 0 | 3 | 0 | 3+ | ClassLibrary, Sdk Style = True |
| [src\RevolutionaryStuff.Applets\RevolutionaryStuff.Applets.csproj](assessment/projects/RevolutionaryStuff.Applets.md) | net10.0 | 🟢 Low | 3 | 0 | 0 |  | ClassLibrary, Sdk Style = True |
| [src\RevolutionaryStuff.AspNetCore\RevolutionaryStuff.AspNetCore.csproj](assessment/projects/RevolutionaryStuff.AspNetCore.md) | net10.0 | 🟢 Low | 0 | 1 | 0 | 1+ | ClassLibrary, Sdk Style = True |
| [src\RevolutionaryStuff.Azure\RevolutionaryStuff.Azure.csproj](assessment/projects/RevolutionaryStuff.Azure.md) | net10.0 | 🟢 Low | 0 | 24 | 0 | 24+ | ClassLibrary, Sdk Style = True |
| [src\RevolutionaryStuff.BlazorWasm\RevolutionaryStuff.BlazorWasm.csproj](assessment/projects/RevolutionaryStuff.BlazorWasm.md) | net10.0 | 🟢 Low | 2 | 2 | 0 | 2+ | ClassLibrary, Sdk Style = True |
| [src\revolutionarystuff.com\revolutionarystuff.com.csproj](assessment/projects/revolutionarystuff.com.md) | net10.0 | 🟢 Low | 0 | 1 | 0 | 1+ | AspNetCore, Sdk Style = True |
| [src\RevolutionaryStuff.Core\RevolutionaryStuff.Core.csproj](assessment/projects/RevolutionaryStuff.Core.md) | net10.0 | 🟢 Low | 7 | 96 | 0 | 96+ | ClassLibrary, Sdk Style = True |
| [src\RevolutionaryStuff.Crm\RevolutionaryStuff.Crm.csproj](assessment/projects/RevolutionaryStuff.Crm.md) | net10.0 | 🟢 Low | 0 | 0 | 0 |  | ClassLibrary, Sdk Style = True |
| [src\RevolutionaryStuff.Dapr\RevolutionaryStuff.Dapr.csproj](assessment/projects/RevolutionaryStuff.Dapr.md) | net10.0 | 🟢 Low | 0 | 2 | 0 | 2+ | ClassLibrary, Sdk Style = True |
| [src\RevolutionaryStuff.Data.Cosmos\RevolutionaryStuff.Data.Cosmos.csproj](assessment/projects/RevolutionaryStuff.Data.Cosmos.md) | net10.0 | 🟢 Low | 0 | 4 | 0 | 4+ | ClassLibrary, Sdk Style = True |
| [src\RevolutionaryStuff.Data.JsonStore.Cosmos\RevolutionaryStuff.Data.JsonStore.Cosmos.csproj](assessment/projects/RevolutionaryStuff.Data.JsonStore.Cosmos.md) | net10.0 | 🟢 Low | 0 | 0 | 0 |  | ClassLibrary, Sdk Style = True |
| [src\RevolutionaryStuff.Data.JsonStore\RevolutionaryStuff.Data.JsonStore.csproj](assessment/projects/RevolutionaryStuff.Data.JsonStore.md) | net10.0 | 🟢 Low | 0 | 8 | 0 | 8+ | ClassLibrary, Sdk Style = True |
| [src\RevolutionaryStuff.Data.SqlAzure\RevolutionaryStuff.Data.SqlAzure.csproj](assessment/projects/RevolutionaryStuff.Data.SqlAzure.md) | net10.0 | 🟢 Low | 1 | 0 | 0 |  | ClassLibrary, Sdk Style = True |
| [src\RevolutionaryStuff.Maui\RevolutionaryStuff.Maui.csproj](assessment/projects/RevolutionaryStuff.Maui.md) | net10.0-android;net10.0-ios;net10.0-maccatalyst;net10.0-windows10.0.19041.0 | 🟢 Low | 1 | 53 | 0 | 53+ | ClassLibrary, Sdk Style = True |
| [src\RevolutionaryStuff.SqlServer\RevolutionaryStuff.Data.SqlServer.csproj](assessment/projects/RevolutionaryStuff.Data.SqlServer.md) | net10.0 | 🟢 Low | 0 | 0 | 0 |  | ClassLibrary, Sdk Style = True |
| [src\RevolutionaryStuff.Storage.Providers.Azure.Blob\RevolutionaryStuff.Storage.Providers.Azure.Blob.csproj](assessment/projects/RevolutionaryStuff.Storage.Providers.Azure.Blob.md) | net10.0 | 🟢 Low | 0 | 56 | 0 | 56+ | ClassLibrary, Sdk Style = True |
| [src\RevolutionaryStuff.Storage.Providers.LocalFileSystem\RevolutionaryStuff.Storage.Providers.LocalFileSystem.csproj](assessment/projects/RevolutionaryStuff.Storage.Providers.LocalFileSystem.md) | net10.0 | 🟢 Low | 0 | 2 | 0 | 2+ | ClassLibrary, Sdk Style = True |
| [src\RevolutionaryStuff.Storage\RevolutionaryStuff.Storage.csproj](assessment/projects/RevolutionaryStuff.Storage.md) | net10.0 | 🟢 Low | 0 | 2 | 0 | 2+ | ClassLibrary, Sdk Style = True |
| [tests\RevolutionaryStuff.Applets.Tests\RevolutionaryStuff.Applets.Tests.csproj](assessment/projects/RevolutionaryStuff.Applets.Tests.md) | net10.0 | 🟢 Low | 0 | 2 | 0 | 2+ | DotNetCoreApp, Sdk Style = True |
| [tests\RevolutionaryStuff.Azure.Tests\RevolutionaryStuff.Azure.Tests.csproj](assessment/projects/RevolutionaryStuff.Azure.Tests.md) | net10.0 | 🟢 Low | 0 | 18 | 0 | 18+ | DotNetCoreApp, Sdk Style = True |
| [tests\RevolutionaryStuff.BlazorWasm.Tests\RevolutionaryStuff.BlazorWasm.Tests.csproj](assessment/projects/RevolutionaryStuff.BlazorWasm.Tests.md) | net10.0 | 🟢 Low | 0 | 0 | 0 |  | DotNetCoreApp, Sdk Style = True |
| [tests\RevolutionaryStuff.Core.Tests\RevolutionaryStuff.Core.Tests.csproj](assessment/projects/RevolutionaryStuff.Core.Tests.md) | net10.0 | 🟢 Low | 0 | 44 | 0 | 44+ | DotNetCoreApp, Sdk Style = True |
| [tests\RevolutionaryStuff.Data.JsonStore.Cosmos.Tests\RevolutionaryStuff.Data.JsonStore.Cosmos.Tests.csproj](assessment/projects/RevolutionaryStuff.Data.JsonStore.Cosmos.Tests.md) | net10.0 | 🟢 Low | 0 | 1 | 0 | 1+ | DotNetCoreApp, Sdk Style = True |
| [tests\RevolutionaryStuff.Data.JsonStore.Tests\RevolutionaryStuff.Data.JsonStore.Tests.csproj](assessment/projects/RevolutionaryStuff.Data.JsonStore.Tests.md) | net10.0 | 🟢 Low | 0 | 0 | 0 |  | DotNetCoreApp, Sdk Style = True |

### Package Compatibility

| Status | Count | Percentage |
| :--- | :---: | :---: |
| ✅ Compatible | 270 | 94.7% |
| ⚠️ Incompatible | 0 | 0.0% |
| 🔄 Upgrade Recommended | 15 | 5.3% |
| ***Total NuGet Packages*** | ***285*** | ***100%*** |

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 136 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 198 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 43441 |  |
| ***Total APIs Analyzed*** | ***43775*** |  |

## Top API Migration Challenges

### Technologies and Features

| Technology | Issues | Percentage | Migration Path |
| :--- | :---: | :---: | :--- |

### Most Frequent API Issues

| API | Count | Percentage | Category |
| :--- | :---: | :---: | :--- |
| T:System.Uri | 69 | 20.7% | Behavioral Change |
| M:System.IO.MemoryStream.#ctor | 23 | 6.9% | Behavioral Change |
| T:Microsoft.Maui.Controls.BindingMode | 20 | 6.0% | Source Incompatible |
| T:System.BinaryData | 20 | 6.0% | Source Incompatible |
| M:System.IO.MemoryStream.#ctor(System.Byte[]) | 19 | 5.7% | Behavioral Change |
| M:System.Uri.#ctor(System.String) | 17 | 5.1% | Behavioral Change |
| T:System.Text.Json.JsonDocument | 14 | 4.2% | Behavioral Change |
| T:System.Net.Http.HttpContent | 11 | 3.3% | Behavioral Change |
| T:System.NotSupportedException | 11 | 3.3% | Behavioral Change |
| M:System.BinaryData.#ctor(System.String) | 10 | 3.0% | Source Incompatible |

The table above is the top 10. See [the complete list](assessment/api-issues/most-frequent-api-issues.md) for every affected API.

## Detailed Reports

- [Projects Relationship Graph](assessment/project-graph.md)
- [Aggregate NuGet packages details](assessment/nuget/aggregate-packages.md)
- [Most Frequent API Issues (complete list)](assessment/api-issues/most-frequent-api-issues.md)

### Project Details

- [src\RevolutionaryStuff.ApiCore\RevolutionaryStuff.ApiCore.csproj](assessment/projects/RevolutionaryStuff.ApiCore.md)
- [src\RevolutionaryStuff.Applets.WebhookReceiverHost\RevolutionaryStuff.Applets.WebhookReceiverHost.csproj](assessment/projects/RevolutionaryStuff.Applets.WebhookReceiverHost.md)
- [src\RevolutionaryStuff.Applets\RevolutionaryStuff.Applets.csproj](assessment/projects/RevolutionaryStuff.Applets.md)
- [src\RevolutionaryStuff.AspNetCore\RevolutionaryStuff.AspNetCore.csproj](assessment/projects/RevolutionaryStuff.AspNetCore.md)
- [src\RevolutionaryStuff.Azure\RevolutionaryStuff.Azure.csproj](assessment/projects/RevolutionaryStuff.Azure.md)
- [src\RevolutionaryStuff.BlazorWasm\RevolutionaryStuff.BlazorWasm.csproj](assessment/projects/RevolutionaryStuff.BlazorWasm.md)
- [src\revolutionarystuff.com\revolutionarystuff.com.csproj](assessment/projects/revolutionarystuff.com.md)
- [src\RevolutionaryStuff.Core\RevolutionaryStuff.Core.csproj](assessment/projects/RevolutionaryStuff.Core.md)
- [src\RevolutionaryStuff.Crm\RevolutionaryStuff.Crm.csproj](assessment/projects/RevolutionaryStuff.Crm.md)
- [src\RevolutionaryStuff.Dapr\RevolutionaryStuff.Dapr.csproj](assessment/projects/RevolutionaryStuff.Dapr.md)
- [src\RevolutionaryStuff.Data.Cosmos\RevolutionaryStuff.Data.Cosmos.csproj](assessment/projects/RevolutionaryStuff.Data.Cosmos.md)
- [src\RevolutionaryStuff.Data.JsonStore.Cosmos\RevolutionaryStuff.Data.JsonStore.Cosmos.csproj](assessment/projects/RevolutionaryStuff.Data.JsonStore.Cosmos.md)
- [src\RevolutionaryStuff.Data.JsonStore\RevolutionaryStuff.Data.JsonStore.csproj](assessment/projects/RevolutionaryStuff.Data.JsonStore.md)
- [src\RevolutionaryStuff.Data.SqlAzure\RevolutionaryStuff.Data.SqlAzure.csproj](assessment/projects/RevolutionaryStuff.Data.SqlAzure.md)
- [src\RevolutionaryStuff.Maui\RevolutionaryStuff.Maui.csproj](assessment/projects/RevolutionaryStuff.Maui.md)
- [src\RevolutionaryStuff.SqlServer\RevolutionaryStuff.Data.SqlServer.csproj](assessment/projects/RevolutionaryStuff.Data.SqlServer.md)
- [src\RevolutionaryStuff.Storage.Providers.Azure.Blob\RevolutionaryStuff.Storage.Providers.Azure.Blob.csproj](assessment/projects/RevolutionaryStuff.Storage.Providers.Azure.Blob.md)
- [src\RevolutionaryStuff.Storage.Providers.LocalFileSystem\RevolutionaryStuff.Storage.Providers.LocalFileSystem.csproj](assessment/projects/RevolutionaryStuff.Storage.Providers.LocalFileSystem.md)
- [src\RevolutionaryStuff.Storage\RevolutionaryStuff.Storage.csproj](assessment/projects/RevolutionaryStuff.Storage.md)
- [tests\RevolutionaryStuff.Applets.Tests\RevolutionaryStuff.Applets.Tests.csproj](assessment/projects/RevolutionaryStuff.Applets.Tests.md)
- [tests\RevolutionaryStuff.Azure.Tests\RevolutionaryStuff.Azure.Tests.csproj](assessment/projects/RevolutionaryStuff.Azure.Tests.md)
- [tests\RevolutionaryStuff.BlazorWasm.Tests\RevolutionaryStuff.BlazorWasm.Tests.csproj](assessment/projects/RevolutionaryStuff.BlazorWasm.Tests.md)
- [tests\RevolutionaryStuff.Core.Tests\RevolutionaryStuff.Core.Tests.csproj](assessment/projects/RevolutionaryStuff.Core.Tests.md)
- [tests\RevolutionaryStuff.Data.JsonStore.Cosmos.Tests\RevolutionaryStuff.Data.JsonStore.Cosmos.Tests.csproj](assessment/projects/RevolutionaryStuff.Data.JsonStore.Cosmos.Tests.md)
- [tests\RevolutionaryStuff.Data.JsonStore.Tests\RevolutionaryStuff.Data.JsonStore.Tests.csproj](assessment/projects/RevolutionaryStuff.Data.JsonStore.Tests.md)


