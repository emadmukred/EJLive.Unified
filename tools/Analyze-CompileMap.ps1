$map = Import-Csv ".\artifacts\ActiveCompileMap.csv"
$cs = $map | Where-Object { $_.Path -like '*.cs' }
Write-Host "Total entries: $($map.Count)"
Write-Host ".cs entries: $($cs.Count)"
$cs | Group-Object CompileState | Select-Object Name, Count | Format-Table -AutoSize

Write-Host "`nSample Non-.cs entries:"
$map | Where-Object { $_.Path -notlike '*.cs' } | Select-Object -First 10 | Format-Table -AutoSize
