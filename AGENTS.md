# Empress Project — Agent Router

## Global Invariants
- **Role Split (Rule 11)**: Primary agent acts strictly as Planner & Tech Lead; never edit `Assets/` or `Packages/` directly (exempt: Q&A, research, or explicit user override). Gate 1 (`TODO` → `DOING`) and Gate 2 (`REVIEW` → `DONE`) require explicit user consent.
- **Harness Immutability (Rule 8)**: Never edit `AGENTS.md`, `.agents/`, or `.agent-harness/` (except `tasks/` and `tasks-board.md`) unless instructed.
- **Stop on Ambiguity (Rule 9)**: If specs conflict or omit details, stop and ask; never guess.
- **Asset Integrity (Rules 1 & 6)**: Never manually edit `.meta` files or raw `.unity`/`.prefab` YAML.
- **Skip Human Docs**: Never read `.agent-harness/README.md` or `.agent-harness/diagrams/`.

## Progressive Disclosure Routing
| Role / Phase | Read ONLY |
| :--- | :--- |
| **Sub-agent (Executor)** | Assigned `TASK-XXX-*.md`, [executor-rules.md](.agent-harness/rules/executor-rules.md), [execute-task.md](.agent-harness/workflows/execute-task.md), and ONLY shards/skills in the task's `Required Context & Skills`. |
| **Primary — Q&A** | [project-topology.md](.agent-harness/architecture/project-topology.md) if layout context is needed. |
| **Primary — Planning** | [planner-rules.md](.agent-harness/rules/planner-rules.md), [plan-task.md](.agent-harness/workflows/plan-task.md), [tasks-board.md](.agent-harness/tasks-board.md), [task.template.md](.agent-harness/task.template.md), plus relevant `.agent-harness/architecture/*.md`. |
| **Primary — Closeout** | [reviewer-rules.md](.agent-harness/rules/reviewer-rules.md), [review-and-closeout.md](.agent-harness/workflows/review-and-closeout.md), [tasks-board.md](.agent-harness/tasks-board.md). |
| **Skills (`.agents/skills/`)** | Activate `unity-cli-recompile`, `unity-test-runner`, `unity-headless-scene-ops`, `unity-official-skills`, or `harness-bootstrap` only when needed. |
