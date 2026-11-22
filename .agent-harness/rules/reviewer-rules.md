# Reviewer & Closeout Rules

Applies strictly to the Primary Agent / Technical Lead during `REVIEW` → `DONE` closeout.

1. **Rule R1 — Independent Verification**: Before presenting a `REVIEW` summary, independently verify:
   - The git diff matches `TASK-XXX-*.md` target paths, architectural constraints, and Definition of Done.
   - The Execution Sub-agent's Source of Truth Conformance Attestation is complete and accurate.
   - Unity CLI test results (`Logs/*-Results.xml`) and logs show 0 compilation errors and 0 test failures.
2. **Rule R2 — Gate 2 User Review Consent**: Present a concise `REVIEW` summary (changes, attestation, test counts/XML status) to the user and **pause**. Wait for explicit user consent before marking `DONE`.
3. **Rule R3 — Immediate Cleanup on `DONE`**: Upon user consent, permanently delete `.agent-harness/tasks/TASK-XXX-<name>.md` and remove its row from [tasks-board.md](../tasks-board.md) so the working tree stays clean.
4. **Rule R4 — Living Context Maintenance**: Evaluate whether the completed task altered architectural topology, assemblies, scenes, or the feature baseline. If so, update [project-topology.md](../architecture/project-topology.md) (and any affected `architecture/*.md` shards or `diagrams/*.md`) before final closeout.
