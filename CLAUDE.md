# Unity Project Architecture & Structure Rules — Clean Architecture

> This is a global reference file used across all my Unity projects. Claude Code must consult this document before creating, moving, or editing any file.

---

## 1. Architectural Philosophy

This structure is based on **Clean Architecture** by Robert C. Martin (Uncle Bob), from *Clean Architecture: A Craftsman's Guide to Software Structure and Design*. Its foundational principle is the **Dependency Rule**:

> Source code dependencies must point only inward (from outer layers toward inner layers). Nothing in an inner layer should know anything about an outer layer.

The four core layers (innermost to outermost):

| Layer | Role | Allowed dependencies |
|---|---|---|
| **Domain (Entities)** | Pure game business rules and models (e.g. health logic, scoring rules, inventory rules) | Nothing — not even `UnityEngine` |
| **Application (Use Cases)** | Orchestrates domain logic; "what should happen" | Domain only |
| **Infrastructure (Interface Adapters)** | Concrete implementations: persistence, networking, files, Unity APIs, Addressables | Domain + Application |
| **Presentation (Frameworks & Drivers)** | MonoBehaviour, Views, UI, animation, input | Application (via interfaces) |

Key point: outer layers depend on **abstractions** (interfaces) defined in inner layers, never the reverse (Dependency Inversion Principle).

### How this differs from a textbook Clean Architecture implementation
In game development, a strict textbook implementation (a Gateway/Repository class for every trivial interaction) tends to add unnecessary overhead. This is why a lighter variant known as **Simple Clean Architecture (SCA)** has become common in the Unity community — it keeps the five core components (Entity, Gateway, UseCase, Presenter, View) but without the extra ceremony. This project follows that lightweight approach: the layering is preserved, but the number of intermediary classes is kept to a minimum.

### Unity-specific challenges and how they're handled
- **MonoBehaviour has no constructor** → business logic is never written inside a MonoBehaviour; MonoBehaviour only plays the role of View/Controller in the Presentation layer and delegates work to plain C# classes in Application/Domain.
- **ScriptableObject** as a data container or event channel goes into `Domain` (if it's a pure data model) or `Infrastructure` (if it's a concrete Repository implementation), depending on its role.
- **Enforcing the Dependency Rule at compile time**: each layer gets its own `Assembly Definition (.asmdef)`. `Domain.asmdef` has zero reference to `UnityEngine.asmdef`. This is the only real way to enforce Clean Architecture in Unity — without asmdef, the Dependency Rule is just a verbal agreement.
- **Dependency Injection**: use a lightweight container (VContainer or Zenject) to wire interfaces to implementations at the Composition Root, instead of Singletons or `FindObjectOfType`.

---

## 2. Folder Structure

The key requirement: **each feature's assets live inside that feature's own folder** (Feature-Colocation) — not in global folders like `Prefabs/`, `Materials/`, etc. Combining Clean Architecture's layering with a Feature-First folder layout means everything related to one capability (code and assets) lives in one place, making it easy to add or remove a feature.

```
Assets/
├── _Project/
│   ├── Core/                          # Shared across the whole project, not tied to one feature
│   │   ├── Domain/
│   │   │   ├── Entities/
│   │   │   ├── ValueObjects/
│   │   │   └── Interfaces/            # Generic ports (IRepository<T>, IEventBus, ...)
│   │   ├── Application/
│   │   │   └── UseCases/              # Base/abstract use cases
│   │   ├── Infrastructure/
│   │   │   ├── Persistence/           # SaveSystem, JSON/Binary serializers
│   │   │   ├── Networking/
│   │   │   └── Analytics/
│   │   ├── Presentation/
│   │   │   └── Base/                  # BasePresenter, BaseView, BaseController
│   │   └── Core.asmdef
│   │
│   ├── Features/
│   │   ├── PlayerMovement/
│   │   │   ├── Domain/
│   │   │   │   ├── Entities/          # PlayerState.cs, MovementRules.cs
│   │   │   │   └── Interfaces/        # IMovementRepository.cs
│   │   │   ├── Application/
│   │   │   │   └── UseCases/          # MovePlayerUseCase.cs, JumpUseCase.cs
│   │   │   ├── Infrastructure/
│   │   │   │   └── Repositories/      # PlayerInputRepository.cs (concrete implementation)
│   │   │   ├── Presentation/
│   │   │   │   ├── Views/             # PlayerView.cs (MonoBehaviour)
│   │   │   │   ├── Presenters/        # PlayerPresenter.cs
│   │   │   │   └── Controllers/       # PlayerInputController.cs
│   │   │   ├── Assets/                # This feature's assets ONLY
│   │   │   │   ├── Prefabs/
│   │   │   │   ├── Materials/
│   │   │   │   ├── Animations/
│   │   │   │   ├── Audio/
│   │   │   │   ├── Textures/
│   │   │   │   └── Data/              # ScriptableObject instances (e.g. MovementConfig.asset)
│   │   │   ├── Tests/
│   │   │   │   ├── EditMode/
│   │   │   │   └── PlayMode/
│   │   │   └── PlayerMovement.asmdef
│   │   │
│   │   ├── Inventory/
│   │   │   └── (same structure as above)
│   │   │
│   │   └── Dialogue/
│   │       └── (same structure as above)
│   │
│   ├── Shared/                        # Used by 2+ features but not core to the project
│   │   ├── UI/                        # Generic UI components (Popup, Button, LoadingSpinner)
│   │   ├── Utilities/
│   │   ├── Extensions/
│   │   └── Shared.asmdef
│   │
│   └── Scenes/
│       ├── Bootstrap/                 # Startup scene and Composition Root
│       ├── Gameplay/
│       └── UI/
│
├── ThirdParty/                        # External plugins (Asset Store, git submodules) — left untouched
├── Settings/                          # Input Actions, URP/HDRP settings, global config ScriptableObjects
├── Resources/                         # Only when truly necessary (prefer Addressables)
├── StreamingAssets/
├── Editor/                            # Project-wide, editor-only tools
└── Tests/                             # Project-level integration tests (not feature-specific)
```

### Why assets live inside each feature instead of global folders
In the traditional Unity layout (global `Prefabs/`, `Materials/`, `Scripts/`), fully understanding one capability means jumping between 5-6 unrelated folders. With Feature-Colocation, removing or refactoring a feature is essentially deleting/moving one folder, and the risk of hidden coupling drops significantly.

---

## 3. Dependency Rule by Folder

```
Presentation   ──depends on──>  Application (via interface)
Infrastructure ──depends on──>  Application, Domain (implements interfaces)
Application    ──depends on──>  Domain
Domain         ──depends on──>  (nothing)
```

- `Domain/` never references `UnityEngine`, `MonoBehaviour`, or `ScriptableObject` as an active base class. If logic needs a Unity API, that's a signal it belongs in `Infrastructure` or `Presentation`, not `Domain`.
- `Infrastructure` is never instantiated directly (`new`) by `Presentation`; it's always injected via an interface through the DI container.
- Communication between different features only happens through an interface defined in `Core/Domain/Interfaces` or a shared Event Bus — never a direct reference from one feature into another feature's internal classes.

---

## 4. Naming Conventions

| File type | Pattern | Example |
|---|---|---|
| Entity | `<Name>.cs` | `PlayerState.cs` |
| Use Case | `<Verb><Noun>UseCase.cs` | `MovePlayerUseCase.cs` |
| Interface (Port) | `I<Name>.cs` | `IMovementRepository.cs` |
| Concrete implementation | `<Name><Role>.cs` | `PlayerInputRepository.cs` |
| Presenter | `<Feature>Presenter.cs` | `PlayerPresenter.cs` |
| View (MonoBehaviour) | `<Feature>View.cs` | `PlayerView.cs` |
| Controller (input/Unity lifecycle) | `<Feature>Controller.cs` | `PlayerInputController.cs` |
| ScriptableObject data | `<Name>Config.cs` / `<Name>Data.cs` | `MovementConfig.cs` |
| DTO | `<Name>Dto.cs` | `PlayerStateDto.cs` |
| Test | `<ClassUnderTest>Tests.cs` | `MovePlayerUseCaseTests.cs` |

- Namespace: `<CompanyName>.<ProjectName>.Features.<FeatureName>.<Layer>`
  Example: `Studio.MyGame.Features.PlayerMovement.Domain`
- Classes and methods: `PascalCase`. Private fields: `_camelCase`. Constants: `UPPER_SNAKE_CASE`.
- Each feature's folder name exactly matches its namespace segment (no spaces, PascalCase).

---

## 5. Operating Instructions for Claude Code

While working on this project, always follow these rules:

1. **Before creating any new file**, ask two questions: (a) which feature does this belong to? (b) which layer (Domain/Application/Infrastructure/Presentation)? Then create it at exactly that path.
2. If a new class is pure business logic (no Unity dependency) → `Domain` or `Application`. If it needs `MonoBehaviour`, `Transform`, `Input`, or any other Unity API → `Infrastructure` or `Presentation`.
3. **Never** use `using UnityEngine;` inside `Domain/`.
4. For a new feature, always scaffold the full skeleton: `Domain/Application/Infrastructure/Presentation/Assets/Tests` plus a dedicated `.asmdef` for that feature.
5. Keep code local to the feature that uses it (colocation). Only move something to `Core/` or `Shared/` once 2+ features actually need it — avoid premature abstraction.
6. Feature-specific visual/audio assets (prefabs, materials, animations, audio, textures, ScriptableObject data) go inside `Features/<Name>/Assets/`, never in Unity's old global folders.
7. For every new Use Case and Entity in Domain/Application (testable without the Unity runtime), create a matching EditMode test in `Tests/EditMode/`.
8. Communication between two features only happens through an interface in `Core/Domain/Interfaces` or a shared Event Bus; never `GetComponent` or a direct reference between Presentation classes of different features.
9. Dependency injection always happens through the Composition Root (the `Bootstrap` scene) and the DI container; avoid `FindObjectOfType`, `GameObject.Find`, and global Singletons unless explicitly requested.
10. For short prototypes/game jams (under a week), don't force this full structure — but still ask whether a lighter version of these rules fits better.

---

## 6. Comparison With Common Game Industry Approaches

| Approach | Strength | Weakness relative to this structure |
|---|---|---|
| Traditional Unity layout (global `Scripts/`, `Prefabs/`, `Materials/`) | Simple and fast for small projects | As the project grows, finding all files for one feature becomes hard; lots of hidden coupling |
| Pure MVC/MVP without a domain layer | Separates View from logic | Business logic and orchestration logic get mixed together; low testability |
| ECS/DOTS | High performance for heavy games/simulations | Completely different mental model (data-oriented); combining it directly with Clean Architecture needs separate design work and isn't covered in this document |
| Feature-First without layering (just a folder per feature) | Good colocation | Without the Dependency Rule, each feature gradually turns into its own "Big Ball of Mud" |
| This structure (Clean Architecture + Feature-Colocation) | Gets both Uncle Bob's layered separation and the benefit of finding a feature's assets quickly | Requires more discipline and some upfront overhead to set up asmdef/DI per feature |

---

## 7. Quick Decision Summary

- "Where should this file go?" = "which feature + which layer."
- "Is this dependency allowed?" = "does it point inward?"
- "Where is this class defined vs. implemented?" = "interface in Domain/Application, implementation in Infrastructure."
