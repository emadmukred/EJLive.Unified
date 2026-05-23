$map = Import-Csv ".\artifacts\ActiveCompileMap.csv"
$abs = $map | Where-Object { $_.Path -like 'C:*' -or $_.Path -like '/*' }
Write-Host "Absolute path entries: $($abs.Count)"
$abs | Select-Object -First 20 | Format-Table -AutoSize
