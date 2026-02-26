// ============================================================================
// Program.cs — Entry point
//
// HOW TO RUN:
//   1.  Install .NET 8 SDK  →  https://dotnet.microsoft.com/download
//   2.  dotnet new console -n CubePathGame
//   3.  Copy all .cs files into the project folder, preserving the
//       folder structure (Core/, Generation/, Rendering/, UI/, Game/).
//   4.  Replace the generated Program.cs with this file.
//   5.  cd CubePathGame
//       dotnet run
//
// RECOMMENDED TERMINAL:
//   Windows Terminal or any terminal that supports Unicode block characters
//   and 16-colour ANSI.  The Windows built-in cmd.exe works but colours
//   may be slightly muted.  On macOS / Linux any modern terminal is fine.
//
// PROJECT STRUCTURE:
//   Program.cs                   ← this file (entry, DI wiring)
//   Core/
//     CellType.cs                ← enum for grid tile states
//     Position.cs                ← immutable row/col value type
//     Grid.cs                    ← grid data model
//     Player.cs                  ← player state & movement
//   Generation/
//     LevelGenerator.cs          ← procedural level builder + BFS solver check
//   Rendering/
//     IsometricRenderer.cs       ← 3D-style tile drawing + animations
//   UI/
//     InputHandler.cs            ← key → GameAction mapping
//     MainMenu.cs                ← animated title & instructions screen
//   Game/
//     GameLoop.cs                ← game state machine, session orchestrator
// =============================================================================

using System;
using CubePathGame.Game;
using CubePathGame.Generation;
using CubePathGame.Rendering;
using CubePathGame.UI;

// -----------------------------------------------------------------
// Console setup
// -----------------------------------------------------------------

try { Console.CursorVisible = false; }
catch { /* Some terminals don't support this — safe to ignore */ }

// Enable Unicode on Windows (important for block characters & arrows)
try { Console.OutputEncoding = System.Text.Encoding.UTF8; }
catch { }

// -----------------------------------------------------------------
// Dependency construction (manual DI — no framework needed)
//
// We create concrete objects here and pass them inward.
// Each class depends on abstractions it receives, not on the
// classes it creates itself — this is the Dependency Inversion Principle.
// -----------------------------------------------------------------

var renderer  = new IsometricRenderer();
var levelGen  = new LevelGenerator();
var mainMenu  = new MainMenu(renderer);

// -----------------------------------------------------------------
// Application loop
// -----------------------------------------------------------------

bool appRunning = true;

while (appRunning)
{
    var choice = mainMenu.Show();

    switch (choice)
    {
        case MenuChoice.StartGame:
            // Each call to GameLoop.Run() is a self-contained session
            var game = new GameLoop(renderer, levelGen);
            game.Run();
            break;

        case MenuChoice.Instructions:
            mainMenu.ShowInstructions();
            break;

        case MenuChoice.Exit:
            appRunning = false;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  Thanks for playing Cube Path Game!  Goodbye!\n");
            Console.ResetColor();
            break;
    }
}

// Restore cursor on graceful exit
try { Console.CursorVisible = true; }
catch { }

