# Core
<img src="https://github.com/user-attachments/assets/635f1ca3-ccb6-4b10-b55e-514b805c2d91" align="right" width="250">

The core component of Crossing Lears tools

This package contains tools and extensions that will mostly be used by different tools created by [Crossing Lears](https://crossinglears.com/)

### Why you should use the Crossing Lears tools?
Crossing Lears creates tools that aims to make game development in Unity Engine more convenient.

## Installation
### Unity 2019.2 or Later (via Unity Package Manager)
1. Open **Window** → **Package Manager**
2. Click **Add package from git URL**
3. Paste the following URL:
   ```
   https://github.com/crossinglears/Core.git#main
   ```

---

## Features & Contents
### Attributes
- `[Button]` – Exposes a method as a button in the **Inspector**
  - ` [Button] `
  - ` [Button("Custom Label")] `
- `[ReadOnly]` – Makes a field **readonly** in the **Inspector**

### Context Menus
- `Autograb` – Fills all empty public/SerializedFields in the selected component  
  _Right-click on a component → Select **Autograb**_  
  (Undo changes with **CTRL+Z**)
- `MoveToTop` – Moves a MonoBehaviour to the top of the component list  
  _Right-click on a MonoBehaviour → Select **MoveToTop**_

### Utilities
- `Chance` - simple checkers for easy implementation of random events.
- `CL_Logger` - Has a `CreateFloatingText2D()` and `CreateFloatingTextUI()` that will help you display texts easily.

### Extensions
- `Instantiate(Object)` – Simplifies object instantiation
- `Destroy(Object)` – Streamlines object destruction
- `Close(GameObject) / Open(GameObject)` – Easily toggle GameObject visibility
- `CSharp_Utilities` - Contains helper tools for C#
   - `Shuffle`, `GetRandomFromList`, `RandomEnum`
  
### Base Classes
- `Pool` – Simple gameobject pool

### Windows
- `CL_Window` - Contains tools and settings menu. You can easily add your own menu here by creting a `CL_WindowTab` and this window will find it on its own.
   - `General`
   - `Packages` - Saves your frequently used git packages.
   - `Scenes` - Finds all scenes and display them in one tab for easier navigation.
   - `Todo` - Contains a simple checklist that is saved in your project (for collaboration)

### MonoBehaviours
- `PlatformDependent` – Restricts a GameObject to specific platforms
- `OnEnableScript` – Exposes gameobject OnEnable and OnDisable events in the editor

---

More features and improvements coming soon.

