---
name: harness-bootstrap
description: >-
  Verify and initialize local dependencies (Node/npx, Unity 6 6000.3.10f1 CLI,
  harness files, runtime directories, and Unity skills). Use on initial environment
  setup or when Unity CLI or npx skills commands fail.
---

# Harness Bootstrap

1. **Run the Bootstrapper** (anchored to repository root regardless of current directory):
   ```cmd
   .agent-harness\scripts\bootstrap-harness.bat
   ```
2. **Inspect the Summary**:
   - Confirm `[OK]` for `Node / npx CLI`, `Unity Editor CLI`, `Harness Integrity`, and `Unity Skills Link`.
   - Confirm runtime directories (`Logs/`, `.agent-harness/tasks/`, `Assets/Editor/AgentAutomation/`) exist at the repository root.
3. **Resolve Missing Unity Path** (if `Unity Editor CLI` reports `[WARN]`):
   ```cmd
   setx UNITY_EDITOR_PATH "C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe"
   ```
   Re-open the shell and re-run `.agent-harness\scripts\bootstrap-harness.bat` to verify `[OK]`.
