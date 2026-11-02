# Construction Site Imports

A mod for Timberborn that lets construction sites request materials through district crossings, without requiring a warehouse for each material. For example, a construction site that needs 20 paper can receive them from another district.

Requires Timberborn **1.1.2.8** and **Moddable Timberborn 11.1.0 or later**, including its TimberUI and Harmony dependencies. Build and validation details are listed in [BUILD-INFO.txt](BUILD-INFO.txt).

Construction Site Imports tracks construction sites and their district access, adds import demand and protects the requested materials against immediate re-export.

Enable Construction Site Imports and its required dependencies, set the material to **Import If Needed** in the receiving district, and keep the district crossing staffed and connected to both districts. The supplying district must allow exports and have stock available. Builders collect the imported materials from the crossing and take them to the construction site.

See [USAGE.md](USAGE.md) for behavior details and manual installation instructions.

To build on Windows, follow [BUILDING.md](BUILDING.md): install a local .NET 10 SDK, prepare the references from your installation, and run `BuildSupport/Build-ConstructionSiteImports.ps1 -SteamApps '<your Steam library>\steamapps'`. The output is placed in `staging/ConstructionSiteImports/version-1.1`. The build script does not install the mod in the game.

This repository includes the mod source, the required shared build files, the reference preparation tool and the checks in `ImportTests`. The game and dependency libraries must be installed locally; they are not included here. Steam paths are configurable and must point to your installation.

After building, run the following in PowerShell from the repository root:

```powershell
& .\dotnet\dotnet.exe run --project .\ImportTests\ImportTests.csproj -c Release -- '<your Steam library>\steamapps'
```

The Release build completed without warnings or errors, and automated verification passed. The checks cover demand calculations, native lifecycle bindings and compatibility with the installed libraries. They do not run the game simulation. The complete transport flow **still needs to be tested in the game**. The mod has not been published on Steam.

Based on Luke Vo's work in [datvm/TimberbornMods](https://github.com/datvm/TimberbornMods), commit `dab484c3fd194d182170e837046f7fc5ac03f9f9`. The [MIT license](LICENSE) and original notices have been preserved.
