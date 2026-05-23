# AGENTS.md Memory Appendix

Add this section to the project-level AGENTS.md.

## Codex Memory Layer

Use `.codex/memory` as the project-local memory layer.

Before major work, read:

1. `.codex/memory/PROJECT_MEMORY.md`
2. `.codex/memory/ACTIVE_CONTEXT.md`
3. `.codex/memory/ARCHITECTURE_MEMORY.md`
4. `.codex/memory/KNOWN_GAPS.md`
5. `.codex/memory/SECURITY_MEMORY.md`
6. `.codex/memory/SYNC_MEMORY.md` when task touches sync/journal transfer
7. `.codex/memory/UI_UX_MEMORY.md` when task touches UI/dashboard
8. `.codex/memory/DECISIONS.md` when task affects architecture

After major work, update the relevant memory file using `.codex/memory/MEMORY_UPDATE_PROTOCOL.md`.

Do not store secrets or sensitive ATM/customer data in memory files.
