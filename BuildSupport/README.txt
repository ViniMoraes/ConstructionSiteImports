Local reference preparation only. This helper does not decompile DLLs, edit game
files, delete directories, or install files into Timberborn. It writes publicized
copies for compilation into the explicitly supplied output directory.

Run from the repository root. Build with the local SDK (set process-local
DOTNET_ROOT, DOTNET_CLI_HOME and NUGET_PACKAGES to the repository's dotnet,
dotnet-cli-home and nuget-packages directories):

  .\dotnet\dotnet.exe build .\BuildSupport\BuildSupport.csproj -c Release

Run:

  .\dotnet\dotnet.exe .\BuildSupport\bin\Release\net10.0\BuildSupport.dll <game-managed-directory> <output-directory> [additional-dll-directory ...]

Only Bindito.*, Timberborn.* and Unity*.dll files are selected from the game input.
All DLL files are selected from additional inputs. Generated files retain their
names. Duplicate names or input/output overlap are rejected. Existing output DLLs
can only be overwritten when recorded in build-support-manifest.json. The helper
verifies the SHA-256 hash of every source file remains unchanged.
