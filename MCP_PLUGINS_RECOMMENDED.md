# MCP / Plugin Integration Guide for EJLive Codex

This file lists recommended plugins/connectors/MCP servers to strengthen analysis, implementation, design, and delivery.

## Recommended plugins / connectors

### 1. GitHub
Use for:
- repository browsing
- issues
- pull requests
- code review workflow
- branches and commits
- release notes

Recommended use:
- create implementation issues from gap analysis
- open PRs per small implementation wave
- attach risk/test notes to PRs

### 2. Figma / FigJam / Figma Slides
Use for:
- NOC dashboard redesign
- UI flows
- status cards
- topology/map view
- implementation-ready screen specs
- management presentations

Recommended EJLive use:
- Dashboard redesign
- Client settings screen layout
- Journal Sync timeline screen
- ATM detail drawer
- command approval workflow

### 3. Hugging Face
Use for:
- exploring optional local/AI models
- log classification experiments
- anomaly detection prototypes
- text extraction model research

Keep AI features optional and separated from core runtime.

### 4. Netlify
Use only for optional web dashboards, docs previews, or static product pages.
Do not make it a dependency of the core WinForms system.

### 5. Local filesystem / repository tools
Use for:
- project tree inspection
- source search
- generated reports
- build scripts
- local artifacts

### 6. Visual Studio / MSBuild / NuGet
Use locally for:
- .sln/.slnx build
- package restore
- project reference validation
- WinForms designer checks

## Plugin safety

Do not let plugins:
- modify secrets
- publish code without review
- change security settings without approval
- expose ATM logs or sensitive journal data
- run destructive commands automatically

## Suggested workflow

1. Use file/project analysis first.
2. Use GitHub for task/PR organization.
3. Use Figma for UI redesign.
4. Use local build tools for validation.
5. Use AI/model tools only for optional analysis modules.
