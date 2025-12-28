# Tower Defense Game Development Roadmap

## Project Overview

A professional 3D isometric tower defense game built with Unity 6, featuring 20 extensible levels, modular architecture, and industry-standard practices.

---

## Research Summary

### Unity 6 Key Features & Best Practices

Based on research from [Unity 6 Features Announcement](https://unity.com/blog/unity-6-features-announcement) and [Unity Engine 2025 Roadmap](https://unity.com/blog/unity-engine-2025-roadmap):

| Feature | Relevance to Tower Defense |
|---------|---------------------------|
| **GPU Resident Drawer** | Optimizes rendering of many towers/enemies on screen |
| **Adaptive Probe Volumes** | Automated lighting for level design efficiency |
| **URP Deferred+** | Efficient handling of projectile/explosion lights |
| **Multiplayer Center** | Future-proofing for potential co-op modes |
| **Unity Sentis** | AI-powered testing and optimization |
| **Build Profiles** | Multi-platform deployment (PC, Mobile, WebGL) |

**Recommended Render Pipeline**: Universal Render Pipeline (URP) - best for cross-platform support, good performance, and modern features without HDRP overhead.

### UI System Decision

Based on [Unity UI Toolkit vs UGUI 2025 Guide](https://www.angry-shark-studio.com/blog/unity-ui-toolkit-vs-ugui-2025-guide/):

**Decision: Hybrid Approach**
- **UI Toolkit**: Menus, settings, level select, inventory-style tower selection, data-heavy interfaces
- **uGUI**: In-game HUD, health bars, floating damage numbers (better animation support)

**Rationale**:
- UI Toolkit is Unity's future-proof solution with better performance for complex UIs
- uGUI remains superior for animated, game-like HUD elements
- Both systems coexist without issues in the same project

### Architecture Patterns

Based on [Unity ScriptableObject Architecture](https://unity.com/resources/create-modular-game-architecture-scriptableobjects-unity-6) and [Tower Defense Architecture Guide](https://www.cubix.co/blog/demystifying-tower-defense-game-architecture-practical-guide/):

**Core Patterns to Implement**:
1. **ScriptableObject Data Architecture** - All game data (towers, enemies, waves, levels) as SOs
2. **Event-Driven Communication** - ScriptableObject-based events for decoupling
3. **State Machine Pattern** - Game states, tower states, enemy states
4. **Object Pooling** - All spawned entities (enemies, projectiles, VFX)
5. **Command Pattern** - Tower placement, upgrades (enables undo/redo)
6. **Strategy Pattern** - Targeting priorities, damage types

### Reference Games

Based on [GameSpot Best Tower Defense Games 2025](https://www.gamespot.com/gallery/best-tower-defense-games/2900-6140/) and [PC Gamesn Best TD Games](https://www.pcgamesn.com/best-tower-defense-games-pc):

| Game | Key Mechanics to Reference |
|------|---------------------------|
| **Bloons TD 6** | Hero system, tower upgrade paths, monkey knowledge (meta progression) |
| **Kingdom Rush** | Strategic tower placement, hero abilities, varied enemy types |
| **Plants vs Zombies** | Lane-based simplicity, resource management (sun), lawnmower "lives" |
| **Sanctum 2** | FPS hybrid, maze building, co-op potential |
| **Legion TD 2** | Multiplayer competitive, unit composition strategy |

**Core Mechanics to Implement**:
- Wave-based progression with difficulty curves
- Multiple tower archetypes (DPS, AoE, Slow, Support)
- Enemy variety (basic, fast, tank, flying, boss)
- Economy with strategic choices (save vs spend)
- Upgrade paths for depth
- Hero/special ability system for player agency

---

## Recommended Asset Store Packages

### Essential (High Priority)

| Package | Purpose | Price | Link |
|---------|---------|-------|------|
| **A* Pathfinding Project Pro** | Superior pathfinding with grid graphs, real-time updates, tower blocking | $70 | [Asset Store](https://assetstore.unity.com/packages/tools/behavior-ai/a-pathfinding-project-pro-87744) |
| **DOTween Pro** | Animation, UI transitions, juice effects | $50 | [Asset Store](https://assetstore.unity.com/packages/tools/visual-scripting/dotween-pro-32416) |
| **Odin Inspector and Serializer** | Editor workflow, SO editing, serialization | $55 | [Asset Store](https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041) |

### Recommended (Quality of Life)

| Package | Purpose | Price | Notes |
|---------|---------|-------|-------|
| **Shapes** | Procedural geometry for range indicators, debug visualization | $40 | Alternative: use Line Renderer + custom shaders |
| **Eazy Sound Manager** | Audio management, pooling, crossfade | Free | [Asset Store](https://assetstore.unity.com/packages/tools/audio/eazy-sound-manager-71142) |
| **UNI VFX: Shields & Defense** | Professional shield/defense VFX | $15 | [Asset Store](https://assetstore.unity.com/packages/vfx/particles/spells/uni-vfx-shields-defense-for-visual-effect-graph-234167) |

### Free Assets for Prototyping

| Package | Purpose |
|---------|---------|
| **Kenney Game Assets** | CC0 3D models, UI elements, icons |
| **Unity Primitives** | Cubes, spheres for initial prototyping |
| **ProBuilder** | Level blockouts (included in Unity) |
| **DOTween (Free)** | Basic tweening if not using Pro |

### Art Assets (To Be Selected)

For a professional look, consider these categories:
- **Stylized/Low-Poly 3D Tower Pack** - Synty Studios or similar
- **Enemy Character Pack** - Animated models with multiple enemy types
- **Environment Kit** - Isometric terrain, props, obstacles
- **VFX Pack** - Explosions, impacts, projectiles
- **UI Kit** - Icons, buttons, frames

---

## Project Architecture

### Folder Structure

```
Assets/
├── _Project/                          # All project-specific assets
│   ├── Scripts/
│   │   ├── Runtime/
│   │   │   ├── Core/                  # GameManager, Events, Pooling, State
│   │   │   ├── Camera/                # Isometric camera controller
│   │   │   ├── Grid/                  # Placement grid, validation
│   │   │   ├── Towers/                # Tower base, behaviors, targeting
│   │   │   ├── Enemies/               # Enemy base, movement, abilities
│   │   │   ├── Projectiles/           # Projectile types, effects
│   │   │   ├── Waves/                 # Wave spawning, progression
│   │   │   ├── Economy/               # Currency, costs, rewards
│   │   │   ├── Upgrades/              # Upgrade system, paths
│   │   │   ├── Levels/                # Level loading, progression
│   │   │   ├── UI/                    # UI controllers, views
│   │   │   └── Audio/                 # Audio management
│   │   ├── Editor/                    # Custom editors, tools
│   │   └── Tests/
│   │       ├── EditMode/
│   │       └── PlayMode/
│   ├── Prefabs/
│   │   ├── Towers/
│   │   ├── Enemies/
│   │   ├── Projectiles/
│   │   ├── VFX/
│   │   └── UI/
│   ├── ScriptableObjects/
│   │   ├── Data/
│   │   │   ├── Towers/                # TowerData SOs
│   │   │   ├── Enemies/               # EnemyData SOs
│   │   │   ├── Waves/                 # WaveConfig SOs
│   │   │   ├── Levels/                # LevelData SOs
│   │   │   └── Upgrades/              # UpgradeData SOs
│   │   ├── Events/                    # SO-based game events
│   │   └── Variables/                 # SO-based runtime variables
│   ├── Scenes/
│   │   ├── Boot/                      # Initialization scene
│   │   ├── MainMenu/
│   │   ├── Levels/                    # L01-L20 gameplay scenes
│   │   └── Test/                      # Development test scenes
│   ├── Art/
│   │   ├── Models/
│   │   ├── Materials/
│   │   ├── Textures/
│   │   └── Animations/
│   ├── Audio/
│   │   ├── Music/
│   │   └── SFX/
│   ├── UI/
│   │   ├── Toolkit/                   # UI Toolkit assets
│   │   │   ├── Documents/             # UXML files
│   │   │   ├── Styles/                # USS files
│   │   │   └── Templates/
│   │   └── Sprites/                   # UI sprites, icons
│   └── Settings/
│       ├── Input/                     # Input Action assets
│       ├── Rendering/                 # URP settings
│       └── Physics/                   # Layer collision matrix
├── Plugins/                           # Third-party packages
└── Resources/                         # Runtime-loaded assets (minimal use)
```

### Assembly Definitions

| Assembly | Contents | References |
|----------|----------|------------|
| `TowerDefense.Runtime` | All runtime scripts | Unity assemblies, DOTween, A* |
| `TowerDefense.Runtime.Core` | Core systems, events | Unity assemblies |
| `TowerDefense.Runtime.Towers` | Tower-specific code | Core |
| `TowerDefense.Runtime.Enemies` | Enemy-specific code | Core |
| `TowerDefense.Runtime.UI` | UI controllers | Core, UI Toolkit |
| `TowerDefense.Editor` | Editor tools | Runtime, Unity Editor |
| `TowerDefense.Tests.EditMode` | Edit mode tests | Runtime, NUnit |
| `TowerDefense.Tests.PlayMode` | Play mode tests | Runtime, NUnit |

### Key ScriptableObject Types

```csharp
// Core Data Types
TowerData           // Stats, prefab, costs, upgrade paths
EnemyData           // Health, speed, rewards, abilities
WaveConfig          // Enemy composition, spawn timing
LevelData           // Map reference, waves, difficulty
UpgradeData         // Stat modifications, costs

// Runtime Variables (SO-based)
IntVariable         // Currency, lives, wave number
FloatVariable       // Game speed, timers
BoolVariable        // Pause state, build mode

// Events (SO-based)
GameEvent           // No parameters
GameEvent<T>        // Generic typed events
```

---

## Issue Categories & Estimates

### Phase 1: Foundation (Issues 1-8)

| # | Issue | Description |
|---|-------|-------------|
| 1 | Project Setup | Unity 6 project, folder structure, packages, settings |
| 2 | Assembly Definitions | Create all asmdef files with proper references |
| 3 | Core Event System | ScriptableObject-based events infrastructure |
| 4 | Core Variable System | ScriptableObject-based runtime variables |
| 5 | Game State Manager | State machine for game flow (Menu, Playing, Paused, Victory, Defeat) |
| 6 | Object Pool System | Generic pooling with Addressables support |
| 7 | Isometric Camera Controller | Pan, zoom, bounds, smooth movement |
| 8 | Input System Setup | New Input System actions for camera, placement, UI |

**Estimated Issues: 8**

### Phase 2: Grid & Placement (Issues 9-15)

| # | Issue | Description |
|---|-------|-------------|
| 9 | Grid Data Structure | Grid class with cell states, coordinates |
| 10 | Grid Visualization | Visual grid overlay, cell highlighting |
| 11 | Placement Validation | Check buildable cells, blocking rules |
| 12 | Tower Placement Preview | Ghost preview, valid/invalid feedback |
| 13 | Tower Placement Execution | Place tower, deduct cost, update grid |
| 14 | Tower Selling | Remove tower, refund partial cost |
| 15 | Placement UI Panel | Tower selection bar, cost display |

**Estimated Issues: 7**

### Phase 3: Tower Core (Issues 16-22)

| # | Issue | Description |
|---|-------|-------------|
| 16 | TowerData ScriptableObject | Data structure for tower stats |
| 17 | Tower Base Component | Core tower MonoBehaviour |
| 18 | Tower Range Detection | Physics overlap for enemy detection |
| 19 | Targeting Priority System | First, nearest, strongest, weakest |
| 20 | Tower Rotation | Smooth rotation toward target |
| 21 | Basic Projectile Tower | Archer-type single target tower |
| 22 | Tower Selection & Info Panel | Click tower to see stats, sell button |

**Estimated Issues: 7**

### Phase 4: Projectile System (Issues 23-27)

| # | Issue | Description |
|---|-------|-------------|
| 23 | Projectile Base Component | Movement, collision, damage delivery |
| 24 | Projectile Data ScriptableObject | Speed, damage, effects |
| 25 | Projectile Pooling | Pool integration for all projectiles |
| 26 | Projectile VFX | Trail, impact effects |
| 27 | Homing Projectile Variant | Tracking projectile for certain towers |

**Estimated Issues: 5**

### Phase 5: Enemy Core (Issues 28-37)

| # | Issue | Description |
|---|-------|-------------|
| 28 | EnemyData ScriptableObject | Health, speed, rewards, resistances |
| 29 | Enemy Base Component | Core enemy MonoBehaviour |
| 30 | A* Pathfinding Integration | Setup A* grids, enemy agent |
| 31 | Enemy Path Following | Navigate waypoints/path |
| 32 | Health System | IDamageable, health tracking |
| 33 | Enemy Health Bar | World-space health bar UI |
| 34 | Enemy Death | Death event, reward, pooling return |
| 35 | Enemy Pooling | Pool integration for enemies |
| 36 | Enemy Reached End | Trigger life loss, remove enemy |
| 37 | Basic Enemy Prefab | Standard ground enemy |

**Estimated Issues: 10**

### Phase 6: Wave System (Issues 38-44)

| # | Issue | Description |
|---|-------|-------------|
| 38 | WaveConfig ScriptableObject | Wave composition data |
| 39 | Wave Spawner | Spawn enemies per wave config |
| 40 | Wave Progression | Wave completion, next wave trigger |
| 41 | Wave Timing | Delay between waves, auto-start option |
| 42 | Wave UI | Current wave, enemies remaining |
| 43 | Send Next Wave Early | Bonus for early wave start |
| 44 | Victory Condition | All waves complete check |

**Estimated Issues: 7**

### Phase 7: Economy System (Issues 45-50)

| # | Issue | Description |
|---|-------|-------------|
| 45 | Currency System | Gold tracking, events |
| 46 | Enemy Kill Rewards | Grant currency on death |
| 47 | Wave Completion Bonus | Bonus currency per wave |
| 48 | Tower Cost Validation | Check affordability |
| 49 | Currency UI | Gold display, animations |
| 50 | Lives System | Player lives, game over trigger |

**Estimated Issues: 6**

### Phase 8: Tower Variety (Issues 51-58)

| # | Issue | Description |
|---|-------|-------------|
| 51 | AoE Tower | Cannon with splash damage |
| 52 | Slow Tower | Freeze/slow effect on enemies |
| 53 | Support Tower | Buff adjacent towers |
| 54 | Sniper Tower | Long range, high damage, slow fire |
| 55 | Area Effect System | Generic area damage/effects |
| 56 | Status Effect System | Slow, burn, poison infrastructure |
| 57 | Tower Buff System | Damage, speed, range buffs |
| 58 | Tower Type UI Icons | Visual distinction in selection |

**Estimated Issues: 8**

### Phase 9: Enemy Variety (Issues 59-66)

| # | Issue | Description |
|---|-------|-------------|
| 59 | Fast Enemy | High speed, low health |
| 60 | Tank Enemy | High health, slow, armored |
| 61 | Flying Enemy | Ignores ground path |
| 62 | Swarm Enemy | Many weak units |
| 63 | Boss Enemy | Massive health, abilities |
| 64 | Armor System | Damage reduction mechanic |
| 65 | Enemy Abilities | Shield, heal, spawn minions |
| 66 | Flying Path System | Separate path for air units |

**Estimated Issues: 8**

### Phase 10: Upgrade System (Issues 67-73)

| # | Issue | Description |
|---|-------|-------------|
| 67 | UpgradeData ScriptableObject | Upgrade definitions |
| 68 | Tower Upgrade Logic | Apply upgrades, track level |
| 69 | Upgrade Path Branching | Choose between upgrade paths |
| 70 | Upgrade UI Panel | Show upgrade options, costs |
| 71 | Visual Upgrade Indication | Tower visual changes per level |
| 72 | Upgrade Cost Scaling | Progressive cost increase |
| 73 | Max Level Handling | Upgrade cap, visual feedback |

**Estimated Issues: 7**

### Phase 11: Level System (Issues 74-82)

| # | Issue | Description |
|---|-------|-------------|
| 74 | LevelData ScriptableObject | Level configuration |
| 75 | Level Scene Template | Standard level scene setup |
| 76 | Level Loading | Scene management, transitions |
| 77 | Level Select UI | Map/list of available levels |
| 78 | Level Unlock Progression | Star-based or linear unlock |
| 79 | Level Difficulty Scaling | Enemy/wave scaling per level |
| 80 | Level-Specific Rules | Unique mechanics per map |
| 81 | Create Levels 1-5 | First 5 playable levels |
| 82 | Create Levels 6-20 | Remaining 15 levels |

**Estimated Issues: 9**

### Phase 12: Main Menu & Meta (Issues 83-89)

| # | Issue | Description |
|---|-------|-------------|
| 83 | Main Menu Scene | Start, continue, settings, quit |
| 84 | Settings Menu | Volume, graphics, controls |
| 85 | Pause Menu | Resume, restart, settings, quit |
| 86 | Victory Screen | Stats, rewards, next level |
| 87 | Defeat Screen | Retry, level select |
| 88 | Save System | Progress persistence |
| 89 | Boot/Loading Scene | Initial load, splash |

**Estimated Issues: 7**

### Phase 13: Audio (Issues 90-94)

| # | Issue | Description |
|---|-------|-------------|
| 90 | Audio Manager Setup | Integration with audio asset |
| 91 | Music System | Background music, transitions |
| 92 | Tower Sound Effects | Shoot, build, upgrade sounds |
| 93 | Enemy Sound Effects | Death, spawn, ability sounds |
| 94 | UI Sound Effects | Click, hover, purchase sounds |

**Estimated Issues: 5**

### Phase 14: VFX & Polish (Issues 95-102)

| # | Issue | Description |
|---|-------|-------------|
| 95 | Projectile Impact VFX | Hit effects per damage type |
| 96 | Tower Attack VFX | Muzzle flash, charging effects |
| 97 | Enemy Death VFX | Death explosions, fades |
| 98 | Status Effect VFX | Slow ice, burn fire visuals |
| 99 | UI Juice | Button animations, transitions |
| 100 | Floating Damage Numbers | Damage feedback |
| 101 | Screen Shake | Impact feedback |
| 102 | Camera Effects | Wave start zoom, boss intro |

**Estimated Issues: 8**

### Phase 15: Quality & Performance (Issues 103-108)

| # | Issue | Description |
|---|-------|-------------|
| 103 | Performance Profiling | Identify bottlenecks |
| 104 | LOD Setup | Level of detail for models |
| 105 | Occlusion Culling | Optimize rendering |
| 106 | Mobile Optimization | Touch controls, performance |
| 107 | Build Configuration | Platform-specific settings |
| 108 | Final Polish Pass | Bug fixes, balance tweaks |

**Estimated Issues: 6**

---

## Issue Count Summary

| Phase | Category | Issues |
|-------|----------|--------|
| 1 | Foundation | 8 |
| 2 | Grid & Placement | 7 |
| 3 | Tower Core | 7 |
| 4 | Projectile System | 5 |
| 5 | Enemy Core | 10 |
| 6 | Wave System | 7 |
| 7 | Economy System | 6 |
| 8 | Tower Variety | 8 |
| 9 | Enemy Variety | 8 |
| 10 | Upgrade System | 7 |
| 11 | Level System | 9 |
| 12 | Main Menu & Meta | 7 |
| 13 | Audio | 5 |
| 14 | VFX & Polish | 8 |
| 15 | Quality & Performance | 6 |
| **TOTAL** | | **108** |

---

## Development Milestones

### Milestone 1: Core Loop (Issues 1-37)
**Playable**: Basic tower shoots basic enemy, enemy follows path, player loses lives.

### Milestone 2: Complete Gameplay (Issues 38-58)
**Playable**: Full wave system, economy, multiple tower types, victory/defeat.

### Milestone 3: Content Complete (Issues 59-82)
**Playable**: All enemy types, upgrade system, 20 levels.

### Milestone 4: Production Ready (Issues 83-108)
**Shippable**: Full UI, audio, VFX, polish, optimized.

---

## Technical Decisions Summary

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Unity Version | Unity 6 (6000.x LTS) | Latest stable, GPU Resident Drawer, future-proof |
| Render Pipeline | URP | Cross-platform, performant, Deferred+ for lights |
| UI System | Hybrid (UI Toolkit + uGUI) | Best of both worlds |
| Pathfinding | A* Pathfinding Project Pro | Superior to NavMesh for TD, grid support |
| Animation/Tweening | DOTween Pro | Industry standard, visual editor |
| Architecture | ScriptableObject-based | Modular, designer-friendly, testable |
| Input | New Input System | Modern, flexible, multi-device |
| Object Pooling | Custom with Addressables | Performance, memory management |
| Audio | Eazy Sound Manager (or built-in) | Simple, pooled, cross-platform |

---

## Next Steps

1. **Review this roadmap** - Adjust scope, priorities, or technical decisions
2. **Approve Asset Store purchases** - Confirm budget for recommended packages
3. **Begin Issue Generation** - Generate issues in batches of 10-15 for review
4. **Setup Unity Project** - Create project once Issue 1 is approved

---

## Sources

### Unity 6 & Best Practices
- [Unity 6 Features Announcement](https://unity.com/blog/unity-6-features-announcement)
- [Unity Engine 2025 Roadmap](https://unity.com/blog/unity-engine-2025-roadmap)
- [Unity 6 Documentation - What's New](https://docs.unity3d.com/6000.2/Documentation/Manual/WhatsNewUnity6.html)
- [Unity 6 Key Features](https://rocketbrush.com/blog/unity-6-what-you-need-to-know-about-the-new-version)

### UI Systems
- [Unity UI Toolkit vs UGUI 2025 Guide](https://www.angry-shark-studio.com/blog/unity-ui-toolkit-vs-ugui-2025-guide/)
- [Unity Discussions - Official Recommendation](https://discussions.unity.com/t/official-recommendation-unity-ui-vs-ui-toolkit/892342)
- [Unity Manual - UI System Comparison](https://docs.unity3d.com/6000.2/Documentation/Manual/UI-system-compare.html)

### Architecture
- [Create Modular Game Architecture with ScriptableObjects](https://unity.com/resources/create-modular-game-architecture-scriptableobjects-unity-6)
- [Separate Game Data and Logic with ScriptableObjects](https://unity.com/how-to/separate-game-data-logic-scriptable-objects)
- [6 Ways ScriptableObjects Benefit Your Team](https://blog.unity.com/engine-platform/6-ways-scriptableobjects-can-benefit-your-team-and-your-code)
- [Tower Defense Architecture Guide](https://www.cubix.co/blog/demystifying-tower-defense-game-architecture-practical-guide/)

### Asset Store Packages
- [A* Pathfinding Project Pro](https://assetstore.unity.com/packages/tools/behavior-ai/a-pathfinding-project-pro-87744)
- [DOTween Pro](https://assetstore.unity.com/packages/tools/visual-scripting/dotween-pro-32416)
- [Odin Inspector and Serializer](https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041)
- [Eazy Sound Manager](https://assetstore.unity.com/packages/tools/audio/eazy-sound-manager-71142)

### Object Pooling
- [Unity Learn - Object Pooling Tutorial](https://learn.unity.com/tutorial/use-object-pooling-to-boost-performance-of-c-scripts-in-unity?uv=6)
- [Unity Addressables Pooling](https://thegamedev.guru/unity-addressables/pooling/)

### Reference Games
- [GameSpot - 24 Best Tower Defense Games 2025](https://www.gamespot.com/gallery/best-tower-defense-games/2900-6140/)
- [PC Gamesn - 15 Best Tower Defense Games PC 2025](https://www.pcgamesn.com/best-tower-defense-games-pc)
- [Plarium - 10 Best Tower Defense Games 2024](https://plarium.com/en/blog/best-tower-defense-games/)
