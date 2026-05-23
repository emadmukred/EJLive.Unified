$root = Get-Location
$allFiles = Get-ChildItem -Path $root -Recurse -File | Where-Object {
    $path = $_.FullName.Replace('\', '/')
    -not ($path -like '*/bin/*' -or $path -like '*/obj/*' -or $path -like '*/.vs/*' -or $path -like '*/artifacts/*')
} | ForEach-Object { $_.FullName.Substring($root.Path.Length + 1).Replace('\', '/') }

$inventory = Get-Content '.\docs\09-file-function-inventory.csv' | Select-Object -Skip 1 | ForEach-Object {
    ($_.Split(',')[0]).Trim('"')
}

$allSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
$allFiles | ForEach-Object { $allSet.Add($_) | Out-Null }

$invSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
$inventory | ForEach-Object { $invSet.Add($_) | Out-Null }

$inAllNotInv = $allSet | Where-Object { -not $invSet.Contains($_) }
$inInvNotAll = $invSet | Where-Object { -not $allSet.Contains($_) }

$codexInInv = $inventory | Where-Object { $_.StartsWith('.codex/') }

Write-Host "All files: $($allSet.Count)"
Write-Host "Inventory rows: $($invSet.Count)"
Write-Host ".codex/ in inventory: $($codexInInv.Count)"
Write-Host "In all but not inventory: $($inAllNotInv.Count)"
Write-Host "In inventory but not all: $($inInvNotAll.Count)"

Write-Host "--- In all but not inventory (first 20):"
$inAllNotInv | Select-Object -First 20 | ForEach-Object { Write-Host $_ }

Write-Host "--- In inventory but not all (first 20):"
$inInvNotAll | Select-Object -First 20 | ForEach-Object { Write-Host $_ }
