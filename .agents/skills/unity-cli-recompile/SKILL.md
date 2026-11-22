---
name: unity-cli-recompile
description: >-
  Headless Unity 6 (6000.3.10f1) recompile and AssetDatabase refresh. Use after
  adding, moving, renaming, or deleting C# scripts or Unity assets to generate
  valid .meta files and verify clean compilation.
---

# Unity CLI Recompile

1. Run the headless recompile helper from the repository root:
   ```powershell
   powershell -ExecutionPolicy Bypass -File .agent-harness/scripts/invoke-unity.ps1 -Mode Recompile
   ```
   *(Fallback direct invocation if needed:)*
   ```powershell
   & "C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe" -batchmode -nographics -projectPath "." -quit -logFile "Logs/Recompile.log"
   ```
2. Check the exit code (`0` = clean compilation and AssetDatabase refresh).
3. Inspect `Logs/Recompile.log` for any `error CS` or `Exception:` lines before proceeding to tests. Never manually create, edit, or delete `.meta` files.
