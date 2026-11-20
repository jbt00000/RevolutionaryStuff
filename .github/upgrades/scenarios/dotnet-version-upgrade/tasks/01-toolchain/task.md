# 01-toolchain: Verify .NET 11 toolchain

Verify the installed SDK, workloads and global.json compatibility for the entire solution, including MAUI platform targets.

**Done when**: SDK and workload availability are checked and blockers documented.

## Research
- .NET SDK 11.0.100 is installed and the SDK validation tool reported compatibility.
- global.json validation reported no changes needed.
- Installed 11.0 workloads include Android, iOS, Mac Catalyst, MAUI Windows and wasm-tools. Remote Apple build requirements remain to be tested during the upgrade build.
