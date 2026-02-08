# .NET 10.0 Upgrade Plan

## Table of Contents
- [Executive Summary](#executive-summary)
- [Migration Strategy](#migration-strategy)
- [Detailed Dependency Analysis](#detailed-dependency-analysis)
- [Project-by-Project Plans](#project-by-project-plans)
- [Package Update Reference](#package-update-reference)
- [Breaking Changes Catalog](#breaking-changes-catalog)
- [Risk Management](#risk-management)
- [Testing & Validation Strategy](#testing--validation-strategy)
- [Complexity & Effort Assessment](#complexity--effort-assessment)
- [Source Control Strategy](#source-control-strategy)
- [Success Criteria](#success-criteria)

---

## Executive Summary

### Scenario Overview
Upgrade the RevolutionaryStuff solution from **.NET 9.0** to **.NET 10.0 (LTS)**.

### Scope
- **Total Projects**: 18 (17 class libraries + 1 ASP.NET Core Razor Pages application)
- **Current State**: All projects targeting `net9.0`
- **Target State**: All projects targeting `net10.0`
- **Total Issues**: 254 (18 mandatory, 236 potential)
- **Affected Files**: 63 files across 8 projects with code changes required

### Selected Strategy
**All-At-Once Strategy** - All projects upgraded simultaneously in a single operation.

**Rationale**:
- 18 projects (medium solution, under 30 threshold)
- All currently on .NET 9.0 (homogeneous codebase)
- Clear 4-level dependency structure with no circular dependencies
- All 14 packages requiring updates have known target versions available
- No security vulnerabilities present
- All issues are framework/API compatibility changes (no blockers)

### Complexity Assessment
**Classification: Medium**

| Metric | Value | Risk Factor |
|--------|-------|-------------|
| Project Count | 18 | Low |
| Dependency Depth | 4 levels | Low |
| Circular Dependencies | None | Low |
| Security Vulnerabilities | 0 | None |
| Source Incompatibilities | 71 occurrences | Medium |
| Behavioral Changes | 150 occurrences | Low |
| Package Updates | 14 packages | Low |

### Critical Issues Summary

**Source Incompatibilities (Api.0002)** - Code changes required:
| Area | Occurrences | Primary Files |
|------|-------------|---------------|
| `System.IO.Hashing` API changes | 32 | `Hash.cs` in RevolutionaryStuff.Core |
| `System.BinaryData` API changes | 16 | `InboundMessage.cs`, `ServiceBusMessageSender.cs` in RevolutionaryStuff.Azure |
| Other API changes | 23 | Various files across 6 additional projects |

**Behavioral Changes (Api.0003)** - Runtime behavior awareness:
- 150 occurrences across multiple projects
- Primarily `System.Uri` constructor behavior changes
- No code changes required unless behavior differences cause issues

### Iteration Strategy
Using **Phase-based batching** approach for plan generation:
- Phase 1: Discovery & Classification (complete)
- Phase 2: Foundation sections (dependency, strategy, risk)
- Phase 3: All project details (single batch - all-at-once strategy)

---

## Migration Strategy

### Selected Approach: All-At-Once

**All projects will be upgraded simultaneously in a single atomic operation.**

### Rationale for All-At-Once Strategy

| Criterion | Assessment | Supports All-At-Once |
|-----------|------------|---------------------|
| Project Count | 18 projects | ✅ Under 30 threshold |
| Framework Homogeneity | All on net9.0 | ✅ Uniform baseline |
| Dependency Structure | 4 levels, no cycles | ✅ Clear hierarchy |
| Package Compatibility | All packages have net10.0 versions | ✅ No blockers |
| Security Vulnerabilities | None | ✅ No urgent fixes needed |
| Breaking Changes | Source incompatibilities manageable | ✅ Concentrated in 2-3 files |

### Execution Approach

The atomic upgrade will be executed as follows:

1. **Update all project files simultaneously**
   - Change `TargetFramework` from `net9.0` to `net10.0` in all 18 project files

2. **Update all package references simultaneously**
   - Update 14 packages across 4 projects to their net10.0-compatible versions

3. **Restore and build solution**
   - Run `dotnet restore` to resolve new package versions
   - Build entire solution to identify all compilation errors

4. **Fix all compilation errors in single pass**
   - Address `System.IO.Hashing` API changes in `Hash.cs`
   - Address `System.BinaryData` API changes in Azure messaging files
   - Fix any other source incompatibilities discovered during build

5. **Verify solution builds with 0 errors**

6. **Execute test projects**
   - Run `RevolutionaryStuff.Core.Tests`
   - Verify all tests pass

### Advantages of This Approach

- **Speed**: Single coordinated update rather than iterative phases
- **Simplicity**: No multi-targeting complexity or intermediate states
- **Clean dependencies**: All projects move to net10.0 together
- **Unified testing**: Test the complete solution state, not intermediate states

### Potential Challenges and Mitigations

| Challenge | Mitigation |
|-----------|------------|
| Multiple compilation errors at once | Breaking changes are well-documented and concentrated in specific files |
| Larger testing surface | Strong test project coverage validates Core functionality |
| All changes must succeed together | Can revert entire branch if critical issues discovered |

---

## Detailed Dependency Analysis

### Dependency Graph Overview

```
Level 0 (Foundation)
└── RevolutionaryStuff.Core (92 issues)
    │
Level 1 (Core Dependents)
├── RevolutionaryStuff.ApiCore (10 issues)
├── RevolutionaryStuff.AspNetCore (2 issues)
├── RevolutionaryStuff.Azure (19 issues)
├── RevolutionaryStuff.Crm (1 issue)
├── RevolutionaryStuff.Data.JsonStore (9 issues)
├── RevolutionaryStuff.Data.SqlServer (1 issue)
├── RevolutionaryStuff.Storage (2 issues)
└── RevolutionaryStuff.Core.Tests (20 issues) [Test Project]
    │
Level 2 (Extended Dependencies)
├── RevolutionaryStuff.Crm.MondayCom (depends on Crm)
├── RevolutionaryStuff.Crm.OpenPhone (depends on Crm)
├── RevolutionaryStuff.Dapr (depends on ApiCore)
├── RevolutionaryStuff.Data.Cosmos (depends on Azure, Core)
├── RevolutionaryStuff.Data.SqlAzure (depends on Azure, Core)
├── RevolutionaryStuff.Storage.Providers.Azure.Blob (depends on Azure, Storage)
├── RevolutionaryStuff.Storage.Providers.LocalFileSystem (depends on Storage)
└── revolutionarystuff.com (depends on Core, AspNetCore) [Web App]
    │
Level 3 (Top Level)
└── RevolutionaryStuff.Data.JsonStore.Cosmos (depends on Core, Data.JsonStore, Data.Cosmos)
```

### Project Groupings for All-At-Once Migration

Since this is an All-At-Once strategy, all projects are upgraded simultaneously. However, understanding the dependency levels is important for troubleshooting:

| Level | Projects | Issue Count | Description |
|-------|----------|-------------|-------------|
| 0 | 1 | 92 | Foundation library - all other projects depend on this |
| 1 | 8 | 64 | Direct dependents of Core, including test project |
| 2 | 8 | 96 | Extended functionality, including web application |
| 3 | 1 | 2 | Top-level composite project |

### Critical Path
The critical path for compilation is:
1. `RevolutionaryStuff.Core` → must compile first (foundation)
2. All Level 1 projects → can compile in parallel once Core compiles
3. All Level 2 projects → can compile once their Level 1 dependencies compile
4. `RevolutionaryStuff.Data.JsonStore.Cosmos` → compiles last

### Circular Dependencies
**None detected** - Clean hierarchical structure enables straightforward all-at-once upgrade.

---

## Project-by-Project Plans

All projects are upgraded simultaneously as part of the All-At-Once strategy. This section provides detailed specifications for each project.

---

### Level 0: Foundation

#### RevolutionaryStuff.Core

**Current State**:
- Target Framework: `net9.0`
- Project Type: ClassLibrary
- Files: 181
- Issues: 92 (1 mandatory, 91 potential)
- Dependencies: None (foundation library)
- Used By: All other projects

**Target State**:
- Target Framework: `net10.0`

**Package Updates**:
| Package | Current | Target |
|---------|---------|--------|
| Microsoft.Extensions.Configuration.EnvironmentVariables | 9.0.9 | 10.0.2 |
| Microsoft.Extensions.Configuration.Json | 9.0.9 | 10.0.2 |
| Microsoft.Extensions.Configuration.UserSecrets | 9.0.9 | 10.0.2 |
| Microsoft.Extensions.Hosting.Abstractions | 9.0.9 | 10.0.2 |
| Microsoft.Extensions.Http | 9.0.9 | 10.0.2 |
| Microsoft.Extensions.Logging.Console | 9.0.9 | 10.0.2 |
| System.IO.Hashing | 9.0.11 | 10.0.2 |

**Breaking Changes to Address**:
- `System.IO.Hashing` API changes in `Crypto/Hash.cs` (32 occurrences)
- See Breaking Changes Catalog for specific line numbers and methods

**Validation**:
- [ ] Project builds without errors
- [ ] Project builds without warnings
- [ ] All dependent projects can reference this successfully

---

### Level 1: Core Dependents

#### RevolutionaryStuff.ApiCore

**Current State**:
- Target Framework: `net9.0`
- Project Type: ClassLibrary
- Issues: 10 (1 mandatory, 9 potential)
- Dependencies: RevolutionaryStuff.Core
- Used By: RevolutionaryStuff.Dapr

**Target State**:
- Target Framework: `net10.0`

**Package Updates**:
| Package | Current | Target |
|---------|---------|--------|
| Microsoft.Extensions.Http.Resilience | 9.9.0 | 10.2.0 |
| Microsoft.Extensions.ServiceDiscovery | 9.4.2 | 10.2.0 |

**Breaking Changes**: Behavioral changes only (Uri constructor)

**Validation**:
- [ ] Project builds without errors

---

#### RevolutionaryStuff.AspNetCore

**Current State**:
- Target Framework: `net9.0`
- Project Type: ClassLibrary
- Issues: 2 (1 mandatory, 1 potential)
- Dependencies: RevolutionaryStuff.Core
- Used By: revolutionarystuff.com

**Target State**:
- Target Framework: `net10.0`

**Package Updates**:
| Package | Current | Target |
|---------|---------|--------|
| OpenTelemetry.Instrumentation.AspNetCore | 1.12.0 | 1.15.0 |
| OpenTelemetry.Instrumentation.Http | 1.12.0 | 1.15.0 |

**Breaking Changes**: None expected

**Validation**:
- [ ] Project builds without errors

---

#### RevolutionaryStuff.Azure

**Current State**:
- Target Framework: `net9.0`
- Project Type: ClassLibrary
- Issues: 19 (1 mandatory, 18 potential)
- Dependencies: RevolutionaryStuff.Core
- Used By: RevolutionaryStuff.Data.Cosmos, RevolutionaryStuff.Data.SqlAzure, RevolutionaryStuff.Storage.Providers.Azure.Blob

**Target State**:
- Target Framework: `net10.0`

**Package Updates**: None required (Azure packages already compatible)

**Breaking Changes to Address**:
- `System.BinaryData` API changes in:
  - `Services/Messaging/Inbound/InboundMessage.cs` (14 occurrences)
  - `Services/Messaging/Outbound/ServiceBus/ServiceBusMessageSender.cs` (2 occurrences)

**Validation**:
- [ ] Project builds without errors
- [ ] Service Bus messaging functionality works correctly

---

#### RevolutionaryStuff.Crm

**Current State**:
- Target Framework: `net9.0`
- Project Type: ClassLibrary
- Issues: 1 (1 mandatory)
- Dependencies: RevolutionaryStuff.Core
- Used By: RevolutionaryStuff.Crm.MondayCom, RevolutionaryStuff.Crm.OpenPhone

**Target State**:
- Target Framework: `net10.0`

**Package Updates**: None required

**Breaking Changes**: None expected

**Validation**:
- [ ] Project builds without errors

---

#### RevolutionaryStuff.Data.JsonStore

**Current State**:
- Target Framework: `net9.0`
- Project Type: ClassLibrary
- Issues: 9 (1 mandatory, 8 potential)
- Dependencies: RevolutionaryStuff.Core
- Used By: RevolutionaryStuff.Data.JsonStore.Cosmos

**Target State**:
- Target Framework: `net10.0`

**Package Updates**: None required

**Breaking Changes**: Source incompatibilities may exist (8 potential issues)

**Validation**:
- [ ] Project builds without errors

---

#### RevolutionaryStuff.Data.SqlServer

**Current State**:
- Target Framework: `net9.0`
- Project Type: ClassLibrary
- Issues: 1 (1 mandatory)
- Dependencies: RevolutionaryStuff.Core
- Used By: None (top-level)

**Target State**:
- Target Framework: `net10.0`

**Package Updates**: None required

**Breaking Changes**: None expected

**Validation**:
- [ ] Project builds without errors

---

#### RevolutionaryStuff.Storage

**Current State**:
- Target Framework: `net9.0`
- Project Type: ClassLibrary
- Issues: 2 (1 mandatory, 1 potential)
- Dependencies: RevolutionaryStuff.Core
- Used By: RevolutionaryStuff.Storage.Providers.Azure.Blob, RevolutionaryStuff.Storage.Providers.LocalFileSystem

**Target State**:
- Target Framework: `net10.0`

**Package Updates**: None required

**Breaking Changes**: Source incompatibility may exist (1 potential issue)

**Validation**:
- [ ] Project builds without errors

---

#### RevolutionaryStuff.Core.Tests

**Current State**:
- Target Framework: `net9.0`
- Project Type: DotNetCoreApp (Test Project)
- Issues: 20 (1 mandatory, 19 potential)
- Dependencies: RevolutionaryStuff.Core
- Used By: None (test project)

**Target State**:
- Target Framework: `net10.0`

**Package Updates**: None required (test packages already compatible)

**Breaking Changes**: Source incompatibilities may exist in test code

**Validation**:
- [ ] Project builds without errors
- [ ] All tests pass

---

### Level 2: Extended Dependencies

#### revolutionarystuff.com

**Current State**:
- Target Framework: `net9.0`
- Project Type: AspNetCore (Razor Pages)
- Issues: 2 (1 mandatory, 1 potential)
- Dependencies: RevolutionaryStuff.Core, RevolutionaryStuff.AspNetCore
- Used By: None (top-level web application)

**Target State**:
- Target Framework: `net10.0`

**Package Updates**:
| Package | Current | Target |
|---------|---------|--------|
| Microsoft.AspNetCore.OpenApi | 9.0.9 | 10.0.2 |

**Breaking Changes**: Behavioral changes only

**Validation**:
- [ ] Project builds without errors
- [ ] Application starts successfully
- [ ] Razor Pages render correctly

---

#### RevolutionaryStuff.Crm.MondayCom

**Current State**:
- Target Framework: `net9.0`
- Project Type: ClassLibrary
- Issues: 9 (1 mandatory, 8 potential)
- Dependencies: RevolutionaryStuff.Crm

**Target State**:
- Target Framework: `net10.0`

**Package Updates**: None required

**Validation**:
- [ ] Project builds without errors

---

#### RevolutionaryStuff.Crm.OpenPhone

**Current State**:
- Target Framework: `net9.0`
- Project Type: ClassLibrary
- Issues: 28 (1 mandatory, 27 potential)
- Dependencies: RevolutionaryStuff.Crm

**Target State**:
- Target Framework: `net10.0`

**Package Updates**: None required

**Breaking Changes**: Multiple behavioral changes (Uri constructor)

**Validation**:
- [ ] Project builds without errors

---

#### RevolutionaryStuff.Dapr

**Current State**:
- Target Framework: `net9.0`
- Project Type: ClassLibrary
- Issues: 1 (1 mandatory)
- Dependencies: RevolutionaryStuff.ApiCore

**Target State**:
- Target Framework: `net10.0`

**Package Updates**: None required (Dapr.AspNetCore already compatible)

**Validation**:
- [ ] Project builds without errors

---

#### RevolutionaryStuff.Data.Cosmos

**Current State**:
- Target Framework: `net9.0`
- Project Type: ClassLibrary
- Issues: 4 (1 mandatory, 3 potential)
- Dependencies: RevolutionaryStuff.Azure, RevolutionaryStuff.Core

**Target State**:
- Target Framework: `net10.0`

**Package Updates**: None required (Microsoft.Azure.Cosmos already compatible)

**Breaking Changes**: Source incompatibilities may exist

**Validation**:
- [ ] Project builds without errors

---

#### RevolutionaryStuff.Data.SqlAzure

**Current State**:
- Target Framework: `net9.0`
- Project Type: ClassLibrary
- Issues: 2 (1 mandatory, 1 potential)
- Dependencies: RevolutionaryStuff.Azure, RevolutionaryStuff.Core

**Target State**:
- Target Framework: `net10.0`

**Package Updates**:
| Package | Current | Target |
|---------|---------|--------|
| Microsoft.EntityFrameworkCore.SqlServer | 9.0.9 | 10.0.2 |
| Microsoft.Extensions.Hosting | 9.0.9 | 10.0.2 |

**Validation**:
- [ ] Project builds without errors

---

#### RevolutionaryStuff.Storage.Providers.Azure.Blob

**Current State**:
- Target Framework: `net9.0`
- Project Type: ClassLibrary
- Issues: 47 (1 mandatory, 46 potential)
- Dependencies: RevolutionaryStuff.Azure, RevolutionaryStuff.Storage

**Target State**:
- Target Framework: `net10.0`

**Package Updates**: None required

**Breaking Changes**: 
- Source incompatibility (1 occurrence)
- Many behavioral changes (45 occurrences - primarily Uri constructor)

**Validation**:
- [ ] Project builds without errors

---

#### RevolutionaryStuff.Storage.Providers.LocalFileSystem

**Current State**:
- Target Framework: `net9.0`
- Project Type: ClassLibrary
- Issues: 3 (1 mandatory, 2 potential)
- Dependencies: RevolutionaryStuff.Storage

**Target State**:
- Target Framework: `net10.0`

**Package Updates**: None required

**Validation**:
- [ ] Project builds without errors

---

### Level 3: Top Level

#### RevolutionaryStuff.Data.JsonStore.Cosmos

**Current State**:
- Target Framework: `net9.0`
- Project Type: ClassLibrary
- Issues: 2 (1 mandatory, 1 potential)
- Dependencies: RevolutionaryStuff.Core, RevolutionaryStuff.Data.JsonStore, RevolutionaryStuff.Data.Cosmos

**Target State**:
- Target Framework: `net10.0`

**Package Updates**: None required

**Breaking Changes**: Source incompatibility may exist (1 potential issue)

**Validation**:
- [ ] Project builds without errors

---

## Package Update Reference

### Packages Requiring Updates

All packages below should be updated as part of the atomic upgrade operation.

#### Microsoft.Extensions Packages (RevolutionaryStuff.Core)

| Package | Current Version | Target Version | Update Reason |
|---------|-----------------|----------------|---------------|
| Microsoft.Extensions.Configuration.EnvironmentVariables | 9.0.9 | 10.0.2 | Framework alignment |
| Microsoft.Extensions.Configuration.Json | 9.0.9 | 10.0.2 | Framework alignment |
| Microsoft.Extensions.Configuration.UserSecrets | 9.0.9 | 10.0.2 | Framework alignment |
| Microsoft.Extensions.Hosting.Abstractions | 9.0.9 | 10.0.2 | Framework alignment |
| Microsoft.Extensions.Http | 9.0.9 | 10.0.2 | Framework alignment |
| Microsoft.Extensions.Logging.Console | 9.0.9 | 10.0.2 | Framework alignment |
| System.IO.Hashing | 9.0.11 | 10.0.2 | Framework alignment |

#### ASP.NET Core Packages (revolutionarystuff.com)

| Package | Current Version | Target Version | Update Reason |
|---------|-----------------|----------------|---------------|
| Microsoft.AspNetCore.OpenApi | 9.0.9 | 10.0.2 | Framework alignment |

#### Entity Framework & Data Packages (RevolutionaryStuff.Data.SqlAzure)

| Package | Current Version | Target Version | Update Reason |
|---------|-----------------|----------------|---------------|
| Microsoft.EntityFrameworkCore.SqlServer | 9.0.9 | 10.0.2 | Framework alignment |
| Microsoft.Extensions.Hosting | 9.0.9 | 10.0.2 | Framework alignment |

#### Resilience & Service Discovery (RevolutionaryStuff.ApiCore)

| Package | Current Version | Target Version | Update Reason |
|---------|-----------------|----------------|---------------|
| Microsoft.Extensions.Http.Resilience | 9.9.0 | 10.2.0 | Framework alignment |
| Microsoft.Extensions.ServiceDiscovery | 9.4.2 | 10.2.0 | Framework alignment |

#### OpenTelemetry Packages (RevolutionaryStuff.AspNetCore)

| Package | Current Version | Target Version | Update Reason |
|---------|-----------------|----------------|---------------|
| OpenTelemetry.Instrumentation.AspNetCore | 1.12.0 | 1.15.0 | Recommended upgrade |
| OpenTelemetry.Instrumentation.Http | 1.12.0 | 1.15.0 | Recommended upgrade |

### Packages Remaining Compatible (No Update Required)

The following packages are already compatible with net10.0 and require no changes:

- Azure.Identity (1.17.1)
- Azure.Messaging.ServiceBus (7.20.1)
- Azure.Security.KeyVault.Keys (4.8.0)
- Azure.Security.KeyVault.Secrets (4.8.0)
- Azure.Storage.Files.DataLake (12.24.0)
- Dapr.AspNetCore (1.16.1)
- DotNet.Glob (3.1.3)
- Microsoft.Azure.Cosmos (3.56.0)
- Microsoft.Data.SqlClient (6.1.3)
- Microsoft.NET.Test.Sdk (18.0.1)
- MSTest.TestAdapter (4.0.2)
- MSTest.TestFramework (4.0.2)
- NCrontab (3.4.0)
- OpenTelemetry.Exporter.OpenTelemetryProtocol (1.12.0)
- OpenTelemetry.Extensions.Hosting (1.12.0)
- OpenTelemetry.Instrumentation.Runtime (1.12.0)
- Polly (8.6.5)
- Polly.Contrib.WaitAndRetry (1.1.1)
- Refit (9.0.2)
- Refit.HttpClientFactory (9.0.2)

---

## Breaking Changes Catalog

### Source Incompatibilities (Require Code Changes)

These issues will cause compilation errors and must be fixed during the upgrade.

#### 1. System.IO.Hashing API Changes

**Affected Project**: `RevolutionaryStuff.Core`  
**Affected File**: `src/RevolutionaryStuff.Core/Crypto/Hash.cs`  
**Occurrences**: 32

**Types Affected**:
- `System.IO.Hashing.NonCryptographicHashAlgorithm`
- `System.IO.Hashing.Crc32`
- `System.IO.Hashing.XxHash32`
- `System.IO.Hashing.XxHash64`
- `System.IO.Hashing.XxHash128`

**Methods Affected**:
- Constructors: `.ctor()` for Crc32, XxHash32, XxHash64, XxHash128
- `NonCryptographicHashAlgorithm.Append(Stream)`
- `NonCryptographicHashAlgorithm.GetHashAndReset()`

**Expected Changes**:
- Constructor signatures may have changed
- Method signatures for `Append()` and `GetHashAndReset()` may require updates
- Review the [Breaking changes in .NET](https://go.microsoft.com/fwlink/?linkid=2262679) documentation

**Code Locations** (Lines in Hash.cs):
- Line 75: `var z = creator();`
- Line 87: `var h = creator();`
- Line 88: `h.Append(st);`
- Line 89: `var buf = h.GetHashAndReset();`
- Lines 156-159: Hash algorithm registrations

#### 2. System.BinaryData API Changes

**Affected Project**: `RevolutionaryStuff.Azure`  
**Affected Files**: 
- `src/RevolutionaryStuff.Azure/Services/Messaging/Inbound/InboundMessage.cs` (14 occurrences)
- `src/RevolutionaryStuff.Azure/Services/Messaging/Outbound/ServiceBus/ServiceBusMessageSender.cs` (2 occurrences)

**Types Affected**:
- `System.BinaryData`

**Methods Affected**:
- `BinaryData.#ctor(String)`
- `BinaryData.#ctor(Byte[])`
- `BinaryData.ToString()`
- `BinaryData.ToStream()`

**Expected Changes**:
- Constructor or method signatures may have changed
- Review Azure SDK compatibility with new BinaryData implementation

**Code Locations** (InboundMessage.cs):
- Line 8, 26, 32: Property access and method calls
- Line 93, 113: `new BinaryData(...)` constructor calls
- Line 125, 137, 150: BinaryData parameter handling

**Code Locations** (ServiceBusMessageSender.cs):
- Line 109: `new BinaryData(await outboundMessage.Payload.ToBufferAsync())`

### Behavioral Changes (Awareness Required)

These changes may affect runtime behavior but do not require code changes to compile.

#### 1. System.Uri Constructor Behavior

**Occurrences**: 150 across multiple projects  
**Severity**: Low - monitor for issues

**Description**: The `System.Uri` constructor behavior may have subtle changes in .NET 10.0. This affects URI parsing and normalization.

**Affected Areas**:
- Key Vault URL handling in `RevolutionaryStuff.Azure`
- Various URI construction throughout the solution

**Recommendation**: 
- No code changes required unless issues observed at runtime
- Monitor logs for URI-related exceptions after upgrade
- Test functionality that constructs URIs from string inputs

### Summary of Code Changes Required

| File | Project | Change Type | Occurrences | Complexity |
|------|---------|-------------|-------------|------------|
| `Crypto/Hash.cs` | RevolutionaryStuff.Core | API signature update | 32 | Medium |
| `Services/Messaging/Inbound/InboundMessage.cs` | RevolutionaryStuff.Azure | API signature update | 14 | Low |
| `Services/Messaging/Outbound/ServiceBus/ServiceBusMessageSender.cs` | RevolutionaryStuff.Azure | API signature update | 2 | Low |

---

## Risk Management

### Risk Assessment by Project

| Project | Risk Level | Risk Factors | Mitigation |
|---------|------------|--------------|------------|
| RevolutionaryStuff.Core | **Medium** | 92 issues, 32 source incompatibilities, foundation library | Address Hash.cs changes carefully; all projects depend on this |
| RevolutionaryStuff.Azure | **Medium** | 19 issues, 16 source incompatibilities | Verify Azure SDK compatibility after BinaryData changes |
| RevolutionaryStuff.Storage.Providers.Azure.Blob | **Low-Medium** | 47 issues (mostly behavioral) | Monitor for URI-related runtime issues |
| RevolutionaryStuff.Core.Tests | **Low** | 20 issues | Test code changes; run full test suite |
| All other projects | **Low** | Minimal issues, mostly framework changes | Standard validation |

### High-Risk Changes

| Change | Project | Description | Mitigation Strategy |
|--------|---------|-------------|---------------------|
| System.IO.Hashing API | RevolutionaryStuff.Core | Constructor and method signature changes for hash algorithms | Review .NET 10 documentation; update method calls accordingly |
| System.BinaryData API | RevolutionaryStuff.Azure | Changes to binary data handling in messaging | Test Service Bus send/receive operations thoroughly |
| Foundation Library | RevolutionaryStuff.Core | All projects depend on Core | Ensure Core compiles first; validate all dependents |

### Security Vulnerabilities

**None identified** - No packages have known security vulnerabilities requiring immediate attention.

### Contingency Plans

#### If Build Fails After Target Framework Update

1. Check if the error is related to documented breaking changes
2. Consult the Breaking Changes Catalog in this plan
3. Apply fixes as documented
4. If issue persists, check [Breaking changes in .NET](https://go.microsoft.com/fwlink/?linkid=2262679)

#### If Tests Fail After Upgrade

1. Identify if failures are due to behavioral changes (Api.0003)
2. Review System.Uri constructor behavior if URI-related tests fail
3. Update test expectations if behavior change is intentional

#### If Critical Blocking Issue Discovered

1. Document the specific error and affected code
2. Search for workarounds in .NET 10 documentation
3. If no workaround available, revert the upgrade branch
4. Report issue and await guidance

### Rollback Strategy

Since this is an All-At-Once upgrade on a dedicated branch:

1. **Immediate Rollback**: `git checkout master` to return to working state
2. **Discard Upgrade**: `git branch -D upgrade-to-NET10` to remove upgrade branch
3. **Partial Recovery**: Cherry-pick successful changes if some projects can be upgraded independently (not recommended for this solution due to tight dependencies)

---

## Testing & Validation Strategy

### Build Validation

After all project files and packages are updated, the solution must build successfully:

1. **Restore Dependencies**
   ```
   dotnet restore RevolutionaryStuff.sln
   ```

2. **Build Entire Solution**
   ```
   dotnet build RevolutionaryStuff.sln --configuration Release
   ```

3. **Expected Outcome**: 0 errors, 0 warnings (or only pre-existing warnings)

### Test Execution

**Test Project**: `RevolutionaryStuff.Core.Tests`

1. **Run All Tests**
   ```
   dotnet test tests/RevolutionaryStuff.Core.Tests/RevolutionaryStuff.Core.Tests.csproj
   ```

2. **Expected Outcome**: All tests pass

### Validation Checklist

#### Pre-Upgrade Verification
- [ ] Currently on `upgrade-to-NET10` branch
- [ ] Starting from clean state (no uncommitted changes)
- [ ] .NET 10 SDK is installed

#### Post-Upgrade Verification
- [ ] All 18 project files updated to `net10.0`
- [ ] All 14 package updates applied
- [ ] Solution restores without errors
- [ ] Solution builds without errors
- [ ] All tests pass
- [ ] No new compiler warnings introduced

### Application Smoke Testing

For the web application (`revolutionarystuff.com`):
- [ ] Application starts without errors
- [ ] Home page loads correctly
- [ ] No runtime exceptions in logs

---

## Complexity & Effort Assessment

### Per-Project Complexity

| Project | Complexity | Reason |
|---------|------------|--------|
| RevolutionaryStuff.Core | **Medium** | 32 source incompatibilities in Hash.cs requiring code changes |
| RevolutionaryStuff.Azure | **Medium** | 16 source incompatibilities in messaging code |
| RevolutionaryStuff.Storage.Providers.Azure.Blob | **Low** | 1 source incompatibility, many behavioral (no code change) |
| RevolutionaryStuff.Core.Tests | **Low** | Test code updates, straightforward |
| All other projects (14) | **Low** | Target framework change only, no code changes expected |

### Overall Assessment

| Category | Assessment |
|----------|------------|
| **Total Complexity** | Medium |
| **Primary Work** | Code fixes in 2 files (Hash.cs, InboundMessage.cs) |
| **Package Updates** | 14 packages across 4 projects (straightforward) |
| **Risk Level** | Low-Medium |
| **Confidence** | High - all packages have compatible versions, changes are well-documented |

### Resource Requirements

- **.NET 10 SDK**: Must be installed before upgrade begins
- **Skills Required**: C# development, familiarity with hashing APIs and Azure SDK
- **Parallel Capacity**: Single developer can complete this upgrade

---

## Source Control Strategy

### Branch Strategy

| Branch | Purpose |
|--------|--------|
| `master` | Main development branch (source) |
| `upgrade-to-NET10` | Upgrade work branch (target) |

### Commit Strategy

**Single Commit Approach** (All-At-Once Strategy)

All upgrade changes should be committed together in a single commit:

```
git add .
git commit -m "Upgrade solution from .NET 9 to .NET 10

- Update all 18 projects to target net10.0
- Update 14 NuGet packages to .NET 10 compatible versions
- Fix System.IO.Hashing API changes in Hash.cs
- Fix System.BinaryData API changes in Azure messaging
- All tests passing"
```

### Merge Process

1. **Verify all checks pass**:
   - Solution builds without errors
   - All tests pass
   - No new warnings

2. **Create Pull Request**:
   - Source: `upgrade-to-NET10`
   - Target: `master`
   - Title: "Upgrade to .NET 10.0 (LTS)"

3. **PR Checklist**:
   - [ ] All projects target net10.0
   - [ ] All package updates applied
   - [ ] Breaking changes addressed
   - [ ] Tests pass
   - [ ] Code reviewed

4. **Merge**: Squash and merge to maintain clean history

---

## Success Criteria

### Technical Criteria

- [ ] All 18 projects successfully target `net10.0`
- [ ] All 14 package updates applied to correct versions
- [ ] Solution builds with 0 errors
- [ ] Solution builds with no new warnings
- [ ] All unit tests pass (RevolutionaryStuff.Core.Tests)
- [ ] No package dependency conflicts
- [ ] No security vulnerabilities (maintained at 0)

### Code Quality Criteria

- [ ] All source incompatibilities resolved (Hash.cs, InboundMessage.cs, ServiceBusMessageSender.cs)
- [ ] Code changes follow existing patterns and style
- [ ] No temporary workarounds or hacks introduced
- [ ] Test coverage maintained

### Process Criteria

- [ ] All-At-Once strategy followed (single atomic upgrade)
- [ ] Single commit containing all changes
- [ ] Upgrade performed on dedicated branch
- [ ] Changes ready for pull request review

### Definition of Done

The upgrade is complete when:

1. ✅ All projects compile against .NET 10.0
2. ✅ All packages updated to .NET 10.0 compatible versions
3. ✅ All breaking changes addressed
4. ✅ All tests pass
5. ✅ Changes committed to `upgrade-to-NET10` branch
6. ✅ Ready for merge to `master`
