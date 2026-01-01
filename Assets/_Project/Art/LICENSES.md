# Art Asset Licenses

This document lists the licenses and sources for art assets used in this project.

## Tower Models

The tower models in this project use Unity's built-in primitive shapes (Cube, Cylinder, Sphere) with custom materials. These primitives are part of Unity and do not require any external license.

### Tower Designs

- **BasicTower**: Cylinder base with cylindrical barrel - Green colored
- **AOETower**: Cube base with mortar-style cylinder - Orange colored
- **SlowTower**: Cylinder base with pillar and sphere orb - Blue colored

## Enemy Models

The enemy models in this project use Unity's built-in primitive shapes (Cube, Cylinder, Sphere) with custom materials. These primitives are part of Unity and do not require any external license.

### Enemy Designs

- **BasicEnemy**: Cube body with cube head - Red-brown colored
- **FastEnemy**: Streamlined cylinder body with sphere head - Green colored
- **TankEnemy**: Large cube body with tread details - Dark gray colored
- **FlyingEnemy**: Central sphere with 4 rotor spheres - Yellow colored
- **BossEnemy**: Large imposing body with shoulder pads and crown - Purple colored

## Materials

All materials in this project use Unity's Universal Render Pipeline (URP) Lit shader. Materials were created specifically for this project.

### Tower Materials

| Material | Color (RGB) | Description |
|----------|-------------|-------------|
| M_Tower_Basic | (0.3, 0.7, 0.3) | Green - Basic tower accent |
| M_Tower_AOE | (0.9, 0.4, 0.15) | Orange - AOE tower accent |
| M_Tower_Slow | (0.2, 0.5, 0.9) | Blue - Slow tower accent |
| M_Tower_Base | (0.4, 0.4, 0.45) | Gray - Shared base material |
| M_Tower_Barrel | (0.25, 0.25, 0.28) | Dark gray - Barrel/weapon material |
### Enemy Materials

| Material | Color (RGB) | Description |
|----------|-------------|-------------|
| M_Enemy_Basic | (0.7, 0.3, 0.2) | Red-brown - Basic enemy |
| M_Enemy_Fast | (0.2, 0.7, 0.3) | Green - Fast enemy |
| M_Enemy_Tank | (0.3, 0.3, 0.35) | Dark gray - Tank enemy |
| M_Enemy_Flying | (0.9, 0.8, 0.2) | Yellow - Flying enemy |
| M_Enemy_Boss | (0.5, 0.2, 0.6) | Purple - Boss enemy |
| M_Enemy_Accent | (0.15, 0.15, 0.18) | Dark accent - Shared details |

## Environment Models

The environment models in this project use Unity's built-in primitive shapes (Cube, Cylinder, Sphere) with custom materials. These primitives are part of Unity and do not require any external license.

### Environment Designs

- **GroundTile**: 1x1 flat cube tile for terrain - Layer 6 (Ground)
- **PathTile**: 1x1 flat cube tile for enemy paths - Layer 12 (Path)
- **SpawnPortal**: Archway gate with two pillars and base - Enemy spawn point - Layer 10 (Obstacle)
- **ExitFortress**: Fortress with central building and four corner towers - Player's base - Layer 10 (Obstacle)
- **GridCell**: Thin transparent overlay for tower placement - Layer 11 (Buildable)
- **Tree_Pine**: Stylized pine tree with trunk and layered cone foliage - Layer 10 (Obstacle)
- **Rock_Medium**: Multi-part rock formation - Layer 10 (Obstacle)
- **Fence_Segment**: Wooden fence with two posts and two rails - Layer 10 (Obstacle)

### Environment Materials

| Material | Color (RGB) | Description |
|----------|-------------|-------------|
| M_Ground | (0.35, 0.55, 0.25) | Grass green - Ground tiles |
| M_Path | (0.6, 0.5, 0.35) | Sandy brown - Path/road tiles |
| M_SpawnPortal | (0.6, 0.2, 0.7) | Purple/magenta with emission - Spawn portal accent |
| M_ExitFortress | (0.3, 0.4, 0.6) | Bluish gray - Fortress stone |
| M_GridCell | (0.3, 0.8, 0.4, 0.3) | Semi-transparent green - Placement indicator |
| M_Tree_Leaves | (0.2, 0.4, 0.2) | Dark green - Tree foliage |
| M_Tree_Trunk | (0.35, 0.25, 0.15) | Brown - Tree bark |
| M_Rock | (0.5, 0.5, 0.5) | Gray - Stone/rock |
| M_Fence | (0.45, 0.35, 0.25) | Wood brown - Fence material |

## Recommended External Assets

For production-quality tower models, consider using:

### Kenney Tower Defense Kit
- **URL**: https://kenney.nl/assets/tower-defense-kit
- **License**: CC0 1.0 Universal (Public Domain)
- **Attribution**: Not required but appreciated
- **Includes**: Multiple low-poly tower models, enemy models, and environment pieces

### Other Free Resources
- **Unity Asset Store**: Various free tower defense packs
- **OpenGameArt.org**: Public domain and CC-licensed models
- **Quaternius**: Free low-poly asset packs

## License Compliance

When importing external assets:
1. Always verify the license before use
2. Keep a copy of the original license
3. Add attribution if required
4. Do not modify assets in ways prohibited by their license

---

Last updated: 2026-01-01
