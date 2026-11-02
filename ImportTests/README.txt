Construction Site Imports verification

Result on 2026-11-02: 43 passed, 0 failed.

Run from the workspace root in PowerShell:

$env:DOTNET_ROOT = Join-Path $PWD 'dotnet'
$env:DOTNET_CLI_HOME = Join-Path $PWD 'dotnet-cli-home'
$env:NUGET_PACKAGES = Join-Path $PWD 'nuget-packages'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
& .\dotnet\dotnet.exe run --project .\ImportTests\ImportTests.csproj -c Release

The optional first argument is the Steam steamapps folder, for example:
& .\dotnet\dotnet.exe run --project .\ImportTests\ImportTests.csproj -c Release -- 'D:\Jogos\Steam\steamapps'

Coverage:
- 34 linked-source calculation cases: paper20 without a warehouse, local stock,
  partial/full incoming and construction reservations, exporter thresholds,
  source Capacity0, final item rounding, vanilla warehouse demand, stale demand,
  construction stock retention and source surplus clamps.
- All six Harmony postfix targets uniquely resolve in the actual game/original
  Workshop assemblies. Every parameter name/type and ref return slot matches.
- All compiled addon types and their method signatures/dependencies resolve.
- A real managed DistributableGood at Capacity0 confirms the addon exports
  only 5 surplus items when stock25 minus local construction20 would otherwise
  allow vanilla to request a destination warehouse's full 1000 free capacity.

The suite loads the staged addon and the installed game/Workshop DLLs for
metadata and plain managed distribution record tests. It does not launch
Timberborn, create Unity components, apply Harmony patches, call native APIs,
install the addon, or modify game files. In-game play testing remains separate.
