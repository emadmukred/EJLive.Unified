#Requires -Version 5.1
param(
    [string]$ActiveCompileMapPath = ".\artifacts\ActiveCompileMap.csv",
    [string]$ServiceStatusPath = ".\docs\12-service-activation-status.csv",
    [string]$MismatchReportPath = ".\artifacts\CompileMapMismatchReport.csv",
    [string]$ReferencePromotionMapPath = ".\artifacts\ReferencePromotionMap.csv"
)

$ErrorActionPreference = "Stop"

$map = Import-Csv $ActiveCompileMapPath
$status = Import-Csv $ServiceStatusPath

# Build lookup
$mapLookup = @{}
foreach ($r in $map) { $mapLookup[$r.Path] = $r }

$mismatches = @()
$matched = 0
$promotions = @()

foreach ($s in $status) {
    $path = $s.Path
    $expectedState = if ($s.Status -eq 'ActiveCompiled') { 'ActiveCompiled' } else { 'ReferenceOnly' }
    $actual = $mapLookup[$path]
    if (-not $actual) {
        $mismatches += [PSCustomObject]@{ Path=$path; Expected=$expectedState; Actual='MISSING'; Project=$s.Project; Issue='Not found in ActiveCompileMap' }
    } elseif ($actual.CompileState -ne $expectedState) {
        $mismatches += [PSCustomObject]@{ Path=$path; Expected=$expectedState; Actual=$actual.CompileState; Project=$s.Project; Issue='State mismatch' }
    } else {
        $matched++
    }

    # Build ReferencePromotionMap for ReferenceCovered items
    if ($s.Status -eq 'ReferenceCovered') {
        $promotions += [PSCustomObject]@{
            Path = $path
            Project = $s.Project
            CurrentState = 'ReferenceOnly'
            TargetReplacement = $s.Replacement
            Detail = $s.Detail
        }
    }
}

Write-Host "Comparison complete: Matched $matched / $($status.Count)"
Write-Host "Mismatches: $($mismatches.Count)"

# Save mismatch report
$headers = '"Path","Project","ExpectedState","ActualState","Issue"'
$lines = @($headers)
foreach ($m in $mismatches) {
    $lines += '"{0}","{1}","{2}","{3}","{4}"' -f $m.Path, $m.Project, $m.Expected, $m.Actual, $m.Issue
}
$lines | Out-File -FilePath $MismatchReportPath -Encoding UTF8
Write-Host "Mismatch report saved to $MismatchReportPath"

# Save promotion map
$promoHeaders = '"Path","Project","CurrentState","TargetReplacement","Detail"'
$promoLines = @($promoHeaders)
foreach ($p in $promotions) {
    $promoLines += '"{0}","{1}","{2}","{3}","{4}"' -f $p.Path, $p.Project, $p.CurrentState, $p.TargetReplacement, $p.Detail
}
$promoLines | Out-File -FilePath $ReferencePromotionMapPath -Encoding UTF8
Write-Host "ReferencePromotionMap saved to $ReferencePromotionMapPath with $($promotions.Count) entries"

if ($mismatches.Count -gt 0) {
    exit 1
}
exit 0
