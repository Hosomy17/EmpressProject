# Execution Sub-Agent Rules

Applies strictly to Execution Sub-agents executing an assigned `TASK-XXX-*.md`.

1. **Rule E1 — Progressive Context Discipline**: Read ONLY your assigned `TASK-XXX-*.md`, this file, [execute-task.md](../workflows/execute-task.md), and the exact architecture shards and skills listed in that task's `Required Context & Skills` section. Do not load `README.md`, `diagrams/`, `planner-rules.md`, `project-topology.md`, or unlisted shards.
2. **Rule E2 — Never Manually Hack `.meta` Files**: Never create, edit, or delete `.meta` files manually. Rely on Unity's `AssetDatabase` via the `unity-cli-recompile` skill to generate deterministic `.meta` files.
3. **Rule E3 — No Raw Structural YAML Edits**: Never use raw text search-and-replace on `.unity` or `.prefab` YAML for structural changes. Use an Editor batch method under `Assets/Editor/AgentAutomation/` via the `unity-headless-scene-ops` skill.
4. **Rule E4 — Prefer Official Unity Skills**: Check `Unity-Technologies/skills` (via `unity-official-skills`) before writing bespoke automation scripts when an official Unity skill covers the task.
5. **Rule E5 — Mandatory SoT Conformance Attestation**: Explicitly log how your implementation satisfies Assembly/Layer, Composition/Scene, Event Pattern, and Memory/GC/Async rules *before* running final verification tests.
6. **Rule E6 — Mandatory Pre-Completion Test Gate & Test Constraint**: Never mark a task `REVIEW` until Unity CLI compilation and all required tests pass with 0 failures. Never write tests that call a coroutine or `UniTask`.
7. **Rule E7 — Transparent Log Diagnostics**: Always capture and inspect `-logFile` and `Logs/*-Results.xml` output (`error CS`, `Exception:`, `Assertion failed`); never suppress or hide CLI errors.
8. **Rule E8 — Harness Immutability & Ambiguity Stop**: Never modify harness configuration files (except updating your task's status in `TASK-XXX-*.md` and `tasks-board.md`). If the task spec is ambiguous or blocked, stop immediately and report back to the Tech Lead.
