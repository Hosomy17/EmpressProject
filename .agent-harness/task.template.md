# TASK-XXX: [Task Title]

* **ID**: TASK-XXX
* **Status**: TODO
* **Target Engine**: Unity 6 (`6000.3.10f1`)
* **Target Platform**: StandaloneWindows64

---

## 1. Objective
<!-- 1-2 sentences describing the exact goal and expected outcome. -->
[State the exact goal and expected runtime behavior or deliverable here.]

---

## 2. Target File Paths
<!-- Explicit list of exact file paths. Planning agent MUST resolve all paths upfront. -->

### Existing Files to Modify
* `Assets/_Project/Source/Project/...` - [Brief role of modification]

### New Files to Create
* `Assets/_Project/Source/Project/...` - [Brief role of new file]

---

## 3. Context & Architectural Constraints
<!-- Direct constraints from source-of-truth.md applicable to this task. -->
* **Assembly Definition**: `[e.g., Com.Voobox.Project.Component — local behavior logic]`
* **Composition Pattern**: `[e.g., New XyzComponent is a plain C# class, injected into AbcComposer via constructor with IXyzData]`
* **Event Pattern**: `[e.g., Cross-entity: EventBus.Notify<SomeEvent>() / EventBus.Subscribe<SomeEvent>() | Local: Direct method call from Composer to Component]`
* **Performance / GC**: `[e.g., Zero heap allocations in Update(), pre-cached component refs in Awake(), Animator hashes via StringToHash()]`
* **Async Pattern**: `[e.g., UniTask for async operations, TaskHandle for cancellation]`
* **Test Creation Constraint**: `Do not write tests that involve calls to a coroutine or UniTask.`

---

## 4. Execution & Unity CLI Steps
<!-- Concrete step-by-step instructions with exact commands for the execution agent. -->

1. **Implement Changes**:
   * [Step 1: modify/create target files according to constraints above]
   * [Step 2: ensure no manual .meta editing; rely on Unity AssetDatabase]

2. **Trigger Headless Recompile (if needed)**:
   ```powershell
   & "C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe" -batchmode -nographics -projectPath "." -quit -logFile "Logs/Recompile.log"
   ```

3. **Execute Verification Tests via Unity CLI**:
   ```powershell
   & "C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe" -batchmode -nographics -projectPath "." `
     -runTests -testPlatform editmode `
     -testResults "Logs/EditMode-Results.xml" `
     -logFile "Logs/EditMode.log"
   ```

4. **Optional Skills Execution (if applicable)**:
   ```bash
   npx skills run <skill-name>
   ```

---

## 5. Definition of Done (Acceptance Criteria)
<!-- Verifiable checklist required before transitioning status to REVIEW. -->
- [ ] All target files created/modified at exact specified paths.
- [ ] Follows Composition pattern: Components are plain C# classes, Composers are MonoBehaviours.
- [ ] Cross-entity interaction uses EventBus; local operations use direct Composer→Component calls.
- [ ] No manual `.meta` modifications performed.
- [ ] Zero GC allocations in update loops (verified against SoT).
- [ ] Async operations use UniTask with TaskHandle for cancellation.
- [ ] Unity CLI compilation clean with 0 errors in `Logs/AgentUnity.log`.
- [ ] All EditMode / PlayMode CLI tests pass cleanly (`TestResults.xml` shows 0 failures).
- [ ] Conformance attestation documented in execution log.
- [ ] Task status updated to `REVIEW` in this file and in `.agent-harness/tasks-board.md`.
