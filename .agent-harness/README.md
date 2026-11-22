# Empress Project — AI Agent Harness (Human Overview)

> **Human Onboarding & Overview** — AI agents start at [AGENTS.md](../AGENTS.md) and **must not** load this file or `.agent-harness/diagrams/`.

The Empress agent harness enforces **progressive disclosure** across Antigravity, Claude Code, GitHub Copilot, Cursor, and Cline so agents load only the rules, architecture shards, workflows, and skills strictly needed for their active step.

---

## Directory Index

| Path | Audience | Purpose |
| :--- | :--- | :--- |
| [AGENTS.md](../AGENTS.md) | AI (Always-On) | Thin root router & immutable global invariants |
| [architecture/](./architecture/) | AI (Selective) | Domain shards: [core-patterns.md](./architecture/core-patterns.md), [assembly-boundaries.md](./architecture/assembly-boundaries.md), [coding-standards.md](./architecture/coding-standards.md), [scene-hierarchy.md](./architecture/scene-hierarchy.md), [scene-audit-backlog.md](./architecture/scene-audit-backlog.md), [project-topology.md](./architecture/project-topology.md) |
| [rules/](./rules/) | AI (Role-Scoped) | Governance by phase: [planner-rules.md](./rules/planner-rules.md), [executor-rules.md](./rules/executor-rules.md), [reviewer-rules.md](./rules/reviewer-rules.md) |
| [workflows/](./workflows/) | AI (Phase-Scoped) | Step-by-step lifecycle: [plan-task.md](./workflows/plan-task.md), [execute-task.md](./workflows/execute-task.md), [review-and-closeout.md](./workflows/review-and-closeout.md) |
| [tasks-board.md](./tasks-board.md) | AI + Human | Live `TODO` / `DOING` / `REVIEW` register |
| [task.template.md](./task.template.md) | AI Planner | Blueprint cloned to `tasks/TASK-XXX-<name>.md` (includes `Required Context & Skills`) |
| [diagrams/](./diagrams/) | Humans Only | Mermaid visual diagrams linked from docs (ignored by AI) |
| [scripts/](./scripts/) | Shared CLI | [bootstrap-harness.bat](./scripts/bootstrap-harness.bat) and [invoke-unity.ps1](./scripts/invoke-unity.ps1) |
| [.agents/skills/](../.agents/skills/) | AI (On-Demand) | Executable runbooks (`unity-cli-recompile`, `unity-test-runner`, `unity-headless-scene-ops`, `unity-official-skills`, `harness-bootstrap`) |

---

## Visual Diagrams (Humans Only)

- [Task Lifecycle & State Machine](./diagrams/task-lifecycle.md)
- [Workspace Isolation & CLI Topology](./diagrams/workspace-topology.md)
- [Core Architecture (Composition + EventBus)](./diagrams/core-architecture.md)
- [Assembly Boundaries (`.asmdef` Graph)](./diagrams/assembly-boundaries.md)
- [Scene Hierarchy Root Skeleton](./diagrams/scene-hierarchy.md)

---

## Human Quick Start

1. **Bootstrap environment** (Windows, from any directory):
   ```cmd
   .agent-harness\scripts\bootstrap-harness.bat
   ```
   Verifies Node/npx, Unity 6 (`6000.3.10f1`) CLI, harness files, runtime folders (`Logs/`, `.agent-harness/tasks/`, `Assets/Editor/AgentAutomation/`), and `Unity-Technologies/skills`.
2. **Approval Gates**:
   - **Gate 1 (`TODO` → `DOING`)**: Primary Agent writes `tasks/TASK-XXX-<name>.md` and pauses for your consent before delegating to an Execution Sub-agent.
   - **Gate 2 (`REVIEW` → `DONE`)**: Primary Agent verifies diff + Unity CLI test XMLs and pauses for your approval before purging the task spec and clearing `tasks-board.md`.
