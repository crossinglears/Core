# Core
<img src="https://github.com/user-attachments/assets/635f1ca3-ccb6-4b10-b55e-514b805c2d91" align="right" width="250">

The core component of Crossing Lears tools

This package contains tools and extensions that will mostly be used by different tools created by Crossing Lears

### Why you should use the Crossing Lears tools?
Crossing Lears creates tools that aims to make game development in Unity Engine more convenient.

## Installation
### (For Unity 2019.2 or later) Through Unity Package Manager
 * MenuItem - Window - Package Manager
 * Add package from git url
 * paste ```https://github.com/crossinglears/Core.git#main```

# Features/Contents
- **Attributes**
- **Context Menus**
- **CustomEditors**
- ... and more (soon)

## Interfaces

## Monobehaviours
> **PlatformDependent** - Add to gameobject to limit what platform if should be allowed in.
- ... and more (soon)

## Context Menus
> **Autograb**
-- Automatically fills all empty public/SerializedFields in the component selected
-- Accessed by right clicking a Component
-- Can undo changes with CTRL+Z

> **MoveToTop**
-- Moves a MonoBehaviour to the first item in the list of the components in a gameobject
-- Accessed by right clicking a MonoBehaviour

## Extensions
-- Instantiate (Object)
-- Destroy (Object)
-- Close and Open (GameObject)

## Attributes
[Button]
[Button(string)]
-- Used on methods to expose a button that executes them in the inspector

[ReadOnly]
-- Makes a field unedittable in the inspector.
-- Used on fields
