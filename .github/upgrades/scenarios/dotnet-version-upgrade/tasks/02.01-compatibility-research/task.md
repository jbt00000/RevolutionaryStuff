# 02.01-compatibility-research: Research upgrade compatibility and OpenAPI contracts

## Objective
Research assessment findings, framework dependencies, OpenAPI helpers and validation scope for the coordinated upgrade.

**Done when**: Findings and upgrade risks are documented for the atomic implementation.

## Findings
- The assessment covers 25 SDK-style .NET 10 projects; see assessment/project-graph.md and per-project reports. It identifies 15 recommended package updates, 136 source compatibility notices and 198 behavioral notices, not confirmed defects.
- Target frameworks and inspected package versions are project-local. Microsoft.AspNetCore.OpenApi 11.0.0 and Microsoft.Maui.Controls 11.0.10 are supported targets. The installed SDK and workload manifests support .NET 11, but Apple platform compilation is not verified.
- The 27 source project files with explicit assembly versions all use 4.207.100; increment all three properties to 4.208.100 consistently, including projects outside the solution as required by repository instructions.
- OpenAPI upload and response helpers use operation transformers to preserve explicit binary schemas, MIME types, descriptions and status codes. Confirm equivalent behavior before deleting or replacing them. Two public document transformers are unreferenced internally, which does not establish that external consumers do not use them.
- The ApiCore test project is outside the solution and contains an executable OpenAPI schema test; include it in targeted validation and retarget it alongside its dependency. Some solution tests are excluded from the default Debug build and require explicit runs.
- Baseline IDE solution build succeeded. The generated assessment and project-specific documents remain the starting points for each project's migration, and behavioral notices need documented applicability and targeted tests where impactful.
