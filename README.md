# Construction Site Imports

An add-on for Timberborn that lets construction sites served by **Construction Site Hauler** request materials through district crossings, without requiring a warehouse for each material. For example, a construction site that needs 20 paper can receive them from another district.

Requires Timberborn **1.1.2.8** and **Construction Site Hauler 11.0.2 or later**, with Moddable Timberborn, TimberUI and Harmony installed. The versions used for the initial build and checks are listed in [REVISAO.txt](REVISAO.txt).

Construction Site Hauler is a required runtime dependency: this add-on uses its construction-site components, hauling registrations and refresh/unregister hooks. Those APIs are available in [11.0.1](https://github.com/datvm/TimberbornMods/commit/62c3a90bd9e07cc07dc716c67a2e73e523019c76), but the manifest keeps **11.0.2 as the minimum supported version** because it includes the [district-access fix for sites such as Tubeway Stations](https://github.com/datvm/TimberbornMods/commit/76459d7822725058868540c15da809346498b1bc). Earlier versions have not been validated with this add-on.

Enable both mods in the game, set the material to **Import If Needed** in the receiving district, and keep the district crossing staffed and connected to both districts. The supplying district must allow exports and have stock available. Local haulers carry the materials from the crossing to the construction site. See [LEIA-ME.txt](LEIA-ME.txt) for behavior details and manual installation instructions (in Portuguese).

To build on Windows, follow [COMO-COMPILAR.txt](COMO-COMPILAR.txt) (in Portuguese): install a local .NET 10 SDK, prepare the references from your installation, and run `BuildSupport/Build-ConstructionSiteImports.ps1 -SteamApps '<your Steam library>\steamapps'`. The output is placed in `staging/ConstructionSiteImports/version-1.1`. The build script does not install the add-on in the game.

This repository includes the add-on source, the required shared build files, the reference preparation tool and the checks in `ImportTests`. The game and dependency libraries must be installed locally; they are not included here. Steam paths are configurable and must point to your installation.

After building, run the following in PowerShell from the repository root:

```powershell
& .\dotnet\dotnet.exe run --project .\ImportTests\ImportTests.csproj -c Release -- '<your Steam library>\steamapps'
```

The initial build completed without warnings or errors and passed **43 checks** covering calculations and compatibility with the installed libraries. The complete transport flow **still needs to be tested in the game**. The add-on has not been published on Steam.

Based on Luke Vo's work in [datvm/TimberbornMods](https://github.com/datvm/TimberbornMods), commit `dab484c3fd194d182170e837046f7fc5ac03f9f9`. The [original Construction Site Hauler](https://steamcommunity.com/sharedfiles/filedetails/?id=3779332547) remains required. The [MIT license](LICENSE) and original notices have been preserved.
