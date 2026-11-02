# Construction Site Imports

A mod for Timberborn that lets construction sites request materials through district crossings, without requiring a warehouse for each material. For example, a construction site that needs 20 paper can receive them from another district.

Requires Timberborn **1.1.2.8** and **Moddable Timberborn 11.1.0 or later**, including its TimberUI and Harmony dependencies. **Construction Site Hauler is optional** and is not required to build or load this mod. Build and validation details are listed in [REVISAO.txt](REVISAO.txt).

Construction Site Imports tracks construction sites and their district access independently. It adds import demand and protects the requested materials against immediate re-export. It does not register hauling destinations or replace Construction Site Hauler's transport system.

Enable Construction Site Imports and its required dependencies, set the material to **Import If Needed** in the receiving district, and keep the district crossing staffed and connected to both districts. The supplying district must allow exports and have stock available.

- **Without Construction Site Hauler:** the game's builders collect the imported materials from the crossing and take them to the construction site.
- **With Construction Site Hauler:** that mod also lets local haulers deliver the materials to the site. Each mod handles its own function; Construction Site Imports adds no duplicate hauling destinations.

See [LEIA-ME.txt](LEIA-ME.txt) for behavior details and manual installation instructions (in Portuguese).

To build on Windows, follow [COMO-COMPILAR.txt](COMO-COMPILAR.txt) (in Portuguese): install a local .NET 10 SDK, prepare the references from your installation, and run `BuildSupport/Build-ConstructionSiteImports.ps1 -SteamApps '<your Steam library>\steamapps'`. The output is placed in `staging/ConstructionSiteImports/version-1.1`. The build script does not install the mod in the game.

This repository includes the mod source, the required shared build files, the reference preparation tool and the checks in `ImportTests`. The game and dependency libraries must be installed locally; they are not included here. Steam paths are configurable and must point to your installation.

After building, run the following in PowerShell from the repository root:

```powershell
& .\dotnet\dotnet.exe run --project .\ImportTests\ImportTests.csproj -c Release -- '<your Steam library>\steamapps'
```

The Release build completed without warnings or errors. **48 checks passed without Construction Site Hauler, and 48 passed with it available in the same loading environment.** These checks cover demand calculations, native lifecycle bindings, compatibility with the installed libraries and the absence of a required Construction Site Hauler assembly reference. They do not run the game simulation. The complete transport flow **still needs to be tested in the game, both with and without Construction Site Hauler**. The mod has not been published on Steam.

Based on Luke Vo's work in [datvm/TimberbornMods](https://github.com/datvm/TimberbornMods), commit `dab484c3fd194d182170e837046f7fc5ac03f9f9`. The [original Construction Site Hauler](https://steamcommunity.com/sharedfiles/filedetails/?id=3779332547) can be installed alongside this mod. The [MIT license](LICENSE) and original notices have been preserved.
