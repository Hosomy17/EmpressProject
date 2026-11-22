# Assembly Definition (`.asmdef`) Boundaries

> Human visual diagram: [Assembly Boundaries](../diagrams/assembly-boundaries.md)

All C# code must live inside explicit `Com.Voobox.*` `.asmdef` assemblies; loose un-assembled scripts in gameplay folders are prohibited.

## 1. Verified Dependency Matrix

| Assembly | Project Assembly References | External / Engine References |
| :--- | :--- | :--- |
| `Com.Voobox.Framework.Events` | *(none)* | *(none)* |
| `Com.Voobox.Framework.ObjectPooling` | *(none)* | Addressables, ResourceManager, UniTask |
| `Com.Voobox.Project.Utils` | *(none)* | *(none)* |
| `Com.Voobox.Project.InputSystem` | *(none)* | `Unity.InputSystem` |
| `Com.Voobox.Project.Events.BossEvents` | `Framework.Events` | *(none)* |
| `Com.Voobox.Project.Events.HeroEvents` | `Framework.Events` | *(none)* |
| `Com.Voobox.Project.Events.SteamworksEvents` | `Framework.Events` | *(none)* |
| `Com.Voobox.Project.Others` | `Framework.Events`, `Framework.ObjectPooling`, `Events.SteamworksEvents` | Addressables, ResourceManager, UniTask, FMODUnity, TextMeshPro |
| `Com.Voobox.Project.Data` | `Project.Others` | ArtificeToolkit, Cinemachine, FMODUnity |
| `Com.Voobox.Project.AI` | `Project.Data`, `Project.Utils` | `MackySoft.SerializeReferenceExtensions` |
| `Com.Voobox.Project.Component` | `Project.Data`, `Project.Others`, `Project.Utils`, `Project.InputSystem` | ArtificeToolkit, Cinemachine, FMODUnity, InputSystem, UniTask |
| `Com.Voobox.Project.Entity` | `Project.AI`, `Project.Component`, `Project.Data`, `Project.Others`, `Project.Utils`, `Project.InputSystem`, `Framework.Events`, `Events.BossEvents`, `Events.HeroEvents` | ArtificeToolkit, Cinemachine, FMODUnity, InputSystem, UniTask |
| `Com.Voobox.Project.Systems` | `Framework.Events`, `Events.HeroEvents`, `Events.BossEvents` | `Unity.InputSystem` |
| `Com.Voobox.Project.Scripts` | `Framework.Events`, `Events.BossEvents`, `Events.SteamworksEvents` | `UnityEngine.UI` |
| `Com.Voobox.Project.Tests.EditMode` | All Framework & Project runtime assemblies above (including `Project.AI`) | `UnityEngine.TestRunner`, `UnityEditor.TestRunner`, Addressables, Cinemachine, FMODUnity, InputSystem, TMP, UGUI, UniTask |

## 2. Boundary Rules
1. **Zero Circular Dependencies**: Maintain a strict DAG across all `.asmdef` layers.
2. **Framework Isolation**: `Com.Voobox.Framework.*` assemblies never reference `Com.Voobox.Project.*`.
3. **Event Isolation**: `Com.Voobox.Project.Events.*` assemblies reference **only** `Com.Voobox.Framework.Events`.
4. **Component, Data & AI Isolation**: `Project.Data`, `Project.Component`, and `Project.AI` do **not** reference `Framework.Events` or `Project.Entity`.
5. **EventBus Wiring Layer**: Only `Project.Entity`, `Project.Systems`, `Project.Scripts`, and `Project.Others` subscribe to or emit `EventBus` events.
