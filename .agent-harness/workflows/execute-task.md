# Workflow: Execute Task (`DOING` → `REVIEW`)

> Human visual diagram: [Task Lifecycle](../diagrams/task-lifecycle.md)

Follow [executor-rules.md](../rules/executor-rules.md) throughout this workflow.

1. **Load Minimal Context**: Read ONLY your assigned `.agent-harness/tasks/TASK-XXX-*.md`, [executor-rules.md](../rules/executor-rules.md), this workflow, and the exact files listed in the task's `Required Context & Skills` section.
2. **Claim Task (`DOING`)**: Update `Status` to `DOING` in both `.agent-harness/tasks/TASK-XXX-*.md` and [tasks-board.md](../tasks-board.md).
3. **Implement Changes**:
   - Execute the task's file creations/edits strictly within the specified paths and constraints.
   - Activate `unity-official-skills` or `unity-headless-scene-ops` only if listed or required by the task.
   - Never manually edit `.meta` files or raw scene/prefab structural YAML.
4. **Log SoT Conformance Attestation**: Before running final verification tests, document this attestation in your execution notes/report:
   ```markdown
   ### Source of Truth Conformance Attestation
   * Assembly / Layer: [e.g. Com.Voobox.Project.Component — local behavior logic]
   * Composition / Scene: [e.g. Plain C# class injected into Composer via IData interface; or scene-hierarchy conformance]
   * Event Pattern: [e.g. EventBus.Notify<T>() / Subscribe<T>() or N/A (local only)]
   * Memory, GC & Async: [e.g. Cached refs in Awake(), zero Update() allocations, UniTask + TaskHandle]
   ```
5. **Compile & Verify**:
   - If scripts or Unity assets were added, moved, renamed, or deleted, activate `unity-cli-recompile` first to refresh `AssetDatabase` and check `Logs/Recompile.log` for 0 errors.
   - Activate `unity-test-runner` to run EditMode (and PlayMode if applicable) tests and verify `0` failures in `Logs/*-Results.xml`.
   - On any error, triage `Logs/*.log` (`error CS`, `Exception:`, `Assertion failed`), fix, and re-run until clean.
6. **Submit for Review (`REVIEW`)**: Check off the Definition of Done items in `TASK-XXX-*.md`, set `Status` to `REVIEW` in both `TASK-XXX-*.md` and [tasks-board.md](../tasks-board.md), and report modified files, SoT attestation, and test results back to the Tech Lead.
