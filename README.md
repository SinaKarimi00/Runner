# Runner Prototype

Built using [Claude Code](https://claude.com/claude-code) as a mid-level developer.

## Unity Version
Unity 2021.3.45f1 (Built-in Render Pipeline, Android target platform, **Portrait** orientation — `PlayerSettings.defaultInterfaceOrientation` fixed to Portrait with all landscape autorotation disabled).

## Packages and Assets
- **Supercyan Character Pack — Animal People Sample** (`Assets/Supercyan Character Pack Animal People Sample/`): supplies the Wolf character model, rig/avatar, and the `FreeAnimations.controller` Animator Controller (parameters `MoveSpeed`, `Grounded`, `Pickup`, `Wave`). Used unmodified; only referenced from a new prefab.
- **Cinemachine** `2.10.3` (`com.unity.cinemachine`), assembly name `Cinemachine`.
- **DOTween** (Demigiant), imported as a precompiled plugin at `Assets/Plugins/Demigiant/DOTween/` (auto-referenced; no asmdef needed).
- **TextMeshPro** `3.0.6` (`com.unity.textmeshpro`), assembly `Unity.TextMeshPro`.
- **UGUI** (`com.unity.ugui`), assembly `Unity.ugui` — used for the gameplay Canvas (Score, Game Over panel, Restart button).
- Legacy Input Manager (`activeInputHandler: 0`) — no Input System package is installed, so input is read via `UnityEngine.Input` (mouse + touch).
- No DI framework (VContainer/Zenject) was present or added; wiring is done through a scene-level Composition Root instead (see Architecture).

## Architecture

A lightweight Clean Architecture, scoped to a single feature (`Assets/_Project/Features/RunnerPrototype/`), colocated with its own assets and tests:

```
Domain/          — RunnerGameState enum, Score value object (validation only, no Unity refs)
Application/     — RunnerGameFlowService, ScoreService, ISceneReloader (plain C#, Domain-only deps)
Infrastructure/  — RunnerGameplayConfig (ScriptableObject), SceneReloader (Unity SceneManager adapter)
Presentation/    — MonoBehaviours: input, player, environment (moving obstacles), collectibles, camera/impact effects, UI, composition root
```

Dependency Rule is enforced with one `.asmdef` per layer:
- `RunnerPrototype.Domain` — `noEngineReferences: true`, zero references.
- `RunnerPrototype.Application` — references Domain only.
- `RunnerPrototype.Infrastructure` — references Application + Domain.
- `RunnerPrototype.Presentation` — references Infrastructure, Application, Domain, `Cinemachine`, `Unity.TextMeshPro`, `Unity.ugui`.
- `RunnerPrototype.Tests.EditMode` / `RunnerPrototype.Tests.PlayMode` — reference only what they test.

**Dependency Injection**: no DI container exists in the project, and one was not added for this prototype (per scope). `RunnerCompositionRoot` (`Presentation/Bootstrap`) is a single `MonoBehaviour` on `Systems` that, in `Awake()`, constructs `RunnerGameFlowService`, `ScoreService`, and `SceneReloader` as plain C# objects, subscribes to their events, and explicitly initializes each Presentation component (`Initialize(...)` calls / `SetRunning`/`SetActive` methods) with serialized-reference-based wiring. No `FindObjectOfType`, `GameObject.Find`, or singletons are used at runtime; the one exception is a single, explicit, one-time `GetComponentsInChildren<CollectibleView>()` sweep over the `Level/Collectibles` container inside the Composition Root's `Awake()`, used purely to wire each collectible's score callback — this is composition-time wiring, not a runtime lookup.

**Presentation design notes**:
- `RunnerPlayerCollisionHandler` owns its own Presentation-layer feedback (impact particle + Cinemachine impulse) directly, and only calls back into Application (`onObstacleHit` → `RunnerGameFlowService.TriggerGameOver`) for the state transition. This keeps Application free of Unity types while letting collision feedback stay colocated with the collision code.
- `ImpactEffectSpawner` lives under `Presentation/Camera/` (grouped with camera-adjacent feedback) rather than a dedicated `Effects/` code folder, since the spec's Presentation subfolder list doesn't include one; the scene hierarchy still uses an `Effects/ImpactEffectContainer` node.
- No `ObstacleMarker` script exists — obstacles are identified purely by the `Obstacle` tag (checked in `RunnerPlayerCollisionHandler`), since a marker component would carry no behaviour.
- `MovingObstacle` (`Presentation/Environment/`) is the one exception to "obstacles are plain tagged cubes": it requires a `Rigidbody` (forced kinematic, no gravity, in `Awake`) and sweeps `x` sinusoidally in `FixedUpdate` via `MovePosition`, so the moving collider still participates correctly in physics collision (a `Transform`-only move on a collider with no `Rigidbody` is a discouraged/unreliable pattern for anything that needs to generate collision events against a dynamic body).

## Implementation Overview

- **Forward movement**: `RunnerPlayerController.FixedUpdate()` calls `Rigidbody.MovePosition` once per physics step, advancing `+Z` at `Config.ForwardSpeed * Time.fixedDeltaTime` and writing the smoothed `X`. Rigidbody uses `Interpolate` + `Continuous Dynamic`, gravity on, rotation frozen (`FreezeRotationX/Y/Z`) so the capsule never tips.
- **Input — discrete 3-lane swipes (Subway Surfers style)**: `RunnerInputReader` reads `Input.touchCount`/`Input.GetTouch(0)` when a touch is present, otherwise the mouse. Per drag, it records the press-start screen X and, every frame while the pointer is held, checks the normalized delta *from that start point* against `Config.SwipeThreshold`; the first frame that crosses the threshold fires a one-shot `SwipeDetected(±1)` event and latches (`_swipeConsumedThisDrag`) so the rest of that same drag can't fire again — the player must release and press again for a second lane change, matching a real swipe gesture rather than continuous dragging. `Update()` itself is thin: it reads a small `PointerState` (touch-or-mouse, unified) via `ReadPointerState()`, then delegates to `UpdateDragState()`/`TryDetectSwipe()` — kept as separate single-purpose methods (guard-clause style, no nested `if`/`else if` chains) so the touch-vs-mouse source selection, press/release tracking, and threshold-crossing logic can each be read and changed independently.
- **Lane movement**: `RunnerPlayerController` subscribes to `SwipeDetected` and calls `MoveToLane(direction)`, which computes a desired lane index and, if it's within `[-1, 0, 1]`, sets `_laneIndex`/`_targetX = _laneIndex * Config.LaneOffset`. Starting in the center lane, one right swipe moves to the right lane; from the left lane, two right swipes are needed to cross through center and reach the right lane (each swipe only steps one lane, never skips). `FixedUpdate()` then moves the Rigidbody's X toward `_targetX` with `Mathf.SmoothDamp` using `HorizontalSmoothTime` — a smoothed slide between lanes, not an instant teleport.
- **Edge bump (Subway Surfers–style)**: if `MoveToLane` computes a desired lane outside `[-1, 1]` (swiping further while already at the leftmost/rightmost lane), the player doesn't move — instead `TriggerEdgeBump()` halves forward speed (`Config.EdgeBumpSpeedMultiplier`) and fires a light Cinemachine impulse (`Config.EdgeBumpCameraShakeForce`, weaker than an obstacle hit) as a "bump" reaction. The penalty tracks *distance traveled* rather than wall-clock time or literal obstacle collisions — `FixedUpdate` decrements `Config.EdgeBumpRecoveryDistance` by each frame's forward step and clears the penalty once it reaches zero — so it reads as "recovers after ~2 obstacle rows" (default recovery distance ≈ 2× the level's row spacing) without coupling the player controller to the actual `Obstacle` GameObjects in the scene.
- **Swipe up/down (jump/slide) — intentionally not implemented**: the Supercyan pack's animation set (`common_people@jump-up/-float/-down`, `@run`, `@walk`, `@idle`, `@pickup`, `@wave`, `@backwards-run/-walk`, `@t-pose`) and the `FreeAnimations.controller` include a full jump cycle but **no slide/crouch/duck clip**, and the controller's `Crouch` blend parameter is referenced by the movement `BlendTree` but never actually defined. Per an explicit "only add this if both animations exist" instruction, the feature was left out entirely rather than half-implemented (e.g. jump-only).
- **Visual tilt**: applied only to `VisualRoot` (a child of the physics root), never the collider. Tilt target is derived from the SmoothDamp's own velocity output (`_horizontalVelocityRef`, normalized against a max-lateral-speed estimate of `Config.LaneOffset / Config.HorizontalSmoothTime`, i.e. one lane-width per smooth-time), so it reflects actual lateral motion rather than raw pointer position, and eases back to 0 via `Mathf.Lerp` once movement stops.
- **Character package integration**: the player prefab (`Assets/_Project/Features/RunnerPrototype/Assets/Prefabs/Player/RunnerPlayer.prefab`) is `PlayerRoot` (Rigidbody, CapsuleCollider, `RunnerPlayerController`, `RunnerPlayerCollisionHandler`, `CinemachineImpulseSource`) with two children: `CameraTarget` and `VisualRoot`, which contains an unmodified instance of the supplied `animal_people_wolf_1` prefab (Animator + `FreeAnimations.controller` already wired by the asset). `RunnerPlayerController` drives `MoveSpeed` (1 while running, 0 otherwise) and `Grounded` (always true — no jump in scope).
- **Collectibles**: `CollectibleView` on the trigger root; a separate `Visual` child (mesh) and `CollectionParticle` child (burst `ParticleSystem`) so the DOTween sequence can move/scale/rotate the visual without touching the trigger collider. On trigger: collider disabled immediately, score callback invoked once (`_collected` guard), particle detached and played, then a ~0.25s DOTween sequence (`DOLocalMoveY` + `DOPunchScale` + `DOLocalRotate`) before deactivating.
- **Score service / ScoreView**: `ScoreService` (Application) wraps the `Score` domain value object and fires `ScoreChanged`. `ScoreView` (Presentation) subscribes via the Composition Root, updates the TMP label, and plays a `DOPunchScale` on the text's own transform, always killing the previous punch tween and restoring the cached original scale first so rapid pickups can't compound/distort the label.
- **Obstacles**: plain cubes tagged `Obstacle` with a non-trigger `BoxCollider`. `RunnerPlayerCollisionHandler.OnCollisionEnter` checks the tag, guards against re-entry with `_isActive`, plays the impact particle + Cinemachine impulse immediately, then invokes the Application callback once. A subset (`Obstacle_R10/R12/R14_Moving`) additionally carry `MovingObstacle`, which sweeps them across all three lanes (`amplitude = LaneOffset`, centered on the middle lane) at increasing speed the further down the route they are (`2.0 → 2.6 → 3.2`), forcing timing-based dodges rather than a fixed lane choice.
- **Impact particles**: `ImpactEffectSpawner` (on `Effects/ImpactEffectContainer`) instantiates the `ImpactBurst` prefab at the contact point/normal and self-destroys it after `duration + startLifetime`.
- **Cinemachine camera**: one `CinemachineVirtualCamera` (Body = Transposer, offset `(0, 3, -6)`, `LockToTargetNoRoll`, low X damping for minimal lateral lag) following/looking at the player's `CameraTarget`. `CinemachineBrain` lives on `Main Camera`.
- **Cinemachine Impulse**: `CinemachineImpulseSource` lives on the **Player** (the object generating the hit); `CinemachineImpulseListener` lives on the **CinemachineVirtualCamera**, not the physical Camera — in Cinemachine 2.10.3, `CinemachineImpulseListener` is itself a `CinemachineExtension`, meaning it must sit on (or be added as an extension of) a vcam to connect; placing it on the output Camera is a common mistake that leaves it inert and logs `"CinemachineExtension requires a Cinemachine Virtual Camera component"`. This was caught via the Console during implementation and fixed. The same `CinemachineImpulseSource` is shared by two call sites, each scaling it via `GenerateImpulse(force)` rather than the parameterless `GenerateImpulse()`: `RunnerPlayerCollisionHandler` uses `Config.CameraShakeIntensity` (a full, pronounced shake on an obstacle hit) and `RunnerPlayerController.TriggerEdgeBump()` uses the smaller `Config.EdgeBumpCameraShakeForce` (a light nudge on an edge bump) — one impulse definition, two distinct feels, driven entirely by config rather than by two separate `CinemachineImpulseSource` components.
- **Start Menu**: the run no longer auto-starts. `RunnerCompositionRoot.Awake()` leaves `RunnerGameFlowService` in `Ready` and calls `StartMenuView.ShowImmediate()`; only the in-scene "PLAY" button (`StartMenuView.Initialize` callback) hides the menu and calls `StartRun()`. `StartMenuView` mirrors `GameOverView`'s DOTween fade/scale pattern (same panel duplicated from `GameOverPanel`, then re-skinned). One gotcha found and fixed here: Unity does not guarantee `Awake()` order across different GameObjects, so `StartMenuView`'s own `Awake()` (which caches `_originalScale` from the RectTransform) can run *after* `RunnerCompositionRoot.Awake()` calls `ShowImmediate()` — the fix is defaulting the cached field to `Vector3.one` at declaration instead of relying on `Awake()` having already run.
- **Game Over**: `RunnerGameFlowService` (Application) is the single source of truth for `Ready → Running → GameOver`, guarding both transitions so `TriggerGameOver()` only succeeds once. `GameOverView` shows the panel via a DOTween `CanvasGroup` fade + `RectTransform` scale (`Ease.OutBack`), enabling interaction only in `OnComplete`, and resets to a hidden, non-interactable, punch-scaled-down state on `HideImmediate()` (called on `Awake` and again by the Composition Root).
- **Restart**: `SceneReloader` (Infrastructure) sets `Time.timeScale = 1` and calls `SceneManager.LoadScene(activeScene.buildIndex)`. `GameOverView` guards the button with a `_restartRequested` latch so repeated clicks can't fire multiple reloads, and disables the button immediately on click. Because the scene fully reloads, restarting returns the game to `Ready` behind the start menu, not straight back into a running state.

## How to Run
Open `Assets/_Project/Scenes/Gameplay/RunnerGameplay.unity` and press Play. A "RUNNER 3D" start menu appears first (`Ready` state, player idle, input disabled) — press the on-screen **PLAY** button to begin the run (`Ready → Running`).

## Controls
- **Editor**: swipe (click, drag past the threshold, release) left or right with the mouse.
- **Android**: swipe left or right with a finger.

Movement snaps between 3 fixed lanes (left / center / right), one lane per swipe — Subway Surfers style. From the left lane, two right-swipes are needed to reach the right lane (each swipe steps exactly one lane and cannot skip over the center lane in a single gesture). Swiping past the outer lanes triggers an **edge bump**: the lane doesn't change, forward speed is temporarily halved, and a light camera shake plays; speed recovers automatically after covering roughly 2 obstacle rows' worth of distance.

Swipe-up/swipe-down (jump/slide) is **not implemented** — the character asset pack has jump animations but no slide/crouch animation, so per the "only add this if both exist" requirement it was left out.

## Build
- Build target: **Android** (project was already configured for Android; not changed for this feature).
- Scene added to *File → Build Settings*: `Assets/_Project/Scenes/Gameplay/RunnerGameplay.unity` (index 0, the only scene in the build).
- A development APK was built successfully to `Builds/Android/RunnerPrototype.apk` (~36 MB, 0 build errors, 0 build warnings) via the MCP `manage_build` tool. No missing-SDK/NDK/JDK or license blockers were encountered in this environment.
- That APK predates the start menu, edge bump, moving obstacles, and extended level (see below) — it has not been rebuilt since those were added. Rebuild before relying on it for playtesting.

## Configuration
ScriptableObject asset: `Assets/_Project/Features/RunnerPrototype/Assets/Data/RunnerGameplayConfig.asset` (type `RunnerGameplayConfig`, `Infrastructure/Configuration/`). All fields are validated with `[Min]`/`[Range]` + `OnValidate` clamping. Current values (defaults chosen at authoring time, not further hand-tuned in a playtest pass — see Notes):

| Field | Value |
|---|---|
| Forward Speed | 6 |
| Swipe Threshold | 0.08 (fraction of screen width) |
| Lane Offset | 3 (world-unit distance of the left/right lanes from center) |
| Horizontal Smooth Time | 0.12 |
| Max Tilt Angle | 20° |
| Tilt Smooth Speed | 8 |
| Camera Shake Duration | 0.3s |
| Camera Shake Intensity | 1 |
| Game Over Transition Duration | 0.35s |
| Score Punch Scale | 0.3 |
| Score Punch Duration | 0.25s |
| Collectible Animation Duration | 0.25s |

The `CinemachineImpulseSource` on the Player is configured separately (impulse duration 0.15s, sustain 0.1s, decay 0.2s, amplitude gain 1, default velocity `(0, -1.5, 0)`) to match the shake duration/intensity above.

## Notes

- **Technical decisions**:
  - Player collider is a `CapsuleCollider` (radius 0.3, height 1.4, center `(0, 0.7, 0)`), sized up from the character package's own `SimpleMovement` sample collider (radius 0.1, height 1) for more forgiving/reliable obstacle collision at runner speed.
  - The road is a single scaled default Plane (`Ground`, 10 × 110 units) with two full-length `BoxCollider` boundary walls at `x = ±5`; the player's own horizontal clamp (`±3`) keeps it well inside those walls under normal play.
  - The spec's route is finite by design (no procedural generation), so a tagged `Obstacle` wall (`EndOfRouteWall`) was added just past the last collectible cluster (`z = 92`, ground ends at `z = 105`) — without it, a player who dodges every obstacle would run off the end of the finite ground and fall indefinitely, which would violate the "no falling through the ground" requirement. This was found and fixed during playtesting.
  - `RunnerPlayerController.MoveToLane` and `RunnerPlayerCollisionHandler.HandleObstacleContact` / `CollectibleView.HandleTriggerEnter` are `public` specifically so PlayMode tests can drive them directly without simulating real input/physics events — a deliberate, minimal testability accommodation, not a general API.
  - **Movement was reworked from continuous free-drag to discrete 3-lane swipes** (left / center / right) at the user's explicit request, superseding the original "continuous, not lane-based" instruction. All 17 collectibles and 8 obstacle rows (plus the `EndOfRouteWall`) were repositioned to align exactly to the 3 lane X-positions (`-LaneOffset`, `0`, `+LaneOffset`) — with continuous movement the old positions were fine, but under lane-snapping anything off-lane would be permanently uncollectible or (for obstacles) unreachable/undodgeable. The obstacle pattern includes two "forced-lane" rows (both side lanes blocked, or center+one side blocked) specifically to exercise multi-swipe lane crossing during play.
  - Camera framing was re-checked for the portrait switch by forcing `Camera.aspect` to `1080/1920` and calling `WorldToViewportPoint` on the outer-lane edges at the nearest and farthest obstacle rows — both landed comfortably inside the `[0,1]` viewport range with the original landscape-tuned Cinemachine offset/FOV, so no camera retuning was needed.
- **Verified after initial writeup** (both were flagged as open items in the first pass and have since been confirmed):
  - **Restart flow**: manual interactive probing of `SceneManager.LoadScene` through this session's automation tooling was too timing-sensitive to trust (real Editor time between tool calls was unpredictable and occasionally caused stray duplicate objects — see below), so the flow was instead verified with a dedicated `[UnityTest]` PlayMode test (`RestartFlowPlayModeTests`) that loads the real scene, triggers Game Over, invokes the Restart button's actual `onClick`, and asserts the score resets to `0`, `RunnerGameFlowService.State` returns to `Running`, and no extra scenes are left loaded. **Passes.**
  - **Tilt direction**: rather than rely on screenshots (unreliable in this unfocused-Editor session), the sign convention was confirmed two ways: (1) an isolated, real gameplay reading captured `playerX` still converging toward `+3` (moving right) paired with a simultaneous `VisualRoot` tilt of `-3.66°` on the same frame; (2) pure geometry — applying that same negative-Z rotation to a reference transform shows the local "up" vector leans toward `+X` (screen-right) and local "right" dips toward `-Y`, i.e. the character banks into the turn correctly, matching the spec's "rightward movement visually tilts the character toward the right." No code change was needed.
  - Mid-session, an earlier round of manual `SceneManager.LoadScene` restart testing left 5 orphaned duplicate objects loose at the scene root (a stray `Main Camera` with `CinemachineImpulseListener` still on it, a duplicate `CinemachineVirtualCamera`, `ImpactEffectContainer`, `Systems`, and `ScoreText`) alongside the correctly-parented originals. This caused a real, transient "2 audio listeners" warning and a `CinemachineExtension requires a Cinemachine Virtual Camera component` error from the stray camera. Found via the Console, all 5 orphans were deleted, the composition root's references were re-verified as still pointing at the correct (properly-parented) objects, and a follow-up Android build confirmed 0 errors / 0 warnings and a single `AudioListener` at runtime.
- **Remaining limitation**: Screenshots captured through the MCP camera tool during this session repeatedly returned a stale/cached Game View frame while the Unity Editor window was unfocused; this is why the two items above were verified through state inspection and a dedicated automated test rather than pixel-level screenshots.

## Demo

![Demo](Gif/Demo.gif)