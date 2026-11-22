---
name: unity-test-runner
description: >-
  Execute Unity 6 (6000.3.10f1) headless EditMode and PlayMode NUnit tests and
  triage failures. Use during the verification gate or when authoring or validating
  Unity test fixtures.
---

# Unity Test Runner

> **Test Authoring Rule**: Never write tests that call a coroutine or `UniTask`.

1. **Run EditMode Tests** (`Assets/Tests/EditMode/`):
   ```powershell
   powershell -ExecutionPolicy Bypass -File .agent-harness/scripts/invoke-unity.ps1 -Mode EditModeTests
   ```
2. **Run PlayMode Tests** (only if `Assets/Tests/PlayMode/` exists and is required by the task):
   ```powershell
   powershell -ExecutionPolicy Bypass -File .agent-harness/scripts/invoke-unity.ps1 -Mode PlayModeTests
   ```
3. **Validate Exit Code**:
   - `0`: All tests passed.
   - `2`: One or more tests failed.
   - `3`: Run cancelled or compilation failed before test execution.
4. **Inspect Logs & XML Results**:
   - Check `Logs/EditMode-Results.xml` (or `Logs/PlayMode-Results.xml`) for `<test-case result="Failed">`.
   - Search `Logs/EditMode.log` (or `Logs/PlayMode.log`) for `error CS`, `Exception:`, `NullReferenceException:`, and `Assertion failed`.
   - Never suppress errors; fix and re-run until exit code `0` with `0` failures.
