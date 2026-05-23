$root = (Resolve-Path ".").Path.TrimEnd('\', '/')
$projectDir = (Resolve-Path "src\EJLive.Client.WinForms").Path

function Resolve-RelativeToRoot {
    param([string]$TargetPath)
    $target = (Resolve-Path $TargetPath -ErrorAction SilentlyContinue)
    if (-not $target) { return $TargetPath.Replace('\', '/') }
    $target = $target.Path
    $targetNorm = $target.Replace('\', '/').TrimEnd('/')
    $rootNorm = $root.Replace('\', '/').TrimEnd('/')
    if ($targetNorm.StartsWith($rootNorm + '/', [System.StringComparison]::OrdinalIgnoreCase)) {
        return $targetNorm.Substring($rootNorm.Length + 1)
    }
    return $targetNorm
}

$excludes = @("Agent\BootNotifier.cs","Agent\TimeSyncScheduler.cs","Agent\LogBackupScheduler.cs","Agent\NetworkMonitor.cs","Agent\ScreenshotScheduler.cs","Agent\AgentBootstrapper.cs","Agent\AgentTrayContext.cs","Agent\AutoAdminSetup.cs")

$excludeSet = @()
foreach ($ex in $excludes) {
    $exPath = if ([System.IO.Path]::IsPathRooted($ex)) { Resolve-RelativeToRoot $ex } else { Resolve-RelativeToRoot (Join-Path $projectDir $ex) }
    $excludeSet += $exPath
    Write-Host "Exclude: $ex -> $exPath"
}

$searchDir = Join-Path $projectDir "Agent"
$files = Get-ChildItem -Path $searchDir -Filter "*.*" -File
foreach ($f in $files) {
    $rel = Resolve-RelativeToRoot $f.FullName
    $isExcluded = $rel -in $excludeSet
    Write-Host "File: $rel -> Excluded: $isExcluded"
}
