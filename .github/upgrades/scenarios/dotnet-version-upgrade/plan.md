# .NET 11 Upgrade Plan

## Overview

**Target**: Upgrade the complete solution to .NET 11.
**Scope**: 25 SDK-style .NET 10 projects, including libraries, web applications, MAUI and tests.

### Selected Strategy
**All-at-Once** — All projects upgraded together in a single operation.
**Rationale**: User selected an atomic upgrade across the modern .NET solution.

## Upgrade Options

| Option | Selected | Why |
|--------|----------|-----|
| Upgrade Strategy | All-at-Once | User selected one coordinated upgrade. |

## Tasks

### 01-toolchain: Verify .NET 11 toolchain

Verify the installed SDK, workloads and global.json compatibility for the entire solution, including MAUI platform targets.

**Done when**: SDK and workload availability are checked and blockers documented.

### 02-solution-upgrade: Upgrade the solution to .NET 11

Upgrade all 25 SDK-style application, library, MAUI and test projects together. Address recommended dependency updates, source incompatibilities and significant behavioral changes from the assessment; evaluate replacement of legacy OpenAPI helpers with supported modern APIs while preserving documented behavior. Update assembly versions consistently and add tests for new behavior.

Research the project-specific assessment, package dependencies and existing tests before changes. Pay special attention to MAUI platform requirements, generated OpenAPI contracts, and the 136 source compatibility and 198 behavioral notices in the assessment.

**Done when**: Projects target .NET 11, required dependency and code changes are complete, versions are consistent, the solution builds and applicable tests pass, and unresolved environmental or platform checks are reported.
