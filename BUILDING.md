# Building Construction Site Imports

This repository includes the mod, the shared build files required from the original repository, the reference preparation tool and the tests. Game and dependency DLLs are not redistributed here.

## Prerequisites

Use Windows with Timberborn **1.1.2.8** and these mods installed through Steam:

- Moddable Timberborn **11.1.0 or later**.
- TimberUI **11.0.x**.
- Harmony **2.4.x**.
- Mod Settings **1.1**, for the shared references used by the original build configuration.

Construction Site Hauler is optional in the game and is not used to compile this mod. TimberUI and Harmony are dependencies of Moddable Timberborn.

The code was built with **.NET SDK 10.0.403**. Run the commands below from the repository root, which corresponds to the `work` directory used during development.

## Install a local SDK

Open PowerShell in the repository root and run:

```powershell
Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile .\dotnet-install.ps1
& .\dotnet-install.ps1 -Channel 10.0 -InstallDir .\dotnet -NoPath
$env:DOTNET_ROOT = Join-Path $PWD 'dotnet'
$env:DOTNET_CLI_HOME = Join-Path $PWD 'dotnet-cli-home'
$env:NUGET_PACKAGES = Join-Path $PWD 'nuget-packages'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
```

## Prepare local references

Set the Steam library where Timberborn is installed. Replace the example `$steamApps` path with your own installation path:

```powershell
$steamApps = 'D:\Jogos\Steam\steamapps'
$managed = Join-Path $steamApps 'common\Timberborn\Timberborn_Data\Managed'
$modSettings = Join-Path $steamApps 'workshop\content\1062090\3283831040\version-1.1\Scripts'
$references = Join-Path $PWD 'TimberbornMods\GameAssemblyPublicizer\out\common'
```

Generate local copies of the build references:

```powershell
& .\dotnet\dotnet.exe run --project .\BuildSupport\BuildSupport.csproj -c Release -- $managed $references $modSettings
```

The preparation tool uses **BepInEx.AssemblyPublicizer 0.4.3**, restored through NuGet when building. It prepares copies of the references; the original files are not modified.

## Build the mod

```powershell
& .\BuildSupport\Build-ConstructionSiteImports.ps1 -SteamApps $steamApps
```

The output is placed in `staging\ConstructionSiteImports\version-1.1`. The build script copies files only to staging; installation is a separate step.

To install, copy `staging\ConstructionSiteImports` to `Documents\Timberborn\Mods`. See [USAGE.md](USAGE.md) for installation details and how to use the mod.

## Run the checks

After building, run:

```powershell
& .\dotnet\dotnet.exe run --project .\ImportTests\ImportTests.csproj -c Release -- $steamApps
```

The checks verify calculations and integration points against the installed libraries without starting the game simulation. Manual testing in the game is still required, both without Construction Site Hauler and with both mods enabled.

See [BUILD-INFO.txt](BUILD-INFO.txt) for the recorded build and validation details.

## Upstream source and license

Original repository: [datvm/TimberbornMods](https://github.com/datvm/TimberbornMods).

The [MIT license](LICENSE) and upstream revision information are included in this repository.
