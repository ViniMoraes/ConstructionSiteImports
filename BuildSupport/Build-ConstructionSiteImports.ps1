[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string]$SteamApps = 'D:\Jogos\Steam\steamapps',
    [string]$LogPath = ''
)

$ErrorActionPreference = 'Stop'
if (-not $LogPath) { $LogPath = Join-Path $PSScriptRoot 'construction-site-imports-build.log' }
$taskWork = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$taskRepo = [IO.Path]::GetFullPath((Join-Path $taskWork 'TimberbornMods'))
$taskStaging = [IO.Path]::GetFullPath((Join-Path $taskWork 'staging'))
$taskModFolder = [IO.Path]::GetFullPath((Join-Path $taskStaging 'ConstructionSiteImports\version-1.1'))
$taskManaged = Join-Path $SteamApps 'common\Timberborn\Timberborn_Data\Managed'
$taskWorkshop = Join-Path $SteamApps 'workshop\content\1062090'
$taskModdable = Join-Path $taskWorkshop '3578009707\version-1.1'
$taskTimberUi = Join-Path $taskWorkshop '3430626620\version-1.1'
$taskProject = Join-Path $taskRepo 'ConstructionSiteImports\ConstructionSiteImports.csproj'
$taskDotnet = Join-Path $taskWork 'dotnet\dotnet.exe'
$taskTargets = Join-Path $PSScriptRoot 'LocalBuild.targets'

foreach ($taskRequiredPath in @($taskManaged, $taskWorkshop, $taskModdable, $taskTimberUi, $taskProject, $taskDotnet, $taskTargets)) {
    if (-not (Test-Path -LiteralPath $taskRequiredPath)) { throw "Required local build path is missing: $taskRequiredPath" }
}
if (-not $taskModFolder.StartsWith($taskStaging + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw "The resolved mod output must stay inside the workspace staging folder: $taskModFolder"
}

$env:DOTNET_ROOT = Join-Path $taskWork 'dotnet'
$env:DOTNET_CLI_HOME = Join-Path $taskWork 'dotnet-cli-home'
$env:NUGET_PACKAGES = Join-Path $taskWork 'nuget-packages'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'

$taskBuildArguments = @(
    'build', $taskProject, '-c', $Configuration, '--nologo',
    "-p:AssemblyPath=$taskManaged",
    "-p:ModPath=$taskWorkshop\",
    "-p:GameSolutionFolder=$taskRepo\",
    "-p:GameModsFolder=$taskStaging\",
    "-p:ModFolder=$taskModFolder\",
    "-p:LocalStagingModFolder=$taskModFolder\",
    "-p:ModdableTimberbornPath=$taskModdable\**\*.dll",
    "-p:TimberUiPath=$taskTimberUi\**\*.dll",
    "-p:CustomAfterMicrosoftCommonTargets=$taskTargets"
)
Write-Host "Building $taskProject"
Write-Host "Output is confined to $taskModFolder"
& $taskDotnet @taskBuildArguments 2>&1 | Tee-Object -FilePath $LogPath
$taskExitCode = $LASTEXITCODE
Write-Host "Build exit code: $taskExitCode"
Write-Host "Build log: $LogPath"
exit $taskExitCode
