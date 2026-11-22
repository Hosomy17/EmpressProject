# TASK-XXX: [Task Title]

* **ID**: TASK-XXX
* **Status**: TODO
* **Engine**: Unity 6 (`6000.3.10f1`)
* **Platform**: StandaloneWindows64

## 1. Objective
[1-2 sentences stating the exact goal and expected runtime behavior or deliverable.]

## 2. Required Context & Skills (Planner-Selected)
<!-- Execution sub-agent loads ONLY the files and skills listed here. Delete unused entries. -->
* **Architecture Shards**:
  * `.agent-harness/architecture/coding-standards.md`
  * `.agent-harness/architecture/core-patterns.md`
  * `.agent-harness/architecture/assembly-boundaries.md`
  <!-- Include only if modifying scenes or Prefabs/SceneSettings/: -->
  <!-- * `.agent-harness/architecture/scene-hierarchy.md` -->
* **Skills (activate at the matching step)**:
  * `unity-cli-recompile` (if scripts/assets are added, moved, or deleted)
  * `unity-test-runner`
  <!-- Include only if applicable: `unity-headless-scene-ops`, `unity-official-skills` -->

## 3. Target File Paths
### Existing Files to Modify
* `Assets/_Project/...` - [Role of modification]

### New Files to Create
* `Assets/_Project/...` - [Role of new file]

## 4. Architectural Constraints
* **Assembly Definition**: `[e.g. Com.Voobox.Project.Component]`
* **Composition / Scene**: `[e.g. Plain C# Component injected into Composer via IData; or Scene Hierarchy Standard]`
* **Event Pattern**: `[e.g. EventBus.Notify<T>() / Subscribe<T>() or N/A]`
* **Performance / GC / Async**: `[Zero heap allocations in Update(), cached refs, UniTask + TaskHandle]`
* **Test Constraint**: `Do not write tests that involve calls to a coroutine or UniTask.`

## 5. Execution & Verification Steps
1. [Concrete implementation step 1]
2. [Concrete implementation step 2]
3. Log SoT Conformance Attestation before verification.
4. Activate `unity-cli-recompile` (if assets/scripts were added/moved/deleted) and `unity-test-runner` (`EditModeTests`) to confirm 0 errors and 0 failures.

## 6. Definition of Done
- [ ] All target files created/modified at exact specified paths with no manual `.meta` edits.
- [ ] Conforms to all constraints in Section 4 and SoT attestation logged.
- [ ] Unity CLI recompile and EditMode tests pass cleanly (`0` failures in `Logs/*-Results.xml`).
- [ ] Status updated to `REVIEW` in this file and `.agent-harness/tasks-board.md`.
