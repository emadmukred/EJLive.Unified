param(
    [Parameter(Mandatory = $true)]
    [string]$SourceName,

    [string]$WorkspaceRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

$ErrorActionPreference = 'Stop'

function Test-BuildOutput {
    param([string]$Path)
    $normalized = $Path.Replace('\', '/')
    return $normalized.Contains('/bin/', [StringComparison]::OrdinalIgnoreCase) -or
        $normalized.Contains('/obj/', [StringComparison]::OrdinalIgnoreCase) -or
        $normalized.Contains('/.vs/', [StringComparison]::OrdinalIgnoreCase)
}

function Get-SafeName {
    param([string]$Name)
    $safeName = ($Name -replace '[^A-Za-z0-9_.-]+', '_')
    $safeName = ($safeName -replace '_{2,}', '_').TrimEnd('_')
    if (-not $Name.StartsWith('_', [StringComparison]::Ordinal)) {
        $safeName = $safeName.TrimStart('_')
    }
    if ([string]::IsNullOrWhiteSpace($safeName) -or $safeName -match '^[_.-]+$') {
        $hash = [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($Name))).Substring(0, 12)
        return "source_$hash"
    }
    return $safeName
}

function Read-TextFile {
    param([string]$Path)
    try {
        return Get-Content -LiteralPath $Path -Raw -ErrorAction Stop
    }
    catch {
        return $null
    }
}

function Get-FileCategory {
    param([System.IO.FileInfo]$File, [string]$Text)

    $name = $File.Name
    $extension = $File.Extension.ToLowerInvariant()
    if ($extension -eq '.cs') {
        if ($name -match '(Form|Control|Panel|Dashboard|Wizard)\.cs$' -or $Text -match ':\s*(Form|UserControl|Panel)\b') {
            return 'WinForms UI'
        }
        if ($File.FullName -match '(Service|Manager|Engine|Processor|Handler|Adapter|Factory|Parser|Store)' -or $name -match '(Service|Manager|Engine|Processor|Handler|Adapter|Factory|Parser|Store)\.cs$') {
            return 'Service/Manager/Engine'
        }
        if ($File.FullName -match '(Model|Models|Constants|Protocol|Security|Database|Data)') {
            return 'Data/Core Source'
        }
        return 'CSharp Source'
    }
    if ($extension -eq '.csproj') { return 'CSharp Project' }
    if ($extension -eq '.sln' -or $extension -eq '.slnx') { return 'Solution' }
    if ($extension -in '.config', '.json', '.xml', '.sql', '.db', '.sqlite') { return 'Configuration/Data' }
    if ($extension -in '.md', '.txt', '.csv', '.xlsx', '.xls') { return 'Documentation/Requirements' }
    if ($extension -in '.log') { return 'Runtime Log Evidence' }
    if ($extension -in '.png', '.jpg', '.jpeg', '.bmp', '.ico') { return 'Visual Asset' }
    if ($extension -in '.zip', '.7z', '.rar') { return 'Archive Package' }
    if ($extension -in '.ps1', '.bat', '.cmd') { return 'Build/Tool Script' }
    if ($extension -in '.dll', '.exe', '.pdb') { return 'Binary Artifact' }
    return 'Reference Artifact'
}

function Get-LayerCandidate {
    param([string]$RelativePath, [string]$Category)

    if ($Category -eq 'WinForms UI') { return 'Presentation Layer' }
    if ($Category -eq 'Service/Manager/Engine') { return 'Business Layer' }
    if ($Category -eq 'Data/Core Source' -or $RelativePath -match '(Core|Shared|Database|Models|Constants|Protocol|Security|Xfs)') { return 'Data/Core Layer' }
    if ($RelativePath -match '(Test|Tests|Verification)') { return 'Verification' }
    if ($Category -in 'CSharp Project', 'Solution') { return 'Project Structure' }
    return 'Reference/Future Development'
}

function Get-FunctionalRole {
    param([string]$RelativePath, [string]$Category)

    if ($Category -eq 'WinForms UI') { return 'Preserved visual surface, form, dashboard, panel, or control for UI consolidation.' }
    if ($Category -eq 'Service/Manager/Engine') { return 'Preserved operational behavior for staged service, manager, engine, adapter, parser, or handler promotion.' }
    if ($Category -eq 'Data/Core Source') { return 'Preserved core model, protocol, constant, data, or utility behavior.' }
    if ($Category -eq 'CSharp Source') { return 'Preserved C# implementation requiring scenario comparison before promotion.' }
    if ($Category -eq 'CSharp Project') { return 'Original project file used for dependency and assembly mapping.' }
    if ($Category -eq 'Solution') { return 'Original solution file used for project graph comparison.' }
    if ($Category -eq 'Configuration/Data') { return 'Original configuration or data artifact used to preserve runtime assumptions.' }
    if ($Category -eq 'Documentation/Requirements') { return 'Original documentation, requirements, analysis, or spreadsheet evidence.' }
    if ($Category -eq 'Runtime Log Evidence') { return 'Original ATM/vendor log evidence for parser and behavior tests.' }
    if ($Category -eq 'Build/Tool Script') { return 'Original build or maintenance script retained as reference.' }
    if ($Category -eq 'Archive Package') { return 'Nested or delivered package preserved for traceability.' }
    if ($Category -eq 'Binary Artifact') { return 'Original compiled output retained only as historical evidence.' }
    return 'Reference artifact retained for future analysis.'
}

function Get-ActiveHashes {
    param([string]$Root, [switch]$CompiledOnly)

    $compiledFolders = @(
        'EJLive.Application',
        'EJLive.Business',
        'EJLive.Client.WinForms',
        'EJLive.Core',
        'EJLive.Installer.WinForms',
        'EJLive.Monitor',
        'EJLive.Monitoring.WinForms',
        'EJLive.Server.WinForms',
        'EJLive.Shared'
    )
    $hashes = @{}
    Get-ChildItem -LiteralPath $Root -Recurse -File -Filter *.cs |
        Where-Object { -not (Test-BuildOutput $_.FullName) } |
        Where-Object {
            if (-not $CompiledOnly) { return $true }
            $relative = [System.IO.Path]::GetRelativePath($Root, $_.FullName).Replace('\', '/')
            $folder = $relative.Split('/')[0]
            return $compiledFolders -contains $folder
        } |
        ForEach-Object {
            $hash = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
            $hashes[$hash] = $true
        }
    return $hashes
}

$sourceRoot = Join-Path $WorkspaceRoot "legacy\original\$SourceName"
if (-not (Test-Path -LiteralPath $sourceRoot)) {
    throw "Source root not found: $sourceRoot"
}

$docsRoot = Join-Path $WorkspaceRoot 'docs\original-audit'
New-Item -ItemType Directory -Path $docsRoot -Force | Out-Null

$activeRoot = Join-Path $WorkspaceRoot 'src'
$workspaceHashes = Get-ActiveHashes -Root $activeRoot
$compiledHashes = Get-ActiveHashes -Root $activeRoot -CompiledOnly
$activeNames = @{}
Get-ChildItem -LiteralPath $activeRoot -Recurse -File |
    Where-Object { -not (Test-BuildOutput $_.FullName) } |
    ForEach-Object { $activeNames[$_.Name] = $true }

$textExtensions = @('.cs', '.csproj', '.sln', '.slnx', '.config', '.json', '.xml', '.md', '.txt', '.csv', '.sql', '.resx')
$files = Get-ChildItem -LiteralPath $sourceRoot -Recurse -File |
    Where-Object { -not (Test-BuildOutput $_.FullName) } |
    Sort-Object FullName

$manifest = foreach ($file in $files) {
    $relative = [System.IO.Path]::GetRelativePath($sourceRoot, $file.FullName).Replace('\', '/')
    $extension = $file.Extension.ToLowerInvariant()
    $text = if ($textExtensions -contains $extension) { Read-TextFile -Path $file.FullName } else { $null }
    $textForParsing = if ($null -eq $text) { '' } else { $text }
    $category = Get-FileCategory -File $file -Text $textForParsing
    $layer = Get-LayerCandidate -RelativePath $relative -Category $category
    $role = Get-FunctionalRole -RelativePath $relative -Category $category
    $hash = if ($extension -eq '.cs') { (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash } else { '' }

    $types = @()
    $namespaces = @()
    $methodCount = 0
    $usingCount = 0
    if ($extension -eq '.cs' -and $text) {
        $types = [regex]::Matches($text, '\b(class|interface|enum|record|struct)\s+([A-Za-z_][A-Za-z0-9_]*)') | ForEach-Object { $_.Groups[2].Value } | Select-Object -Unique
        $namespaces = [regex]::Matches($text, '\bnamespace\s+([A-Za-z_][A-Za-z0-9_.]*)') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique
        $methodCount = ([regex]::Matches($text, '\b(public|private|protected|internal)\s+(static\s+)?[A-Za-z0-9_<>,\[\]\.?]+\s+[A-Za-z_][A-Za-z0-9_]*\s*\(')).Count
        $usingCount = ([regex]::Matches($text, '^\s*using\s+', [System.Text.RegularExpressions.RegexOptions]::Multiline)).Count
    }

    [pscustomobject]@{
        SourceRoot = $SourceName
        RelativePath = $relative
        FileName = $file.Name
        Extension = $extension
        SizeBytes = $file.Length
        Category = $category
        LayerCandidate = $layer
        FunctionalRole = $role
        ContainsArabicText = [bool]($text -and $text -match '[\u0600-\u06FF]')
        MissingFromActiveByName = -not $activeNames.ContainsKey($file.Name)
        SameHashAsActiveWorkspace = [bool]($hash -and $workspaceHashes.ContainsKey($hash))
        SameHashAsActiveCompiled = [bool]($hash -and $compiledHashes.ContainsKey($hash))
        NamespaceCount = $namespaces.Count
        Namespaces = ($namespaces -join ';')
        TypeCount = $types.Count
        Types = ($types -join ';')
        MethodLikeMemberCount = $methodCount
        UsingCount = $usingCount
    }
}

$projectDependencies = foreach ($project in ($files | Where-Object Extension -eq '.csproj')) {
    $relative = [System.IO.Path]::GetRelativePath($sourceRoot, $project.FullName).Replace('\', '/')
    $xml = Read-TextFile -Path $project.FullName
    if (-not $xml) { continue }
    $packages = [regex]::Matches($xml, '<PackageReference\s+Include="([^"]+)"(?:\s+Version="([^"]+)")?') | ForEach-Object {
        [pscustomobject]@{
            SourceRoot = $SourceName
            Project = $relative
            DependencyType = 'PackageReference'
            Include = $_.Groups[1].Value
            Version = $_.Groups[2].Value
        }
    }
    $references = [regex]::Matches($xml, '<ProjectReference\s+Include="([^"]+)"') | ForEach-Object {
        [pscustomobject]@{
            SourceRoot = $SourceName
            Project = $relative
            DependencyType = 'ProjectReference'
            Include = $_.Groups[1].Value
            Version = ''
        }
    }
    $assemblyReferences = [regex]::Matches($xml, '<Reference\s+Include="([^"]+)"') | ForEach-Object {
        [pscustomobject]@{
            SourceRoot = $SourceName
            Project = $relative
            DependencyType = 'AssemblyReference'
            Include = $_.Groups[1].Value
            Version = ''
        }
    }
    $packages
    $references
    $assemblyReferences
}

$safeName = Get-SafeName -Name $SourceName
$manifestPath = Join-Path $docsRoot "$safeName-file-manifest.csv"
$dependencyPath = Join-Path $docsRoot "$safeName-project-dependencies.csv"
$summaryPath = Join-Path $docsRoot "$safeName-summary.md"

$manifest | Export-Csv -LiteralPath $manifestPath -NoTypeInformation -Encoding UTF8
$projectDependencies | Export-Csv -LiteralPath $dependencyPath -NoTypeInformation -Encoding UTF8

$groupedByCategory = $manifest | Group-Object Category | Sort-Object Name
$groupedByLayer = $manifest | Group-Object LayerCandidate | Sort-Object Name
$topFolders = $files |
    ForEach-Object {
        $relative = [System.IO.Path]::GetRelativePath($sourceRoot, $_.FullName).Replace('\', '/')
        $folder = if ($relative.Contains('/')) { $relative.Split('/')[0] } else { '(root)' }
        $folder
    } |
    Group-Object |
    Sort-Object Count -Descending |
    Select-Object -First 20

$uiFiles = $manifest | Where-Object Category -eq 'WinForms UI'
$serviceFiles = $manifest | Where-Object Category -eq 'Service/Manager/Engine'
$missingFiles = $manifest | Where-Object MissingFromActiveByName
$differentCompiled = $manifest | Where-Object { $_.Extension -eq '.cs' -and -not $_.SameHashAsActiveCompiled }

$summary = New-Object System.Collections.Generic.List[string]
$summary.Add("# $SourceName Deep Audit")
$summary.Add('')
$summary.Add("Generated from ``legacy/original/$SourceName``. Build output folders are excluded.")
$summary.Add('')
$summary.Add('## Counts')
$summary.Add('')
$summary.Add('| Metric | Value |')
$summary.Add('| --- | ---: |')
$summary.Add("| Files | $($manifest.Count) |")
$summary.Add("| C# files | $(($manifest | Where-Object Extension -eq '.cs').Count) |")
$summary.Add("| Projects | $(($manifest | Where-Object Category -eq 'CSharp Project').Count) |")
$summary.Add("| Solutions | $(($manifest | Where-Object Category -eq 'Solution').Count) |")
$summary.Add("| Forms/UI files | $($uiFiles.Count) |")
$summary.Add("| Service/manager/engine files | $($serviceFiles.Count) |")
$summary.Add("| Files missing from active by name | $($missingFiles.Count) |")
$summary.Add("| C# files different from active compiled source | $($differentCompiled.Count) |")
$summary.Add("| Files containing Arabic text | $(($manifest | Where-Object ContainsArabicText).Count) |")
$summary.Add("| Declared type count | $(($manifest | Measure-Object TypeCount -Sum).Sum) |")
$summary.Add("| Method-like member count | $(($manifest | Measure-Object MethodLikeMemberCount -Sum).Sum) |")
$summary.Add('')
$summary.Add('## Layer Candidates')
$summary.Add('')
$summary.Add('| Layer | Files |')
$summary.Add('| --- | ---: |')
foreach ($group in $groupedByLayer) {
    $summary.Add("| $($group.Name) | $($group.Count) |")
}
$summary.Add('')
$summary.Add('## Categories')
$summary.Add('')
$summary.Add('| Category | Files |')
$summary.Add('| --- | ---: |')
foreach ($group in $groupedByCategory) {
    $summary.Add("| $($group.Name) | $($group.Count) |")
}
$summary.Add('')
$summary.Add('## Top Folders')
$summary.Add('')
$summary.Add('| Folder | Files |')
$summary.Add('| --- | ---: |')
foreach ($folder in $topFolders) {
    $summary.Add("| $($folder.Name) | $($folder.Count) |")
}
$summary.Add('')
$summary.Add('## UI Surfaces')
$summary.Add('')
if ($uiFiles.Count -eq 0) {
    $summary.Add('No WinForms UI source was detected.')
}
else {
    foreach ($row in $uiFiles | Select-Object -First 80) {
        $summary.Add("- ``$($row.RelativePath)``")
    }
    if ($uiFiles.Count -gt 80) {
        $summary.Add("- Additional UI rows are listed in the CSV manifest.")
    }
}
$summary.Add('')
$summary.Add('## Services, Managers, Engines, Processors')
$summary.Add('')
if ($serviceFiles.Count -eq 0) {
    $summary.Add('No service, manager, engine, processor, handler, adapter, parser, or store source was detected.')
}
else {
    foreach ($row in $serviceFiles | Select-Object -First 120) {
        $summary.Add("- ``$($row.RelativePath)``")
    }
    if ($serviceFiles.Count -gt 120) {
        $summary.Add("- Additional service rows are listed in the CSV manifest.")
    }
}
$summary.Add('')
$summary.Add('## Dependency Records')
$summary.Add('')
$summary.Add("Dependency rows written: $($projectDependencies.Count). See ``docs/original-audit/$safeName-project-dependencies.csv``.")
$summary.Add('')
$summary.Add('## File Manifest')
$summary.Add('')
$summary.Add("Full file-level role map written to ``docs/original-audit/$safeName-file-manifest.csv``.")

Set-Content -LiteralPath $summaryPath -Value $summary -Encoding UTF8

[pscustomobject]@{
    SourceRoot = $SourceName
    Files = $manifest.Count
    CSharpFiles = ($manifest | Where-Object Extension -eq '.cs').Count
    Forms = $uiFiles.Count
    ServiceEngineManager = $serviceFiles.Count
    MissingFromActiveByName = $missingFiles.Count
    DifferentFromActiveCompiledCSharp = $differentCompiled.Count
    ArabicTextFiles = ($manifest | Where-Object ContainsArabicText).Count
    Manifest = $manifestPath
    Summary = $summaryPath
}
