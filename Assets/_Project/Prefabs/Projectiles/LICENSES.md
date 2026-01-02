# Projectile and VFX Asset Licenses

This document tracks the licenses for projectile and VFX assets used in the TowerDefencer project.

## Custom Generated Assets

All projectile prefabs and VFX prefabs in this project were custom-generated for TowerDefencer.

### Projectile Prefabs

Located in `Assets/_Project/Prefabs/Projectiles/`:

| Prefab | Description | Material |
|--------|-------------|----------|
| Projectile_Basic.prefab | Yellow/gold sphere for Basic Tower | M_Projectile_Basic |
| Projectile_Cannon.prefab | Dark metallic cannonball for AOE Tower | M_Projectile_Cannon |
| Projectile_Ice.prefab | Cyan ice shard for Slow Tower | M_Projectile_Ice |
| Projectile_Sniper.prefab | Red/orange elongated projectile for Sniper Tower | M_Projectile_Sniper |
| Projectile_Support.prefab | Green orb for Support Tower | M_Projectile_Support |

### VFX Prefabs

Located in `Assets/_Project/Prefabs/VFX/`:

| Prefab | Description | Material |
|--------|-------------|----------|
| VFX_MuzzleFlash.prefab | Quick flash when tower fires | M_VFX_MuzzleFlash |
| VFX_Impact.prefab | Projectile impact splash | M_VFX_Impact |
| VFX_Explosion.prefab | AOE tower explosion effect | M_VFX_Explosion |
| VFX_Freeze.prefab | Ice tower slow/freeze effect | M_VFX_Freeze |
| VFX_Death.prefab | Enemy death effect | M_VFX_Death |
| VFX_Buff.prefab | Support tower buff aura | M_VFX_Buff |

### Materials

Located in `Assets/_Project/Art/Materials/Projectiles/` and `Assets/_Project/Art/Materials/VFX/`:

All materials use Unity's Universal Render Pipeline (URP) Lit shader with emissive properties for glow effects.

## License Terms

All custom-generated projectile and VFX assets in this project are released under **CC0 1.0 Universal (Public Domain)**.

You can copy, modify, distribute and perform the work, even for commercial purposes, all without asking permission.

## Recommended Replacements

For more polished VFX, consider:

- **Unity Particle Pack** - Free particle effects from Unity
- **Hovl Studio** - High-quality VFX assets on Asset Store
- **Gabriel Aguiar Prod.** - Free VFX tutorials and assets

When replacing assets, ensure you:
1. Update the prefab references in TowerData ScriptableObjects
2. Maintain the same component structure (ParticleSystem, Colliders, etc.)
3. Document new licenses in this file
