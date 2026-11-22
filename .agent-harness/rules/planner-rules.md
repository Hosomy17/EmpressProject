# Planning Agent Rules

Applies strictly to the Primary Agent during Planning (`TODO` creation and Gate 1).

1. **Rule P1 — Mandatory SoT Validation Gate**: Before specifying any code, `.asmdef`, asset, or scene change, validate the design against ONLY the relevant `.agent-harness/architecture/` shards:
   - [assembly-boundaries.md](../architecture/assembly-boundaries.md) (layer & `.asmdef` compliance)
   - [core-patterns.md](../architecture/core-patterns.md) (Composer → Component → Data + `EventBus`)
   - [coding-standards.md](../architecture/coding-standards.md) (Zero-GC loops, caching, pooling, UniTask, naming, test constraints)
   - [scene-hierarchy.md](../architecture/scene-hierarchy.md) (**only** when creating or modifying `.unity` scenes or `Prefabs/SceneSettings/` prefabs)
2. **Rule P2 — Upfront Path & Context Resolution**: Resolve all existing and new target file paths during planning. Populate the `Required Context & Skills` manifest in `TASK-XXX.md` so the Execution Sub-agent never searches the repo or loads unrelated shards.
3. **Rule P3 — Autonomous Task Contract**: Clone [task.template.md](../task.template.md) into `.agent-harness/tasks/TASK-XXX-<name>.md`. The spec must be self-contained (objective, manifest, exact file paths, constraints, verification steps, acceptance criteria) so an Execution Sub-agent needs zero external chat history.
4. **Rule P4 — Gate 1 User Consent**: Register the task on [tasks-board.md](../tasks-board.md) as `TODO`, present the planned spec to the user, and **pause**. Never transition to `DOING` or spawn an Execution Sub-agent without explicit user consent.
5. **Rule P5 — Stop on Ambiguity**: When requirements are ambiguous or admit conflicting designs, stop and present the trade-offs to the user before finalizing the spec or modifying any assets.
