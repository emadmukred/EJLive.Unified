$map = Import-Csv ".\artifacts\ActiveCompileMap.csv"
$files = @("src/EJLive.Shared/AppLogger.cs","src/EJLive.Shared/LightUiTheme.cs","src/EJLive.Shared/Logger.cs","src/EJLive.Shared/MonitoringState.cs","src/EJLive.Shared/MonitoringStateStore.cs")
foreach ($f in $files) {
    $entry = $map | Where-Object { $_.Path -eq $f }
    if ($entry) {
        Write-Host "$f -> $($entry.CompileState) in $($entry.Project)"
    } else {
        Write-Host "$f -> NOT FOUND"
    }
}
