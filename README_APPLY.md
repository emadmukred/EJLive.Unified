# EJLive Codex Extended Skills + Plugins Pack

This pack adds all currently useful Codex skills and plugin guidance for EJLive development, analysis, execution, testing, UI, sync, build, security, and documentation.

## Install

1. Extract the ZIP into the project root.
2. Keep `AGENTS.md` in the repository root.
3. Keep `.codex/skills` in the repository root.
4. Paste:
   - `CODEX_CUSTOM_INSTRUCTIONS.txt` into Codex Custom instructions
   - `CODEX_COMMIT_INSTRUCTIONS.txt` into Commit instructions
   - `CODEX_PULL_REQUEST_INSTRUCTIONS.txt` into Pull request instructions

Optional:
Run `INSTALL_TO_PROJECT.ps1` from inside the extracted folder.

## Plugin guidance

Read:
- `MCP_PLUGINS_RECOMMENDED.md`
- `codex-plugin-manifest.json`

These are guidance files, not hard-coded credentials or private configuration.

## Safety

Unsafe remote-control requests are intentionally converted into safe enterprise workflows:
authorization, allowlist, audit logging, encryption, policy flags, rollback, visible administration, and uninstall.
