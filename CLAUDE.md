# CLAUDE.md - Project Rules and Guidelines

## Project Overview

**TowerDefencer** is a 3D isometric tower defense game built with Unity 6 (6000.0.42f1).

### Technology Stack
- **Engine**: Unity 6000.0.42f1 with 3D URP (Universal Render Pipeline)
- **Language**: C# (.NET Standard 2.1)
- **Input**: Unity Input System 1.13.1
- **UI**: TextMeshPro 3.2.0, UI Toolkit
- **Navigation**: AI Navigation 2.0.6 (NavMesh for enemy pathfinding)
- **Camera**: Cinemachine 3.1.3
- **Level Design**: ProBuilder 6.0.4

### Project Structure
```
Assets/_Project/
├── Art/           # Models, Materials, Textures, Animations
├── Audio/         # Music, SFX
├── Prefabs/       # Towers, Enemies, Projectiles, UI, VFX
├── Scenes/        # Boot, MainMenu, Levels, Test
├── ScriptableObjects/  # Data (Enemies, Towers, Waves, Levels, Upgrades), Events, Variables
├── Scripts/
│   ├── Runtime/   # Game code (Towers, Enemies, Waves, Economy, Grid, etc.)
│   ├── Editor/    # Editor tools
│   └── Tests/     # EditMode and PlayMode tests
├── Settings/      # Input, Physics, Rendering configurations
└── UI/            # Sprites, UI Toolkit documents and styles
```

### Assembly Definitions
- `TowerDefense.Runtime` - Main game code
- `TowerDefense.Editor` - Editor-only tools (Editor platform only)
- `TowerDefense.Tests.EditMode` - Edit mode tests (NUnit)
- `TowerDefense.Tests.PlayMode` - Play mode tests (NUnit)

### Physics Layers
| Layer | Name | Purpose |
|-------|------|---------|
| 6 | Ground | Terrain/grid for placement |
| 7 | Tower | Tower colliders |
| 8 | Enemy | Enemy targeting |
| 9 | Projectile | Projectile collisions |
| 10 | Obstacle | Path blocking |
| 11 | Buildable | Valid tower placement areas |
| 12 | Path | Enemy pathing |
| 13 | Selectable | Click detection |

### Input Actions
- **Gameplay**: Select (LMB), Cancel (RMB/Esc), CameraMove (WASD), CameraZoom (Scroll), Pause (Esc)
- **UI**: Navigate (Arrows), Submit (Enter), Cancel (Esc)

---

## Development Setup

### Prerequisites
1. Unity Hub with Unity 6000.0.42f1 installed
2. Git
3. GitHub CLI (`gh`) authenticated

### Opening the Project
1. Clone the repository
2. Open Unity Hub
3. Add project from disk: select the TowerDefencer folder
4. Open with Unity 6000.0.42f1

### Running Tests
- Open Unity Test Runner: Window > General > Test Runner
- Run EditMode tests for unit tests
- Run PlayMode tests for integration tests

---

## CRITICAL: Sacred Process Development Workflow

This project follows a **Sacred Process** that MUST be followed without deviation.

### The Sacred Process Steps

> **Note**: STEPS 3 and 6 are internal reviews performed by AI agents (Code Reviewer, QA Tester)
> before the PR is created. STEPS 10-12 are GitHub PR-based reviews by Copilot after PR creation.

1. **STEP 1: Issue Selection** - Lead selects the next issue to work on based on the issue number in the TITLE
2. **STEP 2: Development** - Developer implements the issue on a feature branch
3. **STEP 3: Code Review (Internal)** - AI Reviewer reviews the implementation locally
4. **STEP 4: Lead Analyzes Review** - Lead determines if fixes are needed
5. **STEP 5: Review Fixes** - Developer fixes any issues found (loop back to STEP 3)
6. **STEP 6: QA Testing (Internal)** - AI Tester validates the implementation locally
7. **STEP 7: Lead Analyzes QA** - Lead determines if fixes are needed
8. **STEP 8: QA Fixes** - Developer fixes any issues found (loop back to STEP 3)
9. **STEP 9: Create PR** - Developer creates a Pull Request
10. **STEP 10: Await Copilot Review** - Wait for GitHub Copilot review (up to 30 minutes max)
11. **STEP 11: Analyze Copilot Feedback** - Lead analyzes any Copilot comments
12. **STEP 12: Copilot Fixes** - Developer addresses Copilot feedback, pushes changes, returns to STEP 3
13. **STEP 13: Merge PR** - Only after Copilot review passes or timeout
14. **STEP 14: Completion** - Issue is closed, move to next issue

### MANDATORY RULES

#### Rule 1: Copilot Review Wait Time
**WAIT UP TO 30 MINUTES** for GitHub Copilot review after creating a PR.
- Poll `gh pr view <PR_NUMBER> --json reviews,comments` every 2 minutes
- Check `gh pr checks <PR_NUMBER>` for any CI/CD status
- When Copilot review arrives: proceed to STEP 11 (analyze), then STEP 12 (fix if needed)
- If you push fixes in STEP 12: return to STEP 3 (Code Review) to restart the cycle
- If no review after 30 minutes: proceed to merge
- Maximum wait time: 30 minutes each time you reach STEP 10

#### Rule 2: Never Skip Steps
- Every step in the Sacred Process must be executed in order
- No step may be skipped, even if it seems unnecessary
- Document the result of each step before proceeding

#### Rule 3: Review Loops
- If Code Review (STEP 3) finds issues: fix them and return to STEP 3
- If QA Testing (STEP 6) finds issues: fix them and return to STEP 3
- If Copilot Review (STEP 11) finds issues: fix them in STEP 12, push changes, and return to STEP 3 (restart cycle)
- Never proceed to the next major phase until the current phase passes

#### Rule 4: Code Change Restart Rule
**CRITICAL**: If ANY code change is made due to review feedback at ANY point in the process:
- Return to STEP 3 (Code Review) and restart the review cycle
- This applies to fixes from Code Review, QA Testing, OR Copilot Review
- **Internal loops (STEP 3-8) can iterate unlimited times** until issues are resolved
- **Maximum 3 iterations of the FULL cycle** (reaching Copilot review at STEP 10)
- After 3 full cycles through Copilot review, escalate to user for decision

#### Rule 5: Branch Discipline
- Always work on feature branches: `feature/issue-<number>-<short-description>`
- Never commit directly to main
- All changes go through PRs

#### Rule 6: Issue Tracking
- All work must be tied to a GitHub issue
- PRs must reference the issue with "Closes #X"
- Issues are only closed when the PR is merged

### Violation Consequences

Violating these rules undermines the quality assurance process and will result in:
1. Immediate halt of current work
2. Reversion of improper changes
3. Restart of the process from the violated step

### Polling Command Reference

```bash
# Check for reviews and comments
gh pr view <PR_NUMBER> --json reviews,comments

# Check CI/CD status
gh pr checks <PR_NUMBER>

# Check merge status
gh pr view <PR_NUMBER> --json state,mergeable,mergeStateStatus
```

### Time Tracking for STEP 10

When at STEP 10, wait for Copilot review (max 30 minutes):
- Record PR creation timestamp
- Poll every 2 minutes for reviews/comments
- When Copilot review arrives: proceed to STEP 11 (analyze), then STEP 12 (fix if needed)
- If you push fixes: return to STEP 3 (Code Review) to restart the cycle
- If no review after 30 minutes: proceed to merge
- Never wait longer than 30 minutes for any single STEP 10 attempt

---

## Code Style Guidelines

### C# Conventions
- Use PascalCase for public members, camelCase for private
- Prefix private fields with underscore: `_privateField`
- Use explicit access modifiers
- One class per file, file name matches class name

### Unity Conventions
- Use `[SerializeField]` for inspector-exposed private fields
- Prefer ScriptableObjects for game data
- Use events for decoupled communication
- Cache component references in Awake()

### Unity Asset GUIDs (CRITICAL)
Unity uses GUIDs to reference assets across the project. **ALL GUIDs MUST be kept in sync:**

1. **Meta Files**: Every asset (scripts, prefabs, materials, etc.) has a `.meta` file with a unique GUID
2. **Prefab References**: When a prefab references a script, the script's GUID from its `.meta` file must match
3. **ScriptableObject References**: When an SO references a prefab or other asset, the GUID must match the target's `.meta` file
4. **Scene References**: Scene files reference prefabs/scripts by GUID - these must match the actual asset GUIDs

**When creating assets manually (YAML):**
- Generate a unique 32-character lowercase hex GUID for each new asset
- Use the SAME GUID in both the asset and its `.meta` file
- When referencing other assets, use their GUID from their `.meta` file
- Format: `{fileID: <localID>, guid: <32-char-hex>, type: <type>}`

**Common fileID values:**
- `11500000` - MonoBehaviour script reference
- `11400000` - ScriptableObject main object
- `1000000000000000` - Common fileID for root GameObject in prefabs. This value can differ between prefabs/projects; what matters is that fileIDs stay internally consistent within each prefab

**Validation:** If Unity shows "Missing Script" or broken references, the GUIDs are out of sync

### Git Conventions
- Commit messages follow conventional commits: `feat:`, `fix:`, `docs:`, `refactor:`, etc.
- PRs must have descriptive titles and summaries
- Squash merge to keep history clean

---

## GitHub Issues

All development work is tracked via GitHub issues. There are 108 issues organized into 15 milestones:

- M1: Project Setup & Core Systems
- M2: Grid & Building System
- M3: Tower System Foundation
- M4: Enemy System Foundation
- M5: Wave Management
- M6: Combat System
- M7: Economy & Resources
- M8: Camera & Controls
- M9: UI Foundation
- M10: Game Flow & Scenes
- M11: Audio System
- M12: Level Design Tools
- M13: Balance & Polish
- M14: Content Creation
- M15: Mobile & Optimization
