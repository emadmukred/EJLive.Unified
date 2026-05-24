#Requires -Version 5.1
param(
    [string]$OutputPath = ".\docs\09-file-function-inventory.csv",
    [string]$SolutionRoot = "."
)

$root = Resolve-Path $SolutionRoot
$files = Get-ChildItem -Path $root -Recurse -File | Where-Object {
    $path = $_.FullName.Replace('\', '/')
    -not ($path -like '*/bin/*' -or $path -like '*/obj/*' -or $path -like '*/.vs/*' -or $path -like '*/artifacts/*' -or $path -like '*/.codex/*' -or $path -like '*/.git/*')
}

function Get-LayerOrArea {
    param([string]$relativePath)
    $rel = $relativePath.Replace('\', '/')
    if ($rel.StartsWith('src/')) { return 'Source Code' }
    if ($rel.StartsWith('docs/')) { return 'Documentation' }
    if ($rel.StartsWith('commands/')) { return 'Task Commands' }
    if ($rel.StartsWith('.github/')) { return 'GitHub Workflow Metadata' }
    if ($rel.StartsWith('legacy/')) { return 'Reference' }
    if ($rel.StartsWith('tools/')) { return 'Tooling' }
    if ($rel.StartsWith('.codex/')) { return 'Workspace' }
    if ($rel -match '^[^/]+\.(md|txt|props|sln|slnx)$') { return 'Workspace' }
    return 'Workspace'
}

function Get-FunctionalRole {
    param([string]$relativePath)
    $rel = $relativePath.Replace('\', '/')
    $ext = [System.IO.Path]::GetExtension($rel).ToLowerInvariant()

    if ($rel.StartsWith('legacy/')) {
        return 'Preserved legacy source, archives, audit reports, and source evidence'
    }
    if ($rel.StartsWith('docs/')) {
        return 'Architecture, audit, checklist, or generated file inventory documentation'
    }
    if ($rel.StartsWith('tools/')) {
        return 'Build, verification, or development automation script'
    }
    if ($rel.StartsWith('commands/')) {
        return 'Codex task command definition'
    }
    if ($rel.StartsWith('.github/')) {
        return 'GitHub issue or pull request workflow metadata'
    }
    if ($rel.StartsWith('.codex/')) {
        return 'IDE plugin workspace, memory, or skill configuration'
    }
    if ($rel.StartsWith('src/')) {
        $folder = if ($rel -match '^src/([^/]+)/') { $Matches[1] } else { '' }
        switch ($folder) {
            'EJLive.Application' { return 'Application layer host, readiness, and workflow orchestration' }
            'EJLive.Business' { return 'Business runtime, operational fusion, and service gateway' }
            'EJLive.Client.Service' { return 'Client Windows Service headless agent host' }
            'EJLive.Client.WinForms' { return 'Client interactive WinForms presentation and agent UI' }
            'EJLive.Core' { return 'Core engines, protocol, models, data, and XFS adapters' }
            'EJLive.Installer.WinForms' { return 'Installer WinForms setup wizard and service registration' }
            'EJLive.LegacyReference' { return 'Legacy reference preservation project for non-compiled source' }
            'EJLive.Monitor' { return 'Legacy monitoring dashboard WinForms' }
            'EJLive.Monitoring.WinForms' { return 'Operational monitoring WinForms dashboard' }
            'EJLive.Server' { return 'Server-side services and analytics' }
            'EJLive.Server.WinForms' { return 'Server interactive WinForms presentation' }
            'EJLive.Shared' { return 'Shared utilities, logging, security helpers, and themes' }
            'EJLive.Tests' { return 'Unit and integration test fixtures' }
            'EJLive.Verification' { return 'Integration verification and audit probes' }
            default { return 'Source code, configuration, or asset file' }
        }
    }
    if ($ext -in @('.sln','.slnx')) { return 'Repository root artifact or preserved workspace file' }
    if ($ext -eq '.props') { return 'Repository root artifact or preserved workspace file' }
    if ($ext -eq '.md') { return 'Repository root artifact or preserved workspace file' }
    return 'Repository root artifact or preserved workspace file'
}

$lines = @('"Path","LayerOrArea","FunctionalRole"')
foreach ($file in $files) {
    $rel = $file.FullName.Substring($root.Path.Length + 1).Replace('\', '/')
    $layer = Get-LayerOrArea $rel
    $role = Get-FunctionalRole $rel
    $lines += "`"$rel`",`"$layer`",`"$role`""
}

$lines | Out-File -FilePath $OutputPath -Encoding UTF8
Write-Host "Regenerated inventory with $($lines.Count - 1) rows at $OutputPath"
