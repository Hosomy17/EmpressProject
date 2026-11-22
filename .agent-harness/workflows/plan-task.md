# Workflow: Plan Task (`TODO` + Gate 1)

> Human visual diagram: [Task Lifecycle](../diagrams/task-lifecycle.md)

Follow [planner-rules.md](../rules/planner-rules.md) throughout this workflow.

1. **Clarify Scope**: If requirements are ambiguous or conflict with existing architecture, stop and ask the user (Rule P5).
2. **Selectively Inspect Architecture**: Load ONLY the `.agent-harness/architecture/*.md` shards relevant to the request and resolve every existing/new file path upfront (Rule P2).
3. **Create Task Spec**:
   - Determine the next sequential `TASK-XXX` ID from `.agent-harness/tasks/` and [tasks-board.md](../tasks-board.md).
   - Clone [task.template.md](../task.template.md) to `.agent-harness/tasks/TASK-XXX-<name>.md` with `Status: TODO`.
   - Populate `## 2. Required Context & Skills` with the exact minimal list of architecture shards and `.agents/skills/` names the Execution Sub-agent should load.
   - Populate exact target file paths, applicable architectural constraints, step-by-step execution/CLI steps, and Definition of Done.
4. **Register on Board**: Add a `TODO` row to [tasks-board.md](../tasks-board.md).
5. **Gate 1 — Pause for User Consent**: Present the planned `TASK-XXX-<name>.md` summary and file paths to the user and **pause**. Do not set `DOING` or delegate until the user explicitly consents.
6. **Delegate**: Once the user consents, spawn an Execution Sub-agent (`invoke_subagent`) passing `.agent-harness/tasks/TASK-XXX-<name>.md` as its assignment and instructing it to follow `AGENTS.md` → [execute-task.md](./execute-task.md).
