# Core Architecture Patterns

> Human visual diagram: [Core Architecture](../diagrams/core-architecture.md)

Empress uses a custom **Composition + Event-Driven Mediation** pattern (distinct from Unity DOTS) across collaborating layers.

## 1. Three-Layer Pattern + Composable Control & AI

| Layer | Responsibility | Rules & Constraints | Current Types |
| :--- | :--- | :--- | :--- |
| **Composers (Entities)** | Root `MonoBehaviour`s holding entity state flags, trigger/physics consolidation, orchestration conditions, and component wiring. | Clone ScriptableObject data in `Awake()` via `Instantiate()`. Construct plain C# Components via data-interface injection. Sample an `IEntityController` (`IHeroController` / `IEnemyController`) to decouple intent from action. Emit/subscribe `EventBus` for cross-entity coordination. | `HeroComposer`, `EnemyComposer`, `BossComposer`, `BossAttackEmitter` |
| **Components (Local Logic)** | Plain C# classes implementing atomic behaviors or input sampling on their parent entity. | **Not** `MonoBehaviour`s. Receive configuration via data interfaces in constructor. Never reference Composers or subscribe/notify `EventBus`. | `Movement2DComponent`, `Jump2DComponent`, `DasherComponent`, `AttackerComponent`, `HealthComponent`, `ScreenShakerComponent`, `HeroAnimatorComponent`, `FrameFreezerComponent`, `HeroInputController` |
| **AI (`Com.Voobox.Project.AI`)** | Inheritance-free, data-driven composable state machine (`StateMachineBrainComponent : IEnemyController`, `HeroBrainController : IHeroController`) driven by `EnemyContext`. | Zero `MonoBehaviour`s, zero `Physics2D` queries, and zero `EventBus` usage. Composes `[SerializeReference, SubclassSelector]` `IAIBehavior` and `IAICondition` atoms inside `BrainProfileScriptableObject`. | `EnemyContext`, `StateMachineBrainComponent`, `HeroBrainController`, `BrainProfileScriptableObject`, `IAIBehavior` (6 atoms), `IAICondition` (9 atoms) |
| **Data (Interfaces, Control Contracts & SOs)** | Decoupled configuration contracts, zero-GC intent/body structs, and serialized runtime data assets. | Components depend only on `I{Behavior}Data` interfaces and `IEntityController<TBodyState, TIntent>` contracts, never concrete SOs or entity classes. | Interfaces: `IMovementData`, `IJumpData`, `IDasherData`, `IAttackerData`, `IHealthData`, `IScreenShakerData`, `IEntityController<,>`, `IHeroController`, `IEnemyController`. Structs: `HeroIntent`, `HeroBodyState`, `EnemyIntent`, `EnemyBodyState`. Concrete: `HeroData` (`HeroDataScriptableObject`), `EnemyData` (`EnemyDataScriptableObject`), `BossData` (`BossDataScriptableObject`) |

## 2. EventBus & Cross-Entity Systems

`EventBus` (`Com.Voobox.Framework.Events`) bridges actions that cross entity or scene boundaries:

```csharp
public interface IEvent { }
public static class EventBus
{
    public static void Subscribe<T>(Action<T> callback) where T : IEvent;
    public static void Unsubscribe<T>(Action<T> callback) where T : IEvent;
    public static void Notify<T>(T eventArgs) where T : IEvent;
}
```

### Rules for New Events
1. Define a class implementing `IEvent` under `Assets/_Project/Source/Project/Events/<Domain>Events/` with its own `.asmdef`.
2. That `.asmdef` must reference **only** `Com.Voobox.Framework.Events`.
3. Emitters call `EventBus.Notify(new SomeEvent())` from a Composer, System, or Scene Script.
4. Subscribers call `EventBus.Subscribe<T>(handler)` in `Awake()` and `EventBus.Unsubscribe<T>(handler)` in `OnDestroy()`.
5. Cross-scene services (`GameScript`, `PauseController`) live in `Com.Voobox.Project.Systems`.

### Current Event & System Flows
| Event (`IEvent`) | Assembly | Emitter | Subscriber(s) |
| :--- | :--- | :--- | :--- |
| `InitializeBossEvent` | `Events.BossEvents` | `BossSceneScript.Start()` | `BossComposer.InitializeBoss()` |
| `DoRaiseOfSwordsEvent` | `Events.BossEvents` | `BossComposer.InitializeBoss()` | `BossAttackEmitter.DoRaiseOfSwords()` |
| `BossDiedEvent` | `Events.BossEvents` | `BossComposer.TakeDamage()` | `GameScript.OnBossDied()` (loads `VictoryScene`) |
| `HeroDiedEvent` | `Events.HeroEvents` | `HeroComposer` | `GameScript.OnHeroDied()` (loads `GameOverScene`) |
| `InitializeSteamworksEvent` | `Events.SteamworksEvents` | `BossSceneScript.Start()` | `PlaceSteamName.OnInitializeSteamworks()` |
