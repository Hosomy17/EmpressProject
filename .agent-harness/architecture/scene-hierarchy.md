# Scene Hierarchy Standard

> Human visual diagram: [Scene Hierarchy](../diagrams/scene-hierarchy.md)  
> Headless scene/prefab mutation procedure: activate `.agents/skills/unity-headless-scene-ops/SKILL.md`.  
> Known pre-existing scene deviations: [scene-audit-backlog.md](./scene-audit-backlog.md).

Mandatory structural standard for every gameplay and UI scene in `Assets/_Project/Scenes/` (Target: Unity 6 `6000.3.10f1`).

## 1. Principles
1. **Fixed skeleton** — Every scene has the same four root separators, in the same order, even when a group is empty.
2. **Prefabs only** — Separators contain **only prefab instances**. No loose GameObjects at the scene root or directly under a separator. Author all scene content inside prefabs so it can be diffed, reused, and edited headlessly.
3. **One prefab per role** — Split scene content into role prefabs (`_Camera`, `_Light`, `_Systems`, `_UI`, `_Stage`, `_Background`) in `Assets/_Project/Prefabs/SceneSettings/`.
4. **Names mirror assets** — A prefab instance's name in the hierarchy must be identical to its prefab asset file name. Do not rename instances.

## 2. Root Skeleton (Mandatory Order)

| Order | Separator Name (exact) | Purpose |
| :---: | :--- | :--- |
| 1 | `= Environment` | World-space entities: units, attack emitters, stage geometry/colliders, backgrounds |
| 2 | `= Camera and Lighting` | Render camera, Cinemachine rig, camera bounds, URP 2D global light and post-processing volume |
| 3 | `= Systems` | Non-visual scene orchestration and services (scene scripts, object pooling, game/pause controllers) |
| 4 | `= UI` | Screen-space Canvas(es) and the `EventSystem` |

### Separator Object Rules
- **Name**: Exact match above — `=` prefix, single spaces, Title Case words, **no trailing whitespace**. (`=` is consumed by HierarchyDecorator `Assets/HierarchyDecorator/Settings.asset`, style *Header (Centered)*).
- **Components**: `Transform` only. No scripts, renderers, or colliders.
- **Transform**: Identity (`0,0,0`, identity rotation, `1,1,1` scale).
- **Tag / Layer / Active**: `Untagged` / `Default` / `true`.
- **Always present**: Keep all four separators even when empty (e.g. `= UI` in `BossScene`).
- **Reserved prefixes**: `-` (*Subheader*) and `+` (*Mini Header*) are reserved for sub-grouping **inside** prefabs only; never use at scene root.

## 3. Group Contents & Ordering

### 3.1 `= Environment`
| Order | Child | Source | Required |
| :---: | :--- | :--- | :--- |
| 1 | Units & emitters (e.g. `Hero`, `Boss`, `BossAttackEmitter`) | `Prefabs/Units/`, `Prefabs/Projectiles/` | Gameplay scenes |
| 2 | `{SceneName}_Stage` (playable area sprite + boundary/ground colliders) | `Prefabs/SceneSettings/` | Gameplay scenes |
| 3 | `{SceneName}_Background` (parallax/sprite layers `Layer_1..N`) | `Prefabs/SceneSettings/` | Optional |

- Pooled runtime objects (projectiles, VFX) are **never** placed in the scene; `ObjectPooler` spawns them at runtime.

### 3.2 `= Camera and Lighting`
| Order | Child | Contents | Required |
| :---: | :--- | :--- | :--- |
| 1 | `{SceneName}_Light` | `Global Light 2D` (`Light2D`), `Global Volume` (`Volume`) | Optional (required for lit 2D sprites) |
| 2 | `{SceneName}_Camera` | `MainCamera` (+ optional `CinemachineCamera`, `CameraBounds`) | Always |

```text
{SceneName}_Camera
├── MainCamera          Camera, UniversalAdditionalCameraData, [CinemachineBrain], single audio listener
├── CinemachineCamera   (optional) CinemachineCamera, PositionComposer, Confiner2D, ImpulseListener…
└── CameraBounds        (optional) BoxCollider2D for Cinemachine confiner
```
- Child camera GameObject is named `MainCamera` (no space) and tagged `MainCamera`.
- Exactly **one** audio listener per scene, on `MainCamera` (FMOD `StudioListener` instead of Unity `AudioListener` when playing FMOD events).

### 3.3 `= Systems`
| Order | Child | Contents | Required |
| :---: | :--- | :--- | :--- |
| 1 | `{SceneName}_Systems` | `{SceneName}Script` (`Com.Voobox.Project.Scripts`) + scene-scoped services | Always |
| 2 | `Game_Systems` | `GameScript`, `PauseController` (shared cross-scene prefab) | Gameplay scenes only |

- **Single service** in `{SceneName}_Systems`: attach component directly on prefab root (e.g. `GameOverScene_Systems` → `GameOverSceneScript`).
- **Multiple services** in `{SceneName}_Systems`: prefab root holds `Transform` only; each service lives on a named child (`BossScene_Systems/BossSceneScript`, `BossScene_Systems/ObjectPooling`).

### 3.4 `= UI`
| Order | Child | Required |
| :---: | :--- | :--- |
| 1 | `{SceneName}_UI` | Whenever the scene has UI |

```text
{SceneName}_UI
├── Canvas              Canvas, CanvasScaler, GraphicRaycaster
│   ├── {Purpose}Image  e.g. SplashImage
│   ├── {Purpose}Text   e.g. ContinueText, VictoryText (TextMeshProUGUI)
│   └── {Purpose}Button e.g. StartButton, YesButton (Image, Button)
│       └── Text        Button label (TextMeshProUGUI)
└── EventSystem         EventSystem, InputSystemUIInputModule
```
- Exactly **one** `EventSystem` per scene, sibling of `Canvas` inside `{SceneName}_UI`.
- Name UI elements `{Purpose}{ElementType}` in PascalCase; button label child is always `Text`.

## 4. Naming Conventions

| Element | Convention | Example |
| :--- | :--- | :--- |
| Scene file | `{Name}Scene.unity` | `BossScene.unity` |
| Scene role prefab | `{SceneName}_{Role}.prefab` (`Camera`, `Light`, `Systems`, `UI`, `Stage`, `Background`) | `VictoryScene_UI.prefab` |
| Cross-scene shared prefab | `Game_{Role}.prefab` | `Game_Systems.prefab` |
| Prefab instance name | Identical to asset file name | `BossScene_Light` |
| Scene orchestration script | `{SceneName}Script` | `BossSceneScript`, `VictorySceneScript` |
| GameObjects inside prefabs | PascalCase, no spaces (Unity defaults like `Global Light 2D` allowed) | `CameraBounds`, `StartButton` |
| Repeated siblings | Disambiguate with numeric suffix | `Ground_01`, `Ground_02` |

## 5. Canonical Template & `BossScene` Target Reference

```text
{SceneName}.unity
├── = Environment
│   ├── <Unit / Emitter prefabs>          (gameplay scenes)
│   ├── {SceneName}_Stage                 (gameplay scenes)
│   └── {SceneName}_Background            (optional)
├── = Camera and Lighting
│   ├── {SceneName}_Light                 (optional)
│   └── {SceneName}_Camera
├── = Systems
│   ├── {SceneName}_Systems
│   └── Game_Systems                      (gameplay scenes)
└── = UI
    └── {SceneName}_UI                    (when scene has UI)
```

Target `BossScene.unity`: `= Environment` (`BossAttackEmitter`, `Hero`, `Boss`, `BossScene_Stage`, `BossScene_Background`), `= Camera and Lighting` (`BossScene_Light`, `BossScene_Camera`), `= Systems` (`BossScene_Systems`, `Game_Systems`), `= UI` (empty).

## 6. Conformance Checklist
- [ ] Exactly four root objects in order: `= Environment`, `= Camera and Lighting`, `= Systems`, `= UI`.
- [ ] Separator names have no leading/trailing whitespace; separators are `Transform`-only, identity, active.
- [ ] Every separator child is a prefab instance; no loose GameObjects.
- [ ] Prefab instance names match asset file names; scene role prefabs follow `{SceneName}_{Role}`.
- [ ] Exactly one camera named `MainCamera`, one audio listener, and (if UI exists) one `EventSystem`.
- [ ] Scene orchestration script is named `{SceneName}Script` and lives in `{SceneName}_Systems`.
- [ ] Scene is registered in `ProjectSettings/EditorBuildSettings.asset`.
