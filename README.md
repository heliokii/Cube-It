# Cube Path Game

A text-based CLI puzzle game with a **pseudo-3D isometric** tile renderer,
animated level transitions, particle effects, and procedurally generated levels.

## Quick Start

```bash
# 1. Install .NET 8 SDK  →  https://dotnet.microsoft.com/download
# 2. Create project
dotnet new console -n CubePathGame
cd CubePathGame

# 3. Copy all .cs files from this archive into the project,
#    preserving the folder structure shown below.
#    Replace the generated Program.cs with the one provided.

# 4. Run
dotnet run
```

## Project Structure

```
CubePathGame/
├── Program.cs                    Entry point — wires everything together
├── CubePathGame.csproj           .NET 8 project file
│
├── Core/
│   ├── CellType.cs               Enum: Path / Wall / Visited / Powerup
│   ├── Position.cs               Immutable row/col value type
│   ├── Grid.cs                   Grid data model (no rendering logic)
│   └── Player.cs                 Player state, movement, scoring
│
├── Generation/
│   └── LevelGenerator.cs         Procedural level builder + BFS solver check
│
├── Rendering/
│   └── IsometricRenderer.cs      3D tile drawing + all console animations
│
├── UI/
│   ├── InputHandler.cs           Keys → GameAction mapping
│   └── MainMenu.cs               Animated title screen & instructions
│
└── Game/
    └── GameLoop.cs               Game state machine (Playing/Won/Lost/Over)
```

## How to Play

| Key | Action |
|-----|--------|
| W / ↑ | Move Up |
| S / ↓ | Move Down |
| A / ← | Move Left |
| D / → | Move Right |
| R | Restart level |
| Q / Esc | Quit to menu |

**Goal:** Step on every floor tile `▄▄` at least once without revisiting.
Tiles you've visited show as `◦◦`. Walls `████` are impassable.
Collect gems `✦` for +50 bonus points!

## OOP Principles Applied

| Principle | Where |
|-----------|-------|
| **Single Responsibility** | Grid (data only), Renderer (display only), GameLoop (orchestration only) |
| **Open/Closed** | New cell types just need a new `DrawXTile()` method |
| **Dependency Inversion** | GameLoop receives Renderer + Generator via constructor |
| **Encapsulation** | Grid's `_cells` array is private; only Grid mutates it |
| **Value Types** | `Position` is a readonly struct — immutable, copy-safe |
