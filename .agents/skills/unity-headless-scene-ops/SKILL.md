---
name: unity-headless-scene-ops
description: >-
  Perform safe headless structural mutations on Unity .unity scenes and .prefab
  assets via Editor batch methods. Use whenever creating or restructuring scenes
  or prefabs instead of editing YAML by hand.
---

# Unity Headless Scene & Prefab Ops

1. **Load Scene Standard**: Before authoring or restructuring any scene or `Prefabs/SceneSettings/` prefab, read `.agent-harness/architecture/scene-hierarchy.md`.
2. **Author Batch Method**:
   - Create an Editor script under `Assets/Editor/AgentAutomation/` using official Unity APIs (`EditorSceneManager`, `PrefabUtility`, `Undo`).
   - Never edit `.unity` or `.prefab` YAML structurally via raw text search-and-replace.
   - Never copy `Assets/_Project/Settings/Scenes/URP2DSceneTemplate.unity` without restructuring it to the 4-separator, prefab-only standard.
   - If creating a new scene, register it in `ProjectSettings/EditorBuildSettings.asset` via `EditorBuildSettings.scenes`.
3. **Execute Headlessly**:
   ```powershell
   powershell -ExecutionPolicy Bypass -File .agent-harness/scripts/invoke-unity.ps1 -Mode ExecuteMethod -Method "AgentAutomation.<ClassName>.<MethodName>"
   ```
4. **Verify & Clean Up**:
   - Inspect `Logs/BatchExecute.log` for `error CS` or `Exception:`.
   - Delete temporary one-off migration scripts from `Assets/Editor/AgentAutomation/` after execution and run `unity-cli-recompile` so `.meta` state stays clean.
   - Verify the result against the Conformance Checklist in `.agent-harness/architecture/scene-hierarchy.md`.
