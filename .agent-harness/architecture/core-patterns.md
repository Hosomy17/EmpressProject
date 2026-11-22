# Core Architecture Patterns

> Human visual diagram: [Core Architecture](../diagrams/core-architecture.md)

Empress uses a custom **Composition + Event-Driven Mediation** pattern (distinct from Unity DOTS) across three collaborating layers.

## 1. Three-Layer Pattern

| Layer | Responsibility | Rules & Constraints | Current Types |
| :--- | :--- | :--- | :--- |
| **Composers (Entities)** | Root `MonoBehaviour`s holding entity state flags, orchestration conditions, and component wiring. | Clone ScriptableObject data in `Awake()` via `Instantiate()`. Construct plain C# Components via data-interface injection. Emit/subscribe `EventBus` for cross-entity coordination. | `HeroComposer`, `BossComposer`, `BossAttackEmitter` |
| **Components (Local Logic)** | Plain C# classes implementing atomic behaviors on their parent entity. | **Not** `MonoBehaviour`s. Receive configuration via data interfaces in constructor. Never reference Composers or subscribe/notify `EventBus`. | `Movement2DComponent`, `Jump2DComponent`, `DasherComponent`, `AttackerComponent`, `HealthComponent`, `ScreenShakerComponent`, `HeroAnimatorComponent`, `FrameFreezerComponent` |
| **Data (Interfaces + SOs)** | Decoupled configuration contracts and serialized runtime data assets. | Components depend only on `I{Behavior}Data` interfaces, never concrete SOs or entity classes. | Interfaces: `IMovementData`, `IJumpData`, `IDasherData`, `IAttackerData`, `IHealthData`, `IScreenShakerData`. Concrete: `HeroData` (`HeroDataScriptableObject`, implements all 6), `BossData` (`BossDataScriptableObject`, implements `IHealthData` + `HurtLayer`) |

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
