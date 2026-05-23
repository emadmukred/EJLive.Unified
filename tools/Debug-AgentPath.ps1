$root = (Resolve-Path ".").Path.TrimEnd('\', '/')
$projectDir = Resolve-Path "src\EJLive.Client.WinForms"

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

# Explicit compile path
$fullPath1 = Join-Path $projectDir "Agent\AgentBootstrapper.cs"
$relPath1 = Resolve-RelativeToRoot $fullPath1
Write-Host "Explicit: $relPath1"

# Wildcard path
$f = Get-ChildItem -Path (Join-Path $projectDir "Agent") -Filter "*.*" -File | Where-Object { $_.Name -eq 'AgentBootstrapper.cs' }
$relPath2 = Resolve-RelativeToRoot $f.FullName
Write-Host "Wildcard: $relPath2"
Write-Host "Match: $($relPath1 -eq $relPath2)"

# Check if in ActiveCompileMap
$map = Import-Csv ".\artifacts\ActiveCompileMap.csv"
$entry = $map | Where-Object { $_.Path -eq $relPath1 }
if ($entry) {
    Write-Host "Map entry found: $($entry.CompileState) from $($entry.IncludeSource)"
} else {
    Write-Host "Map entry NOT found"
}
