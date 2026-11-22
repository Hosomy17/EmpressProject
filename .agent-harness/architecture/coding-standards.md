# Coding, Performance & Testing Standards

## 1. Zero-GC Hot Paths
- **Update Loops**: Zero heap allocations inside `Update()`, `FixedUpdate()`, and `LateUpdate()`.
- **Prohibited in Loops**: `new` on reference types, string concatenation/formatting, LINQ (`.Where()`, `.Select()`, etc.), capturing lambdas, and value-type boxing.
- **Buffers**: Pre-allocate persistent lists/arrays and reuse them via `.Clear()` or `Span<T>`.

## 2. Component Caching & Hashing
- Never call `GetComponent<T>()`, `FindObjectOfType<T>()`, or `Camera.main` inside update loops or frequent callbacks.
- Cache component references once in `Awake()` or `OnEnable()`.
- Cache Animator parameter IDs with `Animator.StringToHash()`.

## 3. Object Lifecycle & Pooling
- Dynamic runtime `Instantiate()` / `Destroy()` is prohibited for frequent entities (projectiles, VFX impacts, damage numbers).
- Use `ObjectPooler` (`Com.Voobox.Framework.ObjectPooling`, Addressables-based).

## 4. Async Pattern
- Use Cysharp **UniTask** for all new async flows; never introduce `System.Threading.Tasks.Task` or Unity coroutines.
- Manage cancellation via `TaskHandle` (`Com.Voobox.Project.Utils`), calling `GetNewToken()` to cancel prior overlapping operations.
- Use `.Forget()` for fire-and-forget `UniTaskVoid` calls.

## 5. Naming Conventions

| Element | Convention | Example |
| :--- | :--- | :--- |
| Namespaces & `.asmdef` | `Com.Voobox.{Layer}.{Area}` | `Com.Voobox.Project.Component` |
| Composers | `{Entity}Composer` | `HeroComposer`, `BossComposer` |
| Components | `{Behavior}Component` | `Movement2DComponent`, `HealthComponent` |
| Data Interfaces | `I{Behavior}Data` | `IMovementData`, `IHealthData` |
| Data Classes | `{Entity}Data` | `HeroData`, `BossData` |
| Data ScriptableObjects | `{Entity}DataScriptableObject` | `HeroDataScriptableObject` |
| Events | `{Action}Event` | `InitializeBossEvent`, `HeroDiedEvent` |
| Private / Serialized Fields | `m_camelCase` | `[SerializeField] private float m_attackDuration;` |
| Constants | `UPPER_SNAKE_CASE` | `RAISE_OF_SWORDS_LEFT_ATTACK` |
| Properties | `PascalCase` | `IsJumpBufferActive`, `MaxHitPoints` |

## 6. Testing Constraints
- **No Coroutines or UniTask in Tests**: Never write tests that call a coroutine or `UniTask`.
- Keep EditMode unit tests synchronous and deterministic (prefer constructor injection on plain C# Components and injectable delegates such as `GameScript.SetSceneLoader` for scene-transition logic).
