# 02-solution-upgrade: Upgrade the solution to .NET 11

Upgrade all 25 SDK-style application, library, MAUI and test projects together. Address recommended dependency updates, source incompatibilities and significant behavioral changes from the assessment; evaluate replacement of legacy OpenAPI helpers with supported modern APIs while preserving documented behavior. Update assembly versions consistently and add tests for new behavior.

Research the project-specific assessment, package dependencies and existing tests before changes. Pay special attention to MAUI platform requirements, generated OpenAPI contracts, and the 136 source compatibility and 198 behavioral notices in the assessment.

**Done when**: Projects target .NET 11, required dependency and code changes are complete, versions are consistent, the solution builds and applicable tests pass, and unresolved environmental or platform checks are reported.

## Scope Inventory
- All 25 solution projects listed in assessment/project-graph.md are SDK-style and currently target .NET 10; MAUI has Android, iOS, Mac Catalyst and Windows platform TFMs. Other repository projects and tests exist outside the solution and must be considered for the repository-wide assembly version convention and dependencies.
- Direct target framework and package properties are in project files; dependency queries for Core, ApiCore and MAUI found project-local references. No Directory.Build props were found.
- Assessment reports no incompatible packages, 15 recommended package updates, 136 source incompatibility notices and 198 behavioral notices. Notices require triage against actual usages and documentation, not automatic fixes.
- NuGet version discovery recommends Microsoft.AspNetCore.OpenApi 11.0.0 and Microsoft.Maui.Controls 11.0.10. Installed .NET 11 workloads cover Android, iOS, Mac Catalyst, Windows and WebAssembly; Apple build availability remains unverified.
- All observed RSLLC project version properties are 4.207.100; verify repository-wide highest version before applying the required minor increment.
- Existing OpenAPI upload and file-response helpers use custom operation transformers. Document transformer types are public even though no internal consumers were found; public API compatibility and generated contracts must be considered before removal.
- Existing API tests are outside the active solution. Core test discovery returned 783 tests; solution build configuration excludes some test projects from Debug builds.
- No STUB markers found. Prerequisite IDE build reported success before changes; full target coverage was not established.

## Behavioral API review
- Assessment groups notices around Uri, MemoryStream, JsonDocument and HttpContent among others. Applicability, documentation and tests remain to be established for each distinct change before task completion.
