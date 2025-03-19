# Crossing Lears - Core

<img src="https://github.com/user-attachments/assets/635f1ca3-ccb6-4b10-b55e-514b805c2d91" align="right" width="250">

The **Core** package contains essential tools and extensions used across various **Crossing Lears** tools, designed to enhance and streamline game development in Unity.

## Why Use Crossing Lears Tools?
Crossing Lears tools aim to make **Unity game development more efficient** by providing useful utilities, extensions, and workflow improvements.

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
- **[Button]** – Exposes a method as a button in the **Inspector**
  - ` [Button] `
  - ` [Button("Custom Label")] `
- **[ReadOnly]** – Makes a field **readonly** in the **Inspector**

### Context Menus
- **Autograb** – Fills all empty public/SerializedFields in the selected component  
  _Right-click on a component → Select **Autograb**_  
  _Undo changes with **CTRL+Z**_
- **MoveToTop** – Moves a MonoBehaviour to the top of the component list  
  _Right-click on a MonoBehaviour → Select **MoveToTop**_

### Extensions
- `Instantiate(Object)` – Simplifies object instantiation
- `Destroy(Object)` – Streamlines object destruction
- `Close(GameObject) / Open(GameObject)` – Easily toggle GameObject visibility

### MonoBehaviours
- **PlatformDependent** – Restricts a GameObject to specific platforms

---

More features and improvements coming soon.

