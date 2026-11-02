Construction Site Imports verification

Result on 2026-11-02: standalone 48 passed, 0 failed; with the original
Construction Site Hauler available 48 passed, 0 failed (96 total).

Run from the repository root in PowerShell after building the staged addon:

$env:DOTNET_ROOT = Join-Path $PWD 'dotnet'
$env:DOTNET_CLI_HOME = Join-Path $PWD 'dotnet-cli-home'
$env:NUGET_PACKAGES = Join-Path $PWD 'nuget-packages'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
& .\dotnet\dotnet.exe run --project .\ImportTests\ImportTests.csproj -c Release

The default mode loads the addon in an isolated AssemblyLoadContext that
actively rejects ConstructionSiteHauler. That DLL is excluded from the resolver
even when the original mod is installed on this computer.

To also verify metadata coexistence with the original mod available:
& .\dotnet\dotnet.exe run --project .\ImportTests\ImportTests.csproj -c Release -- --with-hauler

Each mode must run as a separate process. The optional positional argument is
the Steam steamapps folder; it can be combined with --with-hauler, for example:
& .\dotnet\dotnet.exe run --project .\ImportTests\ImportTests.csproj -c Release -- 'D:\Jogos\Steam\steamapps' --with-hauler

Coverage (48 checks per mode):
- 34 linked-source calculation cases: paper20 without a warehouse, local stock,
  partial/full incoming and construction reservations, exporter thresholds,
  source Capacity0, final item rounding, vanilla warehouse demand, stale demand,
  construction stock retention and source surplus clamps.
- All four Harmony postfix targets uniquely resolve in actual native game
  assemblies. Every parameter name/type and ref return slot matches.
- The compiled addon has no ConstructionSiteHauler assembly dependency and all
  addon types and method signatures resolve without the original mod.
- The native construction-site decorator implements initialization, deletion,
  and unfinished-state lifecycle interfaces. Registry IL registers its own
  entity tracker and addon methods do not register or replace hauling targets.
- Standalone mode proves the resolver refuses ConstructionSiteHauler; the
  optional coexistence mode loads both addons' component metadata together.
- A real managed DistributableGood at Capacity0 confirms the addon exports
  only 5 surplus items when stock25 minus local construction20 would otherwise
  allow vanilla to request a destination warehouse's full 1000 free capacity.

The suite loads the staged addon and installed game/dependency DLLs for metadata
and plain managed distribution record tests. It does not launch Timberborn,
create Unity components, invoke registry initialization, apply Harmony patches,
call native APIs, install the addon, or modify game files. In-game play testing
of construction delivery and both-mod behavior remains separate.
