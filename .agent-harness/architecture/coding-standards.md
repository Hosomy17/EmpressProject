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

## 6. Implicit Typing (`var`)
- **Use `var`** when the exact type is explicitly visible on the right-hand side of the assignment, reducing visual repetition and improving readability:
  - Object or array creation: `var list = new List<string>();`, `var buffer = new CompiledTransition[count];`
  - Explicit casts and `as` casts: `var handler = (Action<MyEvent>)Delegate.CreateDelegate(...);`, `var move = behavior as MoveTowardTargetBehavior;`
  - Generic factory/lookup calls whose type argument is the exact return type: `var animator = GetComponent<Animator>();`, `var identity = obj.AddComponent<PooledObject>();`, `var data = ScriptableObject.CreateInstance<EnemyDataScriptableObject>();`, `var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);`, `var camera = Object.FindFirstObjectByType<Camera>();`
  - Generic `out var` when the call's type argument already states the type: `TryGetComponent<PooledObject>(out var identity)`
- **Avoid `var` (use explicit types)** when the type is not obvious from the right-hand side without IDE assistance:
  - Non-generic method calls and awaited tasks: `float result = Calculate();`, `HeroBodyState bodyState = BuildBodyState();`, `RaycastHit2D hit = Physics2D.BoxCast(...);`, `Vector2 position = collision.ClosestPoint(...);`, `GameObject[] spawned = await UniTask.WhenAll(...);`
  - Property, field, and indexer reads: `object key = assetReference.RuntimeKey;`, `Transform poolParent = m_poolParents[key];`, `float originalGravity = m_rigidbody2D.gravityScale;`
  - Arithmetic, boolean, and ternary expressions: `float speed = vectorX * m_movementData.Speed;`, `bool isRunning = m_isGrounded && ...;`, `float recoilDirection = isDirectionRight ? 1f : -1f;`
  - Built-in primitive literals, `typeof(...)`, `for` counters, `foreach` loop variables, and non-generic `out` variables: `int value = 0;`, `string name = "Unknown";`, `Type eventType = typeof(T);`, `for (int i = 0; i < count; i++)`, `foreach (Delegate callback in callbacks)`, `TryGetValue(eventType, out List<Delegate> callbacks)`
- **Target-Typed `new()`**: Keep the explicit left-hand type when using target-typed `new()` (C# disallows `var x = new();`).
- **Scope**: Applies to project-authored code under `Assets/_Project/Source/` and `Assets/Tests/`. Exclude generated code (e.g., `Assets/_Project/InputSystem/InputSystemActions.cs`) and third-party plugins (`Assets/Plugins/`).

## 7. Testing Constraints
- **No Coroutines or UniTask in Tests**: Never write tests that call a coroutine or `UniTask`.
- Keep EditMode unit tests synchronous and deterministic (prefer constructor injection on plain C# Components and injectable delegates such as `GameScript.SetSceneLoader` for scene-transition logic).
