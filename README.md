# Crossing Lears Core
<img src="https://github.com/user-attachments/assets/635f1ca3-ccb6-4b10-b55e-514b805c2d91" align="right" width="250">

Core package for Crossing Lears tools.

`Crossing Lears Core` is the shared foundation package behind the Crossing Lears ecosystem. It combines runtime helpers, editor utilities, inspector attributes, context actions, and a modular toolbox window that discovers tabs automatically.

Current package metadata:

- Package name: `com.crossinglears.core`
- Display name: `Crossing Lears Core`
- Version: `0.4.1`
- Minimum Unity version: `6000.0`
- Dependency: `com.unity.ugui` (Unity UI / TextMesh Pro)

Offline documentation: `Runtime/Documentation (Core).txt`  
Third-party notices: `Third-Party Notices.txt`  
Demo scene: `Demo/CrossingLearsCoreDemo.unity`

## What This Package Includes

- Runtime utilities you can use in builds
- Editor-only tools that improve workflows inside Unity
- Inspector attributes such as `[Button]` and `[ReadOnly]`
- Utility MonoBehaviours such as `PlatformDependent`, `OnEnableScript`, `StartState`, and `VersionDisplay`
- A toolbox window available from `Window/Crossing Lears Core/Toolbox`
- Context menu helpers such as `AutoGrab` and `MoveToTop`

## Installation

### Unity Asset Store

1. Download and import **Crossing Lears Core** from the Asset Store / Package Manager.
2. Confirm `com.unity.ugui` is present in your project (default in most Unity 6 installs).
3. Open `Window > Crossing Lears Core > Toolbox`.
4. Open `Assets/Crossing Lears Core/Demo/CrossingLearsCoreDemo.unity` to review the demo.

### Unity Package Manager (Git)

1. Open `Window > Package Manager`
2. Click `+`
3. Select `Add package from git URL...`
4. Paste:

```text
https://github.com/crossinglears/Core.git#main
```

## Toolbox Window

Open from:

```text
Window/Crossing Lears Core/Toolbox
```

- Tabs are discovered automatically by reflection
- `General` is always pinned to the first slot
- Tab order is saved in `EditorPrefs`
- Tabs can be hidden from the `General` tab

### Feedback disclosure

The General tab optional **Send Feedback** button transmits only the text you typed to the Crossing Lears publisher endpoint. Nothing is sent automatically on import or load. The website button asks for confirmation before opening a browser.

## Demo Scene

`Demo/CrossingLearsCoreDemo.unity` showcases:

- `PlatformDependent`
- `StartState` / `StartStateController`
- `[Button]` / `[ReadOnly]` via `DemoAttributeShowcase`
- `OnEnableScript`
- `SmoothScrollRect`
- `RadialMenu`

Sample prefabs are under `Demo/Prefabs/`. Enter Play mode to see the on-screen `DemoGuide` that explains each feature and its setup steps.

## Inspector Attributes

### `[Button]` / `[Button("Custom Label")]`

Exposes a method as a clickable button in the Inspector.

The attribute is `CrossingLears.ButtonAttribute` in:

- Assembly name: `Crossing Lears`
- Assembly definition GUID: `b5b95d56ce8ae8a4d86d5517a33f4f18`
- Assembly definition reference: `GUID:b5b95d56ce8ae8a4d86d5517a33f4f18`

While Core is installed, its editor bootstrap adds `CROSSINGLEARS` to every
NamedBuildTarget. Removing the UPM package or deleting
`Assets/Crossing Lears Core` through Unity removes the define.

Consumer usage:

```csharp
#if CROSSINGLEARS
[CrossingLears.Button]
#else
[ContextMenu("Run Action")]
#endif
private void RunAction()
{
}
```

Custom label:

```csharp
#if CROSSINGLEARS
[CrossingLears.Button("Custom Label")]
#else
[ContextMenu("Custom Label")]
#endif
private void RunCustomAction()
{
}
```

When `CROSSINGLEARS` is active, the consumer assembly must also reference the
Core runtime assembly in its `.asmdef`:

```json
{
  "references": [
    "GUID:b5b95d56ce8ae8a4d86d5517a33f4f18"
  ]
}
```

Do not include that Core assembly reference when shipping the consumer as a
standalone Asset Store package. Without Core, `CROSSINGLEARS` is not defined,
so the consumer compiles with its `[ContextMenu]` fallback.

### `[ReadOnly]`

Draws a field as non-editable in the Inspector.

## Context Menu Utilities

- `CONTEXT/Component/AutoGrab (GameObject)` / `(Scene)` — fill empty serialized references
- `CONTEXT/MonoBehaviour/MoveToTop` — move a component to the top of the list

## Runtime Utilities And Components

Helpers: `Chance`, `CL_Extensions`, `CSharp_Utilities`, `CoroutineAsync`, pooling, object dictionaries, `CL_Logger`.

Components: `PlatformDependent`, `StartState`, `StartStateController`, `OnEnableScript`, `VersionDisplay`, `SmoothScrollRect`, `RadialMenu`.

See `Runtime/Documentation (Core).txt` for the full reference.

## Extending The Toolbox

1. Create a class that inherits from `CL_WindowTab`
2. Implement `TabName` and `DrawContent()`
3. Place it in an Editor assembly or Editor folder
4. Reopen the toolbox or reload scripts

## Website

```text
https://crossinglears.com/
```
