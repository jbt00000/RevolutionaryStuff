# .NET 11 Upgrade

## Preferences
- **Flow Mode**: Automatic
- **Target Framework**: net11.0
- **Scope**: Only projects in the active RevolutionaryStuff.slnx; do not upgrade projects merely because they exist on disk.
- **Modernization**: Fix breaking changes and evaluate modern replacements for legacy code, including OpenAPI helpers.
- **MAUI**: Android API minimum 24 is approved. Re-create the MAUI assembly if necessary, provided Apple and Android remain supported.
- **Assembly versions**: Set Version, AssemblyVersion and FileVersion to 4.208.110 in solution assemblies; leave out-of-solution projects unchanged.
- **Existing test failures**: The user believes the observed failures predate the upgrade. Record them as suspected pre-existing issues, not confirmed regressions; no passing pre-upgrade test baseline is available.

## Upgrade Options

### Strategy
- Upgrade Strategy: All-at-Once

## Strategy
**Selected**: All-at-Once
**Rationale**: User selected a single coordinated upgrade of the 25 modern .NET projects.

### Execution Constraints
- Upgrade projects together; avoid staged framework rollouts.
- Resolve framework, package, and API compatibility together.
- Build the solution and run applicable tests before completing the upgrade.

## Source Control
- **Source Branch**: master
- **Working Branch**: upgrade-dotnet-11-2
- **Commit Strategy**: Manual
- **Branch Sync**: Disabled
