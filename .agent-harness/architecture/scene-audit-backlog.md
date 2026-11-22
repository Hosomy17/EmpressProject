# Scene Audit Baseline & Backlog (2026-11-21)

Observed across `MainMenuScene`, `BossScene`, `GameOverScene`, and `VictoryScene`: all four scenes already use the four-separator skeleton with prefab-only children. Known deviations from [scene-hierarchy.md](./scene-hierarchy.md), to be resolved through dedicated tasks:

| # | Scene / Asset | Deviation | Required Fix |
| :---: | :--- | :--- | :--- |
| 1 | All 4 scenes | Separator named `= Camera and Lighting ` (trailing space) | Rename to `= Camera and Lighting` |
| 2 | `VictoryScene` | Root order is Camera and Lighting → Environment → Systems → UI | Reorder to standard (`= Environment`, `= Camera and Lighting`, `= Systems`, `= UI`) |
| 3 | `MainMenuScene` | `MainMenu_UI.prefab` instance renamed to `MainMenuScene_UI` | Rename asset to `MainMenuScene_UI.prefab`; instance name follows |
| 4 | `BossScene` | `BossScene_Light.prefab` instance renamed to `BossScene_Lightning` | Revert instance name to `BossScene_Light` |
| 5 | `MainMenuScene_Camera`, `GameOverScene_Camera`, `VictoryScene_Camera` | Camera child named `Main Camera` (space) | Rename to `MainCamera` |
| 6 | `MainMenuScene_Systems` | Script named `MainScreenScript` | Rename to `MainMenuSceneScript` (code + asmdef-safe refactor) |
| 7 | `BossScene_Stage` | Five siblings named `Ground` (four inactive); typo `Ceilling` | Suffix repeated names (`Ground_01`…), remove or document inactive ones, rename to `Ceiling` |

> **Note**: Deviations 5–7 are prefab-internal or code changes; 1–4 are scene-level. None alter runtime behavior except #6 (type rename — verify serialized references survive via `[MovedFrom]` or re-assigning the component).
