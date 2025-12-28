# TowerDefencer - Setup Instructions

This document provides step-by-step instructions for setting up the Unity project for TowerDefencer, a 3D isometric tower defense game.

## Prerequisites

- **Unity Hub** (latest version)
- **Unity 2022.3 LTS** or **Unity 6** (LTS recommended)
- **Git** (for version control)
- **Visual Studio 2022** or **JetBrains Rider** (recommended IDEs)

---

## Step 1: Create Unity Project

1. Open **Unity Hub**
2. Click **New Project**
3. Select **Unity 2022.3.x LTS** (or Unity 6)
4. Choose template: **3D (URP)** (Universal Render Pipeline)
5. Project name: `TowerDefencer`
6. Location: `E:\Work\Games\AIGames\TowerDefencer`
   - **Important**: Select the existing folder - Unity will create project files inside it
7. Click **Create Project**

> **Note**: Unity will create the project structure including `Assets/`, `Packages/`, `ProjectSettings/`, etc.

---

## Step 2: Install Required Packages

After Unity opens, install the following packages via **Window > Package Manager**:

### Required Packages

| Package | Version | Purpose |
|---------|---------|---------|
| **Input System** | Latest | Modern input handling for keyboard/mouse/gamepad |
| **TextMeshPro** | Latest | High-quality text rendering |
| **ProBuilder** | Latest | Level prototyping and geometry creation |
| **Cinemachine** | Latest | Camera system for isometric view |
| **AI Navigation** | Latest | NavMesh pathfinding for enemies |

### Installation Steps

1. Open **Window > Package Manager**
2. Ensure "Unity Registry" is selected in the dropdown
3. Search for each package and click **Install**

For **TextMeshPro**, when prompted:
- Click **Import TMP Essentials**
- Optionally import TMP Examples & Extras

### Recommended Optional Packages

| Package | Purpose |
|---------|---------|
| **DOTween** (Asset Store) | Animation tweening |
| **Addressables** | Asset management for larger projects |
| **Unity Test Framework** | Unit testing (usually pre-installed) |

---

## Step 3: Create Folder Structure

Create the following folder structure inside `Assets/`:

### Using Unity Editor (Recommended)

1. Right-click in Project window > **Create > Folder**
2. Create folders in this order:

```
Assets/
└── _Project/
    ├── Scripts/
    │   ├── Runtime/
    │   │   ├── Core/
    │   │   ├── Camera/
    │   │   ├── Grid/
    │   │   ├── Towers/
    │   │   ├── Enemies/
    │   │   ├── Projectiles/
    │   │   ├── Waves/
    │   │   ├── Economy/
    │   │   ├── Upgrades/
    │   │   ├── Levels/
    │   │   ├── UI/
    │   │   └── Audio/
    │   ├── Editor/
    │   └── Tests/
    │       ├── EditMode/
    │       └── PlayMode/
    ├── Prefabs/
    │   ├── Towers/
    │   ├── Enemies/
    │   ├── Projectiles/
    │   ├── VFX/
    │   └── UI/
    ├── ScriptableObjects/
    │   ├── Data/
    │   │   ├── Towers/
    │   │   ├── Enemies/
    │   │   ├── Waves/
    │   │   ├── Levels/
    │   │   └── Upgrades/
    │   ├── Events/
    │   └── Variables/
    ├── Scenes/
    │   ├── Boot/
    │   ├── MainMenu/
    │   ├── Levels/
    │   └── Test/
    ├── Art/
    │   ├── Models/
    │   ├── Materials/
    │   ├── Textures/
    │   └── Animations/
    ├── Audio/
    │   ├── Music/
    │   └── SFX/
    ├── UI/
    │   ├── Toolkit/
    │   │   ├── Documents/
    │   │   ├── Styles/
    │   │   └── Templates/
    │   └── Sprites/
    └── Settings/
        ├── Input/
        ├── Rendering/
        └── Physics/
```

Also create at root level:
```
Assets/
├── Plugins/
└── Resources/
```

### Alternative: PowerShell Script

Run this in PowerShell from `E:\Work\Games\AIGames\TowerDefencer`:

```powershell
$folders = @(
    "Assets/_Project/Scripts/Runtime/Core",
    "Assets/_Project/Scripts/Runtime/Camera",
    "Assets/_Project/Scripts/Runtime/Grid",
    "Assets/_Project/Scripts/Runtime/Towers",
    "Assets/_Project/Scripts/Runtime/Enemies",
    "Assets/_Project/Scripts/Runtime/Projectiles",
    "Assets/_Project/Scripts/Runtime/Waves",
    "Assets/_Project/Scripts/Runtime/Economy",
    "Assets/_Project/Scripts/Runtime/Upgrades",
    "Assets/_Project/Scripts/Runtime/Levels",
    "Assets/_Project/Scripts/Runtime/UI",
    "Assets/_Project/Scripts/Runtime/Audio",
    "Assets/_Project/Scripts/Editor",
    "Assets/_Project/Scripts/Tests/EditMode",
    "Assets/_Project/Scripts/Tests/PlayMode",
    "Assets/_Project/Prefabs/Towers",
    "Assets/_Project/Prefabs/Enemies",
    "Assets/_Project/Prefabs/Projectiles",
    "Assets/_Project/Prefabs/VFX",
    "Assets/_Project/Prefabs/UI",
    "Assets/_Project/ScriptableObjects/Data/Towers",
    "Assets/_Project/ScriptableObjects/Data/Enemies",
    "Assets/_Project/ScriptableObjects/Data/Waves",
    "Assets/_Project/ScriptableObjects/Data/Levels",
    "Assets/_Project/ScriptableObjects/Data/Upgrades",
    "Assets/_Project/ScriptableObjects/Events",
    "Assets/_Project/ScriptableObjects/Variables",
    "Assets/_Project/Scenes/Boot",
    "Assets/_Project/Scenes/MainMenu",
    "Assets/_Project/Scenes/Levels",
    "Assets/_Project/Scenes/Test",
    "Assets/_Project/Art/Models",
    "Assets/_Project/Art/Materials",
    "Assets/_Project/Art/Textures",
    "Assets/_Project/Art/Animations",
    "Assets/_Project/Audio/Music",
    "Assets/_Project/Audio/SFX",
    "Assets/_Project/UI/Toolkit/Documents",
    "Assets/_Project/UI/Toolkit/Styles",
    "Assets/_Project/UI/Toolkit/Templates",
    "Assets/_Project/UI/Sprites",
    "Assets/_Project/Settings/Input",
    "Assets/_Project/Settings/Rendering",
    "Assets/_Project/Settings/Physics",
    "Assets/Plugins",
    "Assets/Resources"
)

foreach ($folder in $folders) {
    New-Item -ItemType Directory -Force -Path $folder
    # Create .gitkeep to preserve empty folders
    New-Item -ItemType File -Force -Path "$folder/.gitkeep"
}

Write-Host "Folder structure created successfully!"
```

> **Important**: After running the script, return to Unity and let it import the new folders.

---

## Step 4: Place Assembly Definitions

Copy the assembly definition files from `Templates/AssemblyDefinitions/` to their respective locations:

| Template File | Destination |
|---------------|-------------|
| `TowerDefense.Runtime.asmdef` | `Assets/_Project/Scripts/Runtime/` |
| `TowerDefense.Editor.asmdef` | `Assets/_Project/Scripts/Editor/` |
| `TowerDefense.Tests.EditMode.asmdef` | `Assets/_Project/Scripts/Tests/EditMode/` |
| `TowerDefense.Tests.PlayMode.asmdef` | `Assets/_Project/Scripts/Tests/PlayMode/` |

### PowerShell Copy Commands

```powershell
Copy-Item "Templates/AssemblyDefinitions/TowerDefense.Runtime.asmdef" "Assets/_Project/Scripts/Runtime/"
Copy-Item "Templates/AssemblyDefinitions/TowerDefense.Editor.asmdef" "Assets/_Project/Scripts/Editor/"
Copy-Item "Templates/AssemblyDefinitions/TowerDefense.Tests.EditMode.asmdef" "Assets/_Project/Scripts/Tests/EditMode/"
Copy-Item "Templates/AssemblyDefinitions/TowerDefense.Tests.PlayMode.asmdef" "Assets/_Project/Scripts/Tests/PlayMode/"
```

After copying, return to Unity and verify the assemblies appear in the Project window.

---

## Step 5: Configure Input System

### Enable New Input System

1. Go to **Edit > Project Settings > Player**
2. Under **Other Settings > Configuration**
3. Find **Active Input Handling**
4. Select **Input System Package (New)** or **Both**
5. Unity will prompt to restart - click **Yes**

### Create Input Actions Asset

1. Navigate to `Assets/_Project/Settings/Input/`
2. Right-click > **Create > Input Actions**
3. Name it `TowerDefenseInput`
4. Double-click to open the Input Actions editor
5. Create the following Action Maps and Actions:

#### Gameplay Action Map

| Action | Type | Bindings |
|--------|------|----------|
| Select | Button | Left Mouse Button |
| Cancel | Button | Right Mouse Button, Escape |
| CameraMove | Value (Vector2) | WASD, Arrow Keys |
| CameraZoom | Value (Axis) | Mouse Scroll Y |
| Pause | Button | Escape, P |

#### UI Action Map

| Action | Type | Bindings |
|--------|------|----------|
| Navigate | Value (Vector2) | WASD, Arrow Keys, Gamepad Left Stick |
| Submit | Button | Enter, Space, Gamepad South |
| Cancel | Button | Escape, Gamepad East |
| Point | PassThrough (Vector2) | Mouse Position |
| Click | Button | Left Mouse Button |

6. Click **Save Asset**
7. Check **Generate C# Class** in the Inspector
8. Set **C# Class File** to `Assets/_Project/Scripts/Runtime/Core/TowerDefenseInput.cs`
9. Click **Apply**

---

## Step 6: Configure Physics Layers

### Create Custom Layers

1. Go to **Edit > Project Settings > Tags and Layers**
2. Add the following layers:

| Layer # | Name |
|---------|------|
| 8 | Ground |
| 9 | Tower |
| 10 | Enemy |
| 11 | Projectile |
| 12 | Obstacle |
| 13 | Path |
| 14 | UI |
| 15 | Selectable |

### Configure Layer Collision Matrix

1. Go to **Edit > Project Settings > Physics**
2. In the **Layer Collision Matrix**, configure:

| Layer | Should Collide With |
|-------|---------------------|
| Ground | Tower, Enemy, Projectile |
| Tower | Ground, Enemy |
| Enemy | Ground, Tower, Projectile, Enemy |
| Projectile | Ground, Enemy |
| Obstacle | Ground, Enemy, Projectile |
| Path | (none - trigger only) |

3. Ensure Projectile does NOT collide with Tower

---

## Step 7: Configure URP Settings

### Quality Settings

1. Go to **Edit > Project Settings > Quality**
2. For each quality level, assign appropriate URP Asset:
   - Low: `Assets/Settings/URP-LowQuality.asset`
   - Medium: `Assets/Settings/URP-MediumQuality.asset`
   - High: `Assets/Settings/URP-HighQuality.asset`

### Graphics Settings

1. Go to **Edit > Project Settings > Graphics**
2. Ensure **Scriptable Render Pipeline Settings** is set to your URP Asset

### Create Custom URP Renderer (Optional)

For tower defense specific rendering features:
1. Create new URP Renderer Asset
2. Add custom render features as needed (outlines, selection highlights)

---

## Step 8: Create Initial Scenes

### Boot Scene

1. Navigate to `Assets/_Project/Scenes/Boot/`
2. Create a new scene: **File > New Scene**
3. Save as `Boot.unity`
4. This scene will handle:
   - Initialization
   - Loading essential managers
   - Transitioning to MainMenu

### Test Scene

1. Navigate to `Assets/_Project/Scenes/Test/`
2. Create a new scene, save as `Test_Gameplay.unity`
3. Use this for development and testing

### Build Settings

1. Go to **File > Build Settings**
2. Add scenes in order:
   - `Assets/_Project/Scenes/Boot/Boot.unity` (index 0)
   - `Assets/_Project/Scenes/MainMenu/MainMenu.unity` (index 1)
   - Level scenes...

---

## Step 9: Configure Version Control

### Verify .gitignore

The `.gitignore` file should already be in place at the project root. Verify it's working:

```bash
cd E:\Work\Games\AIGames\TowerDefencer
git status
```

You should NOT see `Library/`, `Temp/`, `Logs/`, etc. listed.

### Initial Commit

After completing all setup:

```bash
git add .
git commit -m "Initial Unity project setup with folder structure"
git push -u origin feature/issue-1-project-setup
```

---

## Step 10: Recommended Project Settings

### Editor Settings

1. **Edit > Project Settings > Editor**
2. Set **Asset Serialization Mode**: Force Text
3. Set **Default Behavior Mode**: 3D
4. Enable **Enter Play Mode Options** (faster iteration)
   - Disable Domain Reload (optional, requires careful coding)
   - Disable Scene Reload (optional)

### Player Settings

1. **Edit > Project Settings > Player**
2. Set **Company Name**: Your company/name
3. Set **Product Name**: TowerDefencer
4. Configure **Default Icon**
5. Set **Color Space**: Linear (for URP)

### Time Settings

1. **Edit > Project Settings > Time**
2. **Fixed Timestep**: 0.02 (50 FPS physics)
3. **Maximum Allowed Timestep**: 0.1

---

## Verification Checklist

Before proceeding with development, verify:

- [ ] Unity project opens without errors
- [ ] All packages installed and showing in Package Manager
- [ ] Folder structure created with all directories
- [ ] Assembly definitions placed and recognized by Unity
- [ ] Input System active (check for InputSystem namespace access)
- [ ] Physics layers created
- [ ] URP properly configured (no pink materials)
- [ ] Boot and Test scenes created
- [ ] Git repository initialized with .gitignore working
- [ ] Can create and save scripts without errors

---

## Troubleshooting

### Pink Materials
- URP not properly configured
- Check Graphics settings for Scriptable Render Pipeline

### Missing Assembly References
- Verify .asmdef files are in correct locations
- Check references in Inspector

### Input System Not Working
- Ensure "Input System Package (New)" is selected in Player Settings
- Restart Unity after changing input settings

### Git Tracking Library Folder
- Verify .gitignore is at project root
- May need to remove cached files: `git rm -r --cached Library/`

---

## Next Steps

After setup is complete:

1. Create core system scripts (GameManager, ServiceLocator)
2. Implement isometric camera controller
3. Build grid placement system
4. Create tower and enemy base classes
5. Implement wave spawning system

Refer to `FOLDER_STRUCTURE.md` for detailed folder purposes and `ROADMAP.md` for development phases.
