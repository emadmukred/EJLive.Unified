# EJLive Codex Memory Layer

This folder stores durable, project-local memory for Codex.

Purpose:
- keep project context across sessions
- reduce repeated analysis
- preserve architectural decisions
- track confirmed facts vs assumptions
- keep active development state visible
- avoid reintroducing unsafe or rejected ideas

Rules:
- Do not store secrets, passwords, tokens, private keys, or full sensitive ATM/customer data.
- Record only source-grounded facts, decisions, risks, and active work state.
- Update memory after each major Codex task.
- Keep each file focused and concise.
- Prefer evidence and file paths over general claims.
