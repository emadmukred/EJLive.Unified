# Memory Update Protocol

Codex should update memory only when a fact is stable and useful for future work.

## Update memory after

- architecture decisions
- build graph discoveries
- recurring mistakes
- confirmed project structure changes
- implemented services
- accepted security policy
- sync protocol decisions
- UI design rules
- test/build outcomes
- rejected unsafe approaches

## Do not store

- passwords
- tokens
- API keys
- certificates/private keys
- full card/customer/account data
- sensitive ATM logs
- short-lived temporary notes
- guesses without evidence

## How to update

1. Put project facts in PROJECT_MEMORY.md.
2. Put current work state in ACTIVE_CONTEXT.md.
3. Put decisions in DECISIONS.md.
4. Put gaps in KNOWN_GAPS.md.
5. Put mistakes/corrections in LESSONS_LEARNED.md.
6. Put security constraints in SECURITY_MEMORY.md.
7. Put sync facts in SYNC_MEMORY.md.
8. Keep AGENTS.md concise and route to these memory files.
