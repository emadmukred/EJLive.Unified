$map = Import-Csv ".\artifacts\ActiveCompileMap.csv"
$entry = $map | Where-Object { $_.Path -eq "src/EJLive.Shared/AppLogger.cs" }
if ($entry) {
    Write-Host "Found: $($entry.CompileState) in $($entry.Project) via $($entry.IncludeSource)"
} else {
    Write-Host "NOT FOUND"
}
