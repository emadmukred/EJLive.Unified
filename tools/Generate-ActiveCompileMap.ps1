#Requires -Version 5.1
<#
.SYNOPSIS
    Generates ActiveCompileMap.csv from all .csproj files in the solution.
    Handles explicit items, wildcards, Exclude attributes, and default SDK globs.
    When a file appears in multiple projects, priority is: ActiveCompiled > Deprecated > ReferenceOnly.
#>
param(
    [string]$SolutionRoot = ".",
    [string]$OutputPath = ".\artifacts\ActiveCompileMap.csv"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Xml

$root = (Resolve-Path $SolutionRoot).Path.TrimEnd('\', '/')

function Resolve-RelativeToRoot {
    param([string]$TargetPath)
    $target = (Resolve-Path $TargetPath -ErrorAction SilentlyContinue)
    if ($target) {
        $target = $target.Path
    } else {
        try {
            $target = [System.IO.Path]::GetFullPath($TargetPath)
        }
        catch {
            return $TargetPath.Replace('\', '/')
        }
    }
    $targetNorm = $target.Replace('\', '/').TrimEnd('/')
    $rootNorm = $root.Replace('\', '/').TrimEnd('/')
    if ($targetNorm.StartsWith($rootNorm + '/', [System.StringComparison]::OrdinalIgnoreCase)) {
        return $targetNorm.Substring($rootNorm.Length + 1)
    }
    return $targetNorm
}

function Test-BuildOutputRelative {
    param([string]$Path)
    $normalized = $Path.Replace('\', '/')
    return $normalized -match '(^|/)(bin|obj)/'
}

function Expand-Wildcard {
    param(
        [string]$ProjectDir,
        [string]$Pattern,
        [string[]]$Excludes = @()
    )
    $results = @()
    if ($Pattern -match '\*\*') {
        $normalized = $Pattern -replace '\\\*\*\\', '\' -replace '\*\*\\', '' -replace '\\\*\*', ''
        $baseDir = [System.IO.Path]::GetDirectoryName($normalized)
        $filePattern = [System.IO.Path]::GetFileName($normalized)
        if (-not $filePattern) { $filePattern = '*.*' }
        $searchDir = if ([System.IO.Path]::IsPathRooted($baseDir)) { $baseDir } else { Join-Path $ProjectDir $baseDir }
        if (Test-Path $searchDir) {
            $files = Get-ChildItem -Path $searchDir -Filter $filePattern -Recurse -File -ErrorAction SilentlyContinue
            foreach ($f in $files) {
                $rel = Resolve-RelativeToRoot $f.FullName
                if (-not (Test-BuildOutputRelative $rel)) {
                    $results += $rel
                }
            }
        }
    } else {
        $baseDir = [System.IO.Path]::GetDirectoryName($Pattern)
        $filePattern = [System.IO.Path]::GetFileName($Pattern)
        $searchDir = if ([System.IO.Path]::IsPathRooted($baseDir)) { $baseDir } else { Join-Path $ProjectDir $baseDir }
        if (Test-Path $searchDir) {
            $files = Get-ChildItem -Path $searchDir -Filter $filePattern -File -ErrorAction SilentlyContinue
            foreach ($f in $files) {
                $rel = Resolve-RelativeToRoot $f.FullName
                if (-not (Test-BuildOutputRelative $rel)) {
                    $results += $rel
                }
            }
        }
    }
    if ($Excludes.Count -gt 0) {
        $excludeSet = @()
        foreach ($ex in $Excludes) {
            $exPath = if ([System.IO.Path]::IsPathRooted($ex)) { Resolve-RelativeToRoot $ex } else { Resolve-RelativeToRoot (Join-Path $ProjectDir $ex) }
            $excludeSet += $exPath
            if ($ex -match '\*') {
                $exBase = [System.IO.Path]::GetDirectoryName($ex)
                $exPat = [System.IO.Path]::GetFileName($ex)
                $exDir = if ([System.IO.Path]::IsPathRooted($exBase)) { $exBase } else { Join-Path $ProjectDir $exBase }
                if (Test-Path $exDir) {
                    $exFiles = Get-ChildItem -Path $exDir -Filter $exPat -Recurse -File -ErrorAction SilentlyContinue
                    foreach ($ef in $exFiles) {
                        $excludeSet += Resolve-RelativeToRoot $ef.FullName
                    }
                }
            }
        }
        $results = $results | Where-Object { $_ -notin $excludeSet }
    }
    return $results
}

function Get-ProjectCompileMap {
    param([string]$CsprojPath)

    $projectDir = [System.IO.Path]::GetDirectoryName($CsprojPath)
    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($CsprojPath)
    $xml = [xml](Get-Content $CsprojPath -Raw)
    $ns = $xml.DocumentElement.NamespaceURI
    $nsMgr = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
    if ($ns) { $nsMgr.AddNamespace('ns', $ns) }

    $results = @{}

    function Get-Attr($node, $name) {
        $a = $node.Attributes[$name]
        if ($a) { return $a.Value }
        return $null
    }

    # Check if default items are disabled
    $enableDefaultItems = $true
    $propNode = if ($ns) { $xml.SelectSingleNode("//ns:EnableDefaultItems[text()='false']", $nsMgr) } else { $xml.SelectSingleNode("//EnableDefaultItems[text()='false']") }
    if ($propNode) { $enableDefaultItems = $false }

    # Process Compile Include (with Exclude support)
    $compileNodes = if ($ns) { $xml.SelectNodes("//ns:Compile[@Include]", $nsMgr) } else { $xml.SelectNodes("//Compile[@Include]") }
    foreach ($node in $compileNodes) {
        $include = Get-Attr $node 'Include'
        $exclude = Get-Attr $node 'Exclude'
        $link = Get-Attr $node 'Link'
        $excludes = if ($exclude) { $exclude.Split(';') | ForEach-Object { $_.Trim() } | Where-Object { $_ } } else { @() }

        if ($include.Contains('*')) {
            $matched = Expand-Wildcard -ProjectDir $projectDir -Pattern $include -Excludes $excludes
            foreach ($relPath in $matched) {
                $results[$relPath] = @{ Project = $projectName; CompileState = 'ActiveCompiled'; IncludeSource = 'Compile Include (wildcard)'; Reason = 'Compiled via wildcard'; Link = $link }
            }
        } else {
            foreach ($single in $include.Split(';')) {
                $trimmed = $single.Trim()
            if (-not $trimmed) { continue }
            $fullPath = if ([System.IO.Path]::IsPathRooted($trimmed)) { $trimmed } else { Join-Path $projectDir $trimmed }
            $relPath = Resolve-RelativeToRoot $fullPath
            if (Test-BuildOutputRelative $relPath) { continue }
            $results[$relPath] = @{ Project = $projectName; CompileState = 'ActiveCompiled'; IncludeSource = 'Compile Include'; Reason = 'Explicitly compiled'; Link = $link }
        }
        }
    }

    # Process Compile Remove
    $removeNodes = if ($ns) { $xml.SelectNodes("//ns:Compile[@Remove]", $nsMgr) } else { $xml.SelectNodes("//Compile[@Remove]") }
    foreach ($node in $removeNodes) {
        $remove = Get-Attr $node 'Remove'
        foreach ($single in $remove.Split(';')) {
            $trimmed = $single.Trim()
            if (-not $trimmed) { continue }
            $fullPath = if ([System.IO.Path]::IsPathRooted($trimmed)) { $trimmed } else { Join-Path $projectDir $trimmed }
            $relPath = Resolve-RelativeToRoot $fullPath
            if (Test-BuildOutputRelative $relPath) { continue }
            $results[$relPath] = @{ Project = $projectName; CompileState = 'Deprecated'; IncludeSource = 'Compile Remove'; Reason = 'Explicitly removed from compilation'; Link = '' }
        }
    }

    # Process None Include with Link
    $noneNodes = if ($ns) { $xml.SelectNodes("//ns:None[@Include]", $nsMgr) } else { $xml.SelectNodes("//None[@Include]") }
    foreach ($node in $noneNodes) {
        $include = Get-Attr $node 'Include'
        $link = Get-Attr $node 'Link'
        if (-not $link) { continue }
        $exclude = Get-Attr $node 'Exclude'
        $excludes = if ($exclude) { $exclude.Split(';') | ForEach-Object { $_.Trim() } | Where-Object { $_ } } else { @() }

        if ($include.Contains('*')) {
            $matched = Expand-Wildcard -ProjectDir $projectDir -Pattern $include -Excludes $excludes
            foreach ($relPath in $matched) {
                if (-not $results.ContainsKey($relPath)) {
                    $results[$relPath] = @{ Project = $projectName; CompileState = 'ReferenceOnly'; IncludeSource = 'None Include + Link'; Reason = 'Preserved as reference only via Link'; Link = $link }
                }
            }
        } else {
            foreach ($single in $include.Split(';')) {
                $trimmed = $single.Trim()
                if (-not $trimmed) { continue }
                $fullPath = if ([System.IO.Path]::IsPathRooted($trimmed)) { $trimmed } else { Join-Path $projectDir $trimmed }
                $relPath = Resolve-RelativeToRoot $fullPath
                if (Test-BuildOutputRelative $relPath) { continue }
                if (-not $results.ContainsKey($relPath)) {
                    $results[$relPath] = @{ Project = $projectName; CompileState = 'ReferenceOnly'; IncludeSource = 'None Include + Link'; Reason = 'Preserved as reference only via Link'; Link = $link }
                }
            }
        }
    }

    # Handle default SDK globs for .cs files when EnableDefaultItems is true
    if ($enableDefaultItems) {
        $csFiles = Get-ChildItem -Path $projectDir -Filter '*.cs' -Recurse -File -ErrorAction SilentlyContinue
        foreach ($f in $csFiles) {
            $relPath = Resolve-RelativeToRoot $f.FullName
            if (Test-BuildOutputRelative $relPath) { continue }
            if (-not $results.ContainsKey($relPath)) {
                $results[$relPath] = @{ Project = $projectName; CompileState = 'ActiveCompiled'; IncludeSource = 'Default SDK glob'; Reason = 'Compiled by default SDK glob (EnableDefaultItems=true)'; Link = '' }
            }
        }
    }

    return $results
}

$csprojFiles = Get-ChildItem -Path (Join-Path $root 'src') -Filter '*.csproj' -Recurse

$allResults = @{}
foreach ($csproj in $csprojFiles) {
    Write-Host "Processing $($csproj.Name)..."
    $map = Get-ProjectCompileMap $csproj.FullName
    foreach ($entry in $map.GetEnumerator()) {
        $key = $entry.Key
        $val = $entry.Value
        if ($allResults.ContainsKey($key)) {
            $existing = $allResults[$key]
            $priority = @{ 'ActiveCompiled' = 3; 'Deprecated' = 2; 'ReferenceOnly' = 1 }
            if ($priority[$val.CompileState] -gt $priority[$existing.CompileState]) {
                $allResults[$key] = $val
            }
        } else {
            $allResults[$key] = $val
        }
    }
}

# Output CSV
$lines = @('"Project","FilePath","CompileState","Reason","Path","IncludeSource"')
foreach ($entry in ($allResults.GetEnumerator() | Sort-Object Key)) {
    $v = $entry.Value
    $lines += "`"$($v.Project)`",`"$($entry.Key)`",`"$($v.CompileState)`",`"$($v.Reason)`",`"$($entry.Key)`",`"$($v.IncludeSource)`""
}

$dir = [System.IO.Path]::GetDirectoryName($OutputPath)
if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
$lines | Out-File -FilePath $OutputPath -Encoding UTF8
Write-Host "ActiveCompileMap generated with $($lines.Count - 1) entries at $OutputPath"
