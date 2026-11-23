# Project Context & Technical Topology

> Human visual diagram: [Workspace & CLI Topology](../diagrams/workspace-topology.md)

## 1. Technical Profile
- **Engine**: Unity 6 (`6000.3.10f1`), CoreCLR / `.NET 8` (C# 12), StandaloneWindows64.
- **Render Pipeline**: URP 2D (`17.3.0`).
- **Key Middleware**: FMOD (`Assets/Plugins/FMOD/`), Cysharp UniTask, Unity Input System (`1.18.0`), Cinemachine (`3.1.6`), Addressables (`2.9.1`), SerializeReferenceExtensions, Facepunch.Steamworks (`Assets/Plugins/Facepunch.Steamworks/`, test AppID `480`), NUnit Test Framework (`1.6.0`).
- **Exact UPM Package Versions**: Inspect `Packages/manifest.json` directly when needed.
- **Unity CLI Resolution**: `$env:UNITY_EDITOR_PATH` → `C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe` → Unity Hub Editor directory scan (automated by [invoke-unity.ps1](../scripts/invoke-unity.ps1)).

## 2. Current Feature & Scene Baseline
- **Hero (`HeroComposer`)**: Decoupled intent via `IHeroController` (`HeroInputController` default, swappable with `HeroBrainController`), horizontal movement (`Movement2DComponent`), Jump + buffer/cut/pogo (`Jump2DComponent`), Dash (`DasherComponent`), 4-way attack + recoil (`AttackerComponent`), Hit Stop (`FrameFreezerComponent`), Screen Shake + camera offset (`ScreenShakerComponent`), Sprite flip & animations (`HeroAnimatorComponent`), Ghost i-frames, Damage/Knockback via `Hurtbox` child trigger (`Sensor` layer 9, hierarchy layer resolution, no `Hero` ↔ `Enemy` physical collision), Particles, FMOD SFX, Health (`HealthComponent` → emits `HeroDiedEvent`).
- **Enemy (`EnemyComposer` + `Com.Voobox.Project.AI`)**: Modular, inheritance-free AI (`StateMachineBrainComponent : IEnemyController`, `EnemyContext`, `BrainProfileScriptableObject`, 6 `IAIBehavior` atoms, 9 `IAICondition` atoms), composer-owned `OnTriggerEnter2D` / `OnTriggerExit2D` zone tracking (`DetectionZone` collider on Physics2D `Sensor` layer 9 colliding with `Hero` and `Enemy`), dynamic `SetDetectionRangeMultiplier` / `SetDetectionTargetLayer`, movement + dash lunge + health (`Movement2DComponent`, `DasherComponent`, `HealthComponent`), plus `enemy1.prefab`, `Enemy1Data.asset`, and `Enemy1Brain.asset` (`Idle` / `Combat` / `LungeAttack` / `Waiting` / `Hunting` — not yet placed in any scene).
- **Boss (`BossComposer`, `BossAttackEmitter`)**: Init (`InitializeBossEvent`), Raise of Swords (`DoRaiseOfSwordsEvent`), Health & damage (`BossData.asset`, `HealthComponent` → emits `BossDiedEvent`). **Not yet implemented**: Boss AI / state machine, additional attack patterns.
- **Cross-Scene Systems (`Com.Voobox.Project.Systems`)**: `GameScript` (loads `GameOverScene` on `HeroDiedEvent`, `VictoryScene` on `BossDiedEvent`), `PauseController` (Escape toggles pause).
- **Framework**: `EventBus` (`Com.Voobox.Framework.Events`), `ObjectPooler` / `PooledObject` / `ReturnToPoolBehaviour` (`Com.Voobox.Framework.ObjectPooling`), Steamworks init (`BossSceneScript` + `PlaceSteamName`), `TaskHandle` (`Utils`).
- **Scenes (4)**: `MainMenuScene.unity`, `BossScene.unity`, `GameOverScene.unity`, `VictoryScene.unity`.
- **Tests**: 30 EditMode NUnit test fixtures (131 project tests; 132 total EditMode run) in `Assets/Tests/EditMode/` (`Com.Voobox.Project.Tests.EditMode`). `Assets/Tests/PlayMode/` does not yet exist.

## 3. Directory Anchor Points

| Path | Ownership / Purpose |
| :--- | :--- |
| `.agent-harness/` | Agent rules, workflows, architecture shards, task board, active `tasks/`, scripts, human `diagrams/` |
| `.agents/skills/` | On-demand executable Agent Skills (`SKILL.md`) |
| `Assets/_Project/Source/Framework/` | Shared `Events/` and `ObjectPooling/` (no project dependencies) |
| `Assets/_Project/Source/Project/` | `AI/` (`Core/`, `Behaviors/`, `Conditions/`), `Component/`, `Data/` (`Control/`, `Interfaces/`), `Entity/`, `Events/` (`BossEvents/`, `HeroEvents/`, `SteamworksEvents/`), `Others/`, `Scripts/`, `Systems/`, `Utils/` |
| `Assets/_Project/Prefabs/` | `Units/` (`Hero`, `Boss`, `enemy1`), `Projectiles/` (`BigSword`, `BossAttackEmitter`), `VFXs/` (`VFX_Hit_01`), `SceneSettings/` (role & `Game_Systems` prefabs) |
| `Assets/_Project/Scenes/` | `MainMenuScene.unity`, `BossScene.unity`, `GameOverScene.unity`, `VictoryScene.unity` |
| `Assets/_Project/ScriptableObject/` | `HeroData.asset`, `BossData.asset`, `Enemy1Data.asset`, `Enemy1Brain.asset` |
| `Assets/_Project/InputSystem/` | Generated `InputSystemActions` + `.asmdef` |
| `Assets/Editor/AgentAutomation/` | Headless Editor batch scripts for scene/prefab ops |
| `Assets/Tests/EditMode/` | NUnit EditMode test fixtures |
| `Packages/`, `ProjectSettings/` | UPM manifest/lock and Unity engine settings (`Sensor` layer 9, `Enemy` ↔ `Ground` collision, `Hero` ↔ `Enemy` ignored) |
| `Logs/` | Ephemeral headless CLI logs (`*.log`) and NUnit XML results (`*-Results.xml`) |
