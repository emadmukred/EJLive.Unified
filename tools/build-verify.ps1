param(
    [string]$Configuration = "Debug",
    [switch]$CiRelease,
    [int]$NullableWarningsStage = 1,
    [switch]$GenerateNullableReport
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

if ($CiRelease) {
    $Configuration = "Release"
}

$commonBuildArgs = @(
    "-m:1",
    "/p:BuildInParallel=false",
    "/p:UseSharedCompilation=false"
)

if ($CiRelease) {
    $commonBuildArgs += "/p:ContinuousIntegrationBuild=true"
    $commonBuildArgs += "/p:NullableWarningsStage=$NullableWarningsStage"
}

dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -c $Configuration @commonBuildArgs
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -c $Configuration @commonBuildArgs
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -c $Configuration @commonBuildArgs

if ($GenerateNullableReport) {
    & (Join-Path $PSScriptRoot "nullable-gate-report.ps1") `
        -Phase $NullableWarningsStage `
        -Configuration $Configuration `
        -ContinuousIntegrationBuild:$CiRelease
}
