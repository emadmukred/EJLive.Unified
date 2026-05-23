---
name: client-outbox-queue-engineer
description: Design durable client outbox queues, delivery confirmation, retry, backoff, and idempotent resend.
---

# client-outbox-queue-engineer

## When to use

Use this skill when the task directly matches its description and requires source-grounded analysis, implementation planning, or safe C#/.NET development inside the EJLive project.

## Required workflow

1. Inspect the active project structure first.
2. Identify relevant active source files, project files, reference documents, and legacy/original candidates.
3. Distinguish active compiled code from preserved reference code.
4. Avoid broad implementation before mapping dependencies and risks.
5. Produce a small, safe implementation slice when code changes are needed.
6. Report build/test status honestly.

## EJLive rules

- Preserve current project structure unless a change is justified.
- Do not add broken legacy files directly to compile graph.
- Convert reference functionality into active services, adapters, facades, or documented backlog items.
- Respect active contracts first.
- Maintain dual compatibility where possible.
- Do not delete without preserving function and explaining why.

## Security rules

- Do not implement stealth, zero-footprint cleanup, security-tool disabling, log wiping, hidden arbitrary shell execution, or unauthorized no-consent remote control.
- For Windows services, remote operations, screenshots, password workflows, RDP, registry, firewall, or command execution, require:
  - authorization
  - allowlist
  - audit logging
  - policy flags
  - encryption/authentication where applicable
  - rollback/uninstall path

## Output format

Return:

- Findings
- Evidence
- Files involved
- Proposed changes
- Risks
- Tests/build validation
- Rollback plan
- Next step
