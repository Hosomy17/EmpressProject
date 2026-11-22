# Workflow: Review & Closeout (`REVIEW` → `DONE`)

> Human visual diagram: [Task Lifecycle](../diagrams/task-lifecycle.md)

Follow [reviewer-rules.md](../rules/reviewer-rules.md) throughout this workflow.

1. **Audit Submission**:
   - Inspect `git diff` / modified files against `TASK-XXX-*.md` target paths, constraints, and Definition of Done.
   - Confirm the Execution Sub-agent provided a valid Source of Truth Conformance Attestation.
   - Verify `Logs/*-Results.xml` and CLI logs directly (0 compilation errors, 0 failed tests).
2. **Handle Revisions (if needed)**: If verification or conformance fails, change status back to `DOING` in `TASK-XXX-*.md` and [tasks-board.md](../tasks-board.md) with explicit revision notes and message the Execution Sub-agent to fix.
3. **Gate 2 — Pause for User Review & Consent**: Present a concise `REVIEW` summary (files changed, attestation summary, observed test results) to the user and **pause** for explicit approval.
4. **Closeout & Cleanup (`DONE`)**: Upon user consent:
   - Delete `.agent-harness/tasks/TASK-XXX-<name>.md`.
   - Remove the task's row from [tasks-board.md](../tasks-board.md) (restore the `*None*` placeholder if the table becomes empty).
   - Check Rule R4: update [project-topology.md](../architecture/project-topology.md) (and any affected `architecture/*.md` or `diagrams/*.md`) only if the broader architecture, assemblies, scenes, or feature baseline changed.
   - Deliver a concise completion report citing verified file paths and test results.
