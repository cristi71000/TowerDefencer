# TowerDefencer - Folder Structure

This document defines the folder hierarchy for the TowerDefencer 3D isometric tower defense game project.

## Overview

The project uses Unity's recommended folder organization with a `_Project` prefix to keep project-specific assets at the top of the Project window and separated from third-party packages.

## Complete Folder Structure

```
Assets/
├── _Project/                          # All project-specific assets
│   │
│   ├── Scripts/                       # All C# scripts
│   │   ├── Runtime/                   # Runtime code (playable game code)
│   │   │   ├── Core/                  # Core systems (GameManager, ServiceLocator, Events)
│   │   │   ├── Camera/                # Camera controllers, isometric camera setup
│   │   │   ├── Grid/                  # Grid system, tile management, pathfinding
│   │   │   ├── Towers/                # Tower base classes, targeting, shooting
│   │   │   ├── Enemies/               # Enemy controllers, movement, health
│   │   │   ├── Projectiles/           # Projectile behavior, damage application
│   │   │   ├── Waves/                 # Wave spawning, wave configuration
│   │   │   ├── Economy/               # Currency, resource management
│   │   │   ├── Upgrades/              # Upgrade systems, tower enhancement
│   │   │   ├── Levels/                # Level loading, level progression
│   │   │   ├── UI/                    # UI controllers, screen managers
│   │   │   └── Audio/                 # Audio managers, sound controllers
│   │   │
│   │   ├── Editor/                    # Editor-only code (custom inspectors, tools)
│   │   │
│   │   └── Tests/                     # Unit and integration tests
│   │       ├── EditMode/              # Tests that run in Edit mode
│   │       └── PlayMode/              # Tests that run in Play mode
│   │
│   ├── Prefabs/                       # Reusable game objects
│   │   ├── Towers/                    # Tower prefabs (Archer, Cannon, etc.)
│   │   ├── Enemies/                   # Enemy prefabs (Goblin, Orc, etc.)
│   │   ├── Projectiles/               # Projectile prefabs (Arrow, Cannonball)
│   │   ├── VFX/                       # Visual effects (explosions, impacts)
│   │   └── UI/                        # UI prefabs (panels, buttons, HUD elements)
│   │
│   ├── ScriptableObjects/             # Data-driven configurations
│   │   ├── Data/                      # Game data definitions
│   │   │   ├── Towers/                # TowerData assets
│   │   │   ├── Enemies/               # EnemyData assets
│   │   │   ├── Waves/                 # WaveConfig assets
│   │   │   ├── Levels/                # LevelData assets
│   │   │   └── Upgrades/              # UpgradeData assets
│   │   │
│   │   ├── Events/                    # ScriptableObject-based events
│   │   │
│   │   └── Variables/                 # ScriptableObject-based variables
│   │
│   ├── Scenes/                        # Game scenes
│   │   ├── Boot/                      # Initial loading scene
│   │   ├── MainMenu/                  # Main menu scene
│   │   ├── Levels/                    # Playable level scenes
│   │   └── Test/                      # Test/development scenes
│   │
│   ├── Art/                           # Visual assets
│   │   ├── Models/                    # 3D models (.fbx, .obj)
│   │   ├── Materials/                 # Unity materials
│   │   ├── Textures/                  # Texture files
│   │   └── Animations/                # Animation clips and controllers
│   │
│   ├── Audio/                         # Sound assets
│   │   ├── Music/                     # Background music tracks
│   │   └── SFX/                       # Sound effects
│   │
│   ├── UI/                            # UI-specific assets
│   │   ├── Toolkit/                   # UI Toolkit assets
│   │   │   ├── Documents/             # UXML documents
│   │   │   ├── Styles/                # USS stylesheets
│   │   │   └── Templates/             # Reusable UI templates
│   │   │
│   │   └── Sprites/                   # 2D sprites for UI
│   │
│   └── Settings/                      # Project settings assets
│       ├── Input/                     # Input System actions
│       ├── Rendering/                 # URP settings, render features
│       └── Physics/                   # Physics materials, layer configs
│
├── Plugins/                           # Third-party plugins and native libraries
│
└── Resources/                         # Assets loaded via Resources.Load()
                                       # (use sparingly - prefer Addressables)
```

## Folder Descriptions

### Scripts/Runtime/

| Folder | Purpose |
|--------|---------|
| **Core/** | Game initialization, service locator, event bus, game state management |
| **Camera/** | Isometric camera controller, camera bounds, zoom functionality |
| **Grid/** | Grid-based placement system, tile types, pathfinding integration |
| **Towers/** | Tower base class, targeting systems, firing mechanics, range indicators |
| **Enemies/** | Enemy base class, health system, movement along paths, damage handling |
| **Projectiles/** | Projectile movement, collision detection, damage application |
| **Waves/** | Wave spawning logic, wave timing, enemy spawn configuration |
| **Economy/** | Currency management, resource tracking, purchase validation |
| **Upgrades/** | Tower upgrade paths, stat modifications, unlock systems |
| **Levels/** | Level loading, progression tracking, difficulty scaling |
| **UI/** | Screen management, HUD updates, menu controllers |
| **Audio/** | Audio manager, sound pooling, music transitions |

### ScriptableObjects/

| Folder | Purpose |
|--------|---------|
| **Data/Towers/** | Tower stats, cost, range, damage, attack speed |
| **Data/Enemies/** | Enemy stats, health, speed, reward |
| **Data/Waves/** | Wave composition, spawn timing, enemy types per wave |
| **Data/Levels/** | Level configuration, available towers, starting resources |
| **Data/Upgrades/** | Upgrade costs, stat modifications, requirements |
| **Events/** | ScriptableObject events for decoupled communication |
| **Variables/** | Shared runtime variables (FloatVariable, IntVariable, etc.) |

### Scenes/

| Folder | Purpose |
|--------|---------|
| **Boot/** | Entry point scene for initialization |
| **MainMenu/** | Main menu, settings, level select |
| **Levels/** | Playable game levels (Level_01, Level_02, etc.) |
| **Test/** | Development/testing scenes |

## Assembly Definitions

The project uses Assembly Definitions to organize code compilation:

| Assembly | Location | Purpose |
|----------|----------|---------|
| **TowerDefense.Runtime** | Scripts/Runtime/ | Main game runtime code |
| **TowerDefense.Editor** | Scripts/Editor/ | Editor tools and custom inspectors |
| **TowerDefense.Tests.EditMode** | Scripts/Tests/EditMode/ | Edit mode unit tests |
| **TowerDefense.Tests.PlayMode** | Scripts/Tests/PlayMode/ | Play mode integration tests |

## Naming Conventions

### Files
- **Scripts**: PascalCase matching class name (`TowerController.cs`)
- **Prefabs**: PascalCase with type suffix (`ArcherTower_Prefab.prefab`)
- **ScriptableObjects**: PascalCase with data type (`ArcherTower_Data.asset`)
- **Scenes**: PascalCase with area prefix (`Level_01_Forest.unity`)
- **Materials**: PascalCase (`Wood_Material.mat`)
- **Textures**: PascalCase with type suffix (`Wood_Albedo.png`, `Wood_Normal.png`)

### Folders
- Use PascalCase for all folder names
- Keep names descriptive but concise

## Best Practices

1. **Never use Resources folder for large assets** - Use Addressables instead
2. **Keep prefabs atomic** - One prefab per entity type
3. **Use ScriptableObjects for data** - Avoid hard-coding values
4. **Maintain clear separation** - Runtime code never references Editor code
5. **Place .meta files in version control** - Essential for Unity collaboration

## Creating Empty Folders

Unity does not track empty folders in version control. To preserve folder structure, add a `.gitkeep` file to empty folders that should be tracked.

```bash
# Example command to create .gitkeep in all empty folders
find Assets/_Project -type d -empty -exec touch {}/.gitkeep \;
```
