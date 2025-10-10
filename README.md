# Project Initializer

A Unity utility that helps initializing new Unity projects.

## Features

This package provides two main features:

1.  **Project Initialization:** A tool to set up a standardized folder structure and add a predefined set of useful packages to your project.
2.  **Embedded Package Creator:** A tool to easily create new embedded Unity packages with the correct structure and files.

## How to Use

### Project Initialization

To initialize your project, go to the Unity menu and select `Tools > Project Setup > Initialize Project`.

This will do the following:

*   **Create a standard folder structure:**

    ```
    Assets/
    ├── _Project/
    │   ├── Scenes/
    │   └── Timeline/
    ├── Art/
    ├── Audio/
    ├── Materials/
    ├── Models/
    ├── Prefabs/
    ├── Scripts/
    ├── Settings/
    ├── Shaders/
    └── Textures/
    ```

*   **Add and resolve the following packages:**
    *   `com.unity.multiplayer.playmode`
    *   `com.unity.multiplayer.tools`
    *   `com.unity.netcode.gameobjects`
    *   `com.unity.postprocessing`
    *   `VContainer` (from a git URL)

### Embedded Package Creator

To create a new embedded Unity package, go to the Unity menu and select `Tools > Project Setup > Create Embedded Unity Package`.

This will open a window where you can define the metadata for your new package, such as:

*   Package Name (e.g., `com.mycompany.mypackage`)
*   Version
*   Display Name
*   Description
*   Root Namespace
*   Author Information

Once you click "Create", the tool will generate the complete folder structure and all necessary files for your new package inside the `Packages` folder.
