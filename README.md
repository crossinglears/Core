# Crossing Lears Core
<img src="https://github.com/user-attachments/assets/635f1ca3-ccb6-4b10-b55e-514b805c2d91" align="right" width="250">

Core package for Crossing Lears tools.

`Crossing Lears Core` is the shared foundation package behind the rest of the Crossing Lears ecosystem. It combines runtime helpers, editor utilities, inspector attributes, context actions, and a modular toolbox window that discovers tabs automatically.

Current package metadata:

- Package name: `com.crossinglears.core`
- Display name: `Crossing Lears Core`
- Version: `0.4.0`
- Minimum Unity version declared in `package.json`: `6000.0`

## What This Package Includes

This package is split into two main parts:

- Runtime utilities you can use in builds
- Editor-only tools that improve workflows inside Unity

At a high level it includes:

- Inspector attributes such as `[Button]` and `[ReadOnly]`
- Runtime helpers such as `Chance`, `CSharp_Utilities`, extension methods, object dictionaries, and pooling
- Utility MonoBehaviours such as `PlatformDependent`, `OnEnableScript`, `StartState`, and `VersionDisplay`
- A toolbox window available from `Crossing Lears/Toolbox`
- Editor tabs for scenes, packages, todo tracking, clipboard snippets, commands, selection helpers, text utilities, transform tools, and more
- Context menu helpers such as `AutoGrab` and `MoveToTop`

## Installation

### Unity Package Manager

1. Open `Window > Package Manager`
2. Click `+`
3. Select `Add package from git URL...`
4. Paste:

```text
https://github.com/crossinglears/Core.git#main
```

### Short Windows Command Examples

If you want command examples kept short for Windows environments, use small batches like these.

Command Prompt:

```bat
cd /d C:\Path\To\YourProject
```

```bat
git clone https://github.com/crossinglears/Core.git
```

PowerShell:

```powershell
Set-Location 'C:\Path\To\YourProject'
```

```powershell
git clone https://github.com/crossinglears/Core.git
```

For Unity projects, the normal and recommended path is still installing through the Package Manager using the Git URL above.

## Toolbox Window

Open the toolbox from:

```text
Crossing Lears/Toolbox
```

The toolbox is an extensible editor window built around `CL_Window` and `CL_WindowTab`.

Key behavior:

- Tabs are discovered automatically by reflection
- `General` is always pinned to the first slot
- Tab order is saved in `EditorPrefs`
- Tabs can be hidden from the `General` tab
- A search field filters the visible tab list
- Clicking a tab title can ping its backing script

This makes the toolbox useful both as a shipped editor utility and as a container for project-specific internal tabs.

## Built-In Toolbox Tabs

The current package contains a larger set of tabs than the old README listed. Not all of them are equally mature, but they are part of the package surface.

### General

Main landing tab for the toolbox.

- Updates the toolbox package from `https://github.com/crossinglears/Core.git#main`
- Lets you enable, disable, and reorder visible tabs
- Includes a helper to generate a new `CL_WindowTab` script
- Contains a feedback field and website button

### Packages

Package shortcut manager for projects that frequently pull Git-based packages.

- Ships with a preset list of Crossing Lears packages
- Lets you store your own favorite packages locally on the machine
- Supports add, edit, delete, reorder, install, and reset
- Favorites are saved outside the Unity project at:

```text
%AppData%\CrossingLears\FavoritePackages.json
```

Included default package shortcuts:

- Directory
- Toolbar
- Latest Menu
- UI
- Audio
- Haptic Feedback
- Auto RuleTile
- Paid entry: Editor Calendar

### Scenes

Scene browser for fast navigation inside larger projects.

- Finds scenes with `AssetDatabase.FindAssets("t:Scene")`
- Groups them by parent folder name
- Supports fuzzy filtering
- Highlights currently open scenes
- Can open scenes normally or additive
- Can close open scenes
- Can add or remove scenes from Build Settings
- Supports right-click copy for scene name or path
- Can ping the scene asset in the Project window

### Todo

Simple per-project checklist stored in source files so teams can share it.

- Add tasks
- Edit tasks
- Mark tasks complete
- Reassign finished tasks back to active
- Delete active or completed tasks
- Reorder active tasks

Saved file:

```text
Assets/Editor/Development Files/Todo.json
```

### Clipboard

Editor clipboard and quick-action lists.

- Organize multiple named lists
- Reorder lists in a separate arrange window
- Store reusable entries with titles and messages
- Save clipboard presets into the project

Supported entry purposes:

- `Clipboard`
- `ResizeObject`
- `SnapInterval`
- `PrefabReplace`
- `Log`

Example uses:

- Copy repeated text snippets into the system clipboard
- Apply move snap values in the editor
- Resize the selected transform from a preset
- Replace selected objects with a prefab while keeping parent, local transform, and sibling index

Saved file:

```text
Assets/Editor/Development Files/Clipboard.json
```

### Other Included Tabs

The package also currently includes these tabs:

- `Auto Save`
- `Command`
- `Core Methods`
- `Editor`
- `Level Design`
- `Object Snap`
- `Paths`
- `Pivot`
- `Play Mode`
- `Random Prefab`
- `Renderer`
- `Script`
- `Selection`
- `Snippets`
- `Text Edit`
- `Transform`

Some of these are clearly workflow-specific, and some are marked or structured as in-progress tooling. They are still part of the shipped toolbox and can be enabled, disabled, or extended like any other tab.

## Inspector Attributes

### `[Button]`

Exposes a method as a clickable button in the inspector.

Supported forms:

```csharp
[Button]
```

```csharp
[Button("Custom Label")]
```

Typical use cases:

- Trigger editor-time refresh methods
- Rebuild caches
- Run setup helpers without writing custom inspectors

### `[ReadOnly]`

Draws a field as non-editable in the inspector.

Typical use cases:

- Debug state
- Generated values
- Runtime diagnostics

### Other Attributes Present In The Package

Additional attributes and drawers currently included:

- `CL_CommandAttribute`
- `CL_CommandHolderAttribute`
- `InspectorTextAttribute`
- `ShowSpriteAttribute`

## Context Menu Utilities

### AutoGrab

Context menu entries:

- `CONTEXT/Component/AutoGrab (GameObject)`
- `CONTEXT/Component/AutoGrab (Scene)`

Purpose:

- Automatically fills empty public or serialized references
- Useful during setup and scene wiring
- Undo-friendly through Unity undo history

### MoveToTop

Context menu entry:

- `CONTEXT/MonoBehaviour/MoveToTop`

Purpose:

- Moves a `MonoBehaviour` to the top of the component list on a GameObject

## Runtime Utilities

### `Chance`

Utility methods for random logic.

Includes:

- Percentage checks for `float`
- Percentage checks for `int`
- `chance()` coin-flip helper
- Weighted `RandomEnum<T>(params float[] weights)`
- `RandomEnum<T>()`
- Rounded randomization helper through `round(float input)`
- Range helpers for `Vector2`, `Vector3`, `Vector2Int`, and `Vector3Int`

### `CSharp_Utilities`

General-purpose helper collection referenced by the older docs and package layout.

The package positions this as a home for convenience methods such as:

- `Shuffle`
- `GetRandomFromList`
- `RandomEnum`

### `CL_Extensions`

Includes extension helpers for Unity objects and vectors.

Object helpers include:

- `instantiate(...)`
- `destroy(...)`
- `open()`
- `close()`
- `destroyChildObjects()`
- `closeChildObjects()`
- `alphaChange(...)`

Vector helpers include:

- `SnapToVector2(...)`
- `SnapToInt(...)`
- `IsInRange(...)`

### `CL_Logger`

Floating text helpers for quick visual feedback.

Included helpers:

- `CreateFloatingText2D(...)`
- `CreateFloatingTextUI(...)`

These create temporary TextMeshPro objects that rise over time and destroy themselves after their duration ends.

### `CoroutineAsync`

Runtime coroutine helper utility included in the package.

### `RemoveDuplicatesExtension`

Collection helper extension included in the runtime scripts.

## Runtime Components

### `PlatformDependent`

Used to limit objects by target build platform.

Behavior:

- Holds allowed target platforms
- Participates in build preprocessing
- Excludes GameObjects that do not match the active build target
- Includes an inspector button to populate platform entries

This is useful for platform-specific content, conditional assets, or scene variants.

### `OnEnableScript`

Exposes enable and disable style behavior through the component workflow.

### `StartState` and `StartStateController`

Utility components for controlling initial object state.

The controller includes inspector buttons to trigger updates directly.

### `CloseOnDisable`

Included runtime GameObject helper component.

### `VersionDisplay`

Reusable runtime component for version presentation.

### `SmoothScrollRect`

Custom `ScrollRect` implementation included in the package.

### `RadialMenu`

Runtime radial menu component.

## Base Classes

### `Pool<T>`

Generic pooling base class for `Component` types.

### `NamedPool<T>`

Named variation of the generic pool structure.

### `ObjectDictionary<T>` and `ObjectLibrary<T>`

Asset lookup helpers designed around editor-time population and runtime access patterns.

Notable details:

- Can search assets with `AssetDatabase`
- Includes inspector buttons such as relaunch and duplicate cleanup
- Intended for building reusable object registries

## Editor Helpers

The package also contains a number of editor infrastructure classes used by the toolbox and inspector experience.

Examples:

- `CL_Editor`
- `CL_Design`
- `EditorExtensions`
- `ExtensionTools`
- `TempInputWindow`
- `CoreFeedbackSender`
- `ZipScriptedImporter`
- `CL_EditorEvents`

These are mostly internal support utilities but are useful to know about if you extend the package.

## Persistence And Generated Files

The package stores data in both project-local files and machine-local files depending on purpose.

Project-local files:

- `Assets/Editor/Development Files/Todo.json`
- `Assets/Editor/Development Files/Clipboard.json`

Machine-local files:

- `%AppData%\CrossingLears\FavoritePackages.json`

Project-local files are suitable for collaboration if committed to source control. Machine-local files are intended for user-specific editor preferences and package shortcuts.

## Extending The Toolbox

The toolbox is meant to be extendable.

To add a new tab:

1. Create a class that inherits from `CL_WindowTab`
2. Implement `TabName`
3. Implement `DrawContent()`
4. Place the script in an editor assembly or editor folder
5. Reopen the toolbox or trigger a script reload

The `General` tab also contains a helper button named `Create New Menu` that scaffolds a basic tab script for you.

Tabs are discovered automatically, so no manual registration step is required.

## Notes

- The toolbox currently contains both general-purpose utilities and project/workflow-specific tabs
- Some tabs are marked as work in progress
- The package already exposes more functionality than the original short README described
- The Package Manager Git URL is the primary installation route

## Website

```text
https://crossinglears.com/
```
