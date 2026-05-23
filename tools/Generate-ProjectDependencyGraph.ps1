#Requires -Version 5.1
param(
    [string]$SolutionRoot = ".",
    [string]$OutputPath = ".\artifacts\ProjectDependencyGraph.md"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Xml
$root = (Resolve-Path $SolutionRoot).Path

$csprojFiles = Get-ChildItem -Path (Join-Path $root 'src') -Filter '*.csproj' -Recurse

$projects = @{}
$edges = @()

foreach ($csproj in $csprojFiles) {
    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($csproj.Name)
    $projectDir = [System.IO.Path]::GetDirectoryName($csproj.FullName)
    $xml = [xml](Get-Content $csproj.FullName -Raw)
    $ns = $xml.DocumentElement.NamespaceURI
    $nsMgr = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
    if ($ns) { $nsMgr.AddNamespace('ns', $ns) }

    $projects[$projectName] = @{ Path = $csproj.FullName.Substring($root.Length + 1).Replace('\', '/'); References = @() }

    $refNodes = if ($ns) { $xml.SelectNodes("//ns:ProjectReference", $nsMgr) } else { $xml.SelectNodes("//ProjectReference") }
    foreach ($node in $refNodes) {
        $refPath = $node.GetAttribute('Include')
        $refName = [System.IO.Path]::GetFileNameWithoutExtension($refPath)
        $projects[$projectName].References += $refName
        $edges += @{ From = $projectName; To = $refName }
    }
}

$lines = @()
$lines += '# Project Dependency Graph'
$lines += ''
$lines += "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
$lines += ''
$lines += '## Projects'
$lines += ''
foreach ($name in ($projects.Keys | Sort-Object)) {
    $p = $projects[$name]
    $lines += "- **$name**: ``$($p.Path)``"
}
$lines += ''
$lines += '## Dependencies'
$lines += ''
$lines += '```'
foreach ($name in ($projects.Keys | Sort-Object)) {
    $refs = $projects[$name].References | Sort-Object
    if ($refs.Count -gt 0) {
        $lines += ('{0} - {1} {2}' -f $name, '>', ($refs -join ', '))
    } else {
        $lines += ('{0} - {1} (none)' -f $name, '>')
    }
}
$lines += '```'
$lines += ''
$lines += '## Mermaid Diagram'
$lines += ''
$lines += '```mermaid'
$lines += 'graph TD'
$seen = @{}
foreach ($e in $edges) {
    $key = "$($e.From)->$($e.To)"
    if (-not $seen.ContainsKey($key)) {
        $seen[$key] = $true
        $lines += "    $($e.From) --> $($e.To)"
    }
}
$lines += '```'
$lines += ''

$lines | Out-File -FilePath $OutputPath -Encoding UTF8
Write-Host 'ProjectDependencyGraph generated at' $OutputPath
