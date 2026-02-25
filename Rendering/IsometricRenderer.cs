// =============================================================================
// Rendering/IsometricRenderer.cs
//
// Draws the game grid in a faux-isometric (2.5D) style using Unicode
// block characters and ANSI colours.  Every cell is rendered as a small
// raised tile so the scene looks like looking at a board from above-left.
//
// Tile anatomy (3-row high, 4-col wide per cell):
//
//   Row 0:  "▄▄▄▄"   ← top face top edge (darker shade)
//   Row 1:  "█  █"   ← top face body (main colour)
//   Row 2:  "▀▀▀▀"   ← bottom bevel / shadow
//
// For walls the tile is doubled in height to look raised.
// =============================================================================

using System;
using System.Threading;
using CubePathGame.Core;  // Grid, Player, Position, CellType, Direction

namespace CubePathGame.Rendering
{
    /// <summary>
    /// Renders the Grid and Player in a pseudo-3D isometric style.
    /// Also owns all console animations (splash, transitions, particle bursts).
    ///
    /// Design note: the renderer is deliberately stateless between frames —
    /// it re-draws the full scene from the Grid data each call.  This keeps
    /// the rendering logic simple and predictable.
    /// </summary>
    public class IsometricRenderer
    {
        // -----------------------------------------------------------------
        // Layout constants  (all in console character units)
        // -----------------------------------------------------------------

        private const int CellW  = 4;   // characters wide per tile
        private const int CellH  = 2;   // character rows per tile (path/visited)
        private const int WallH  = 3;   // character rows for a wall tile

        // Where on screen the grid top-left starts (leaves room for the HUD)
        private const int OriginRow = 7;
        private const int OriginCol = 4;

        // -----------------------------------------------------------------
        // Palette (Console colours used throughout)
        // -----------------------------------------------------------------

        // Floor tiles
        private static readonly ConsoleColor FloorTop    = ConsoleColor.DarkCyan;
        private static readonly ConsoleColor FloorSide   = ConsoleColor.DarkBlue;

        // Visited tiles
        private static readonly ConsoleColor VisitedTop  = ConsoleColor.Green;
        private static readonly ConsoleColor VisitedSide = ConsoleColor.DarkGreen;
        private static readonly ConsoleColor TrailMark   = ConsoleColor.Green;

        // Wall tiles
        private static readonly ConsoleColor WallTop     = ConsoleColor.DarkGray;
        private static readonly ConsoleColor WallFace    = ConsoleColor.Gray;
        private static readonly ConsoleColor WallShadow  = ConsoleColor.DarkRed;

        // Player cube
        private static readonly ConsoleColor CubeTop    = ConsoleColor.Cyan;
        private static readonly ConsoleColor CubeFace   = ConsoleColor.White;
        private static readonly ConsoleColor CubeShine  = ConsoleColor.Yellow;

        // Powerup
        private static readonly ConsoleColor GemTop     = ConsoleColor.Yellow;
        private static readonly ConsoleColor GemGlow    = ConsoleColor.DarkYellow;

        // -----------------------------------------------------------------
        // Frame state (for animation tick)
        // -----------------------------------------------------------------

        private int _animTick = 0;  // increments each render call

        // -----------------------------------------------------------------
        // Public: full-scene render
        // -----------------------------------------------------------------

        /// <summary>
        /// Clears the console and redraws everything: HUD, grid, legend.
        /// </summary>
        public void RenderFrame(Grid grid, Player player, int level, string statusMsg = "")
        {
            _animTick++;
            Console.Clear();

            DrawHud(grid, player, level, statusMsg);
            DrawGrid(grid, player);
            DrawLegend(grid.Height);
        }

        // -----------------------------------------------------------------
        // HUD (top panel)
        // -----------------------------------------------------------------

        private void DrawHud(Grid grid, Player player, int level, string statusMsg)
        {
            // Animated title shimmer — cycles through cyan/white/yellow
            ConsoleColor titleColor = (_animTick % 6) switch
            {
                0 or 1 => ConsoleColor.Cyan,
                2 or 3 => ConsoleColor.White,
                _      => ConsoleColor.Yellow
            };

            SetColor(titleColor);
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("  ╔══════════════════════════════════════════════╗");
            Console.WriteLine($"  ║   ▓▓ CUBE PATH — LEVEL {level,-2}                    ║");
            Console.WriteLine("  ╚══════════════════════════════════════════════╝");
            Console.ResetColor();

            // Progress bar
            int filled   = grid.TotalPathCells > 0
                           ? (int)((double)grid.VisitedPathCells / grid.TotalPathCells * 30)
                           : 0;
            int remaining = 30 - filled;

            SetColor(ConsoleColor.DarkGray);  Console.Write("  Progress [");
            SetColor(ConsoleColor.Green);     Console.Write(new string('█', filled));
            SetColor(ConsoleColor.DarkGray);  Console.Write(new string('░', remaining));
            SetColor(ConsoleColor.DarkGray);  Console.Write("] ");
            SetColor(ConsoleColor.White);
            Console.WriteLine($"{grid.VisitedPathCells}/{grid.TotalPathCells} cells");
            Console.ResetColor();

            // Score / moves row
            SetColor(ConsoleColor.Cyan);    Console.Write($"  Score: {player.Score,-7}");
            SetColor(ConsoleColor.Yellow);  Console.Write($"Moves: {player.MoveCount,-6}");
            SetColor(ConsoleColor.Magenta); Console.Write($"Left:  {grid.TotalPathCells - grid.VisitedPathCells}");
            Console.ResetColor();

            if (!string.IsNullOrEmpty(statusMsg))
            {
                Console.Write("   ");
                SetColor(ConsoleColor.Yellow);
                Console.Write(statusMsg);
                Console.ResetColor();
            }
            Console.WriteLine();
        }

        // -----------------------------------------------------------------
        // Grid drawing — isometric tile loop
        // -----------------------------------------------------------------

        private void DrawGrid(Grid grid, Player player)
        {
            // Top isometric "roof" edge of the entire grid
            SetColor(ConsoleColor.DarkGray);
            Console.SetCursorPosition(OriginCol - 2, OriginRow - 1);
            Console.Write("╔" + new string('═', grid.Width * CellW) + "╗");

            for (int r = 0; r < grid.Height; r++)
            {
                for (int row = 0; row < CellH; row++) // each cell takes CellH console rows
                {
                    // Left wall of grid
                    int consoleRow = OriginRow + r * CellH + row;
                    Console.SetCursorPosition(OriginCol - 2, consoleRow);
                    SetColor(ConsoleColor.DarkGray);
                    Console.Write("║");

                    for (int c = 0; c < grid.Width; c++)
                    {
                        Console.SetCursorPosition(OriginCol + c * CellW, consoleRow);
                        bool isPlayer = (player.Position.Row == r && player.Position.Col == c);

                        if (isPlayer)
                            DrawPlayerTile(row, player);
                        else
                            DrawCellTile(grid.GetCell(new Position(r, c)), row, r, c);
                    }

                    // Right wall
                    Console.SetCursorPosition(OriginCol + grid.Width * CellW, consoleRow);
                    SetColor(ConsoleColor.DarkGray);
                    Console.Write("║");
                }
            }

            // Bottom border
            int bottomRow = OriginRow + grid.Height * CellH;
            Console.SetCursorPosition(OriginCol - 2, bottomRow);
            SetColor(ConsoleColor.DarkGray);
            Console.Write("╚" + new string('═', grid.Width * CellW) + "╝");
            Console.ResetColor();
        }

        // -----------------------------------------------------------------
        // Individual tile renderers
        // -----------------------------------------------------------------

        private void DrawCellTile(CellType cell, int tileRow, int gridRow, int gridCol)
        {
            switch (cell)
            {
                case CellType.Path:
                    DrawFloorTile(tileRow);
                    break;

                case CellType.Visited:
                    DrawVisitedTile(tileRow, gridRow, gridCol);
                    break;

                case CellType.Wall:
                    DrawWallTile(tileRow, gridRow, gridCol);
                    break;

                case CellType.Powerup:
                    DrawPowerupTile(tileRow);
                    break;
            }
        }

        // Floor (unvisited path)
        private void DrawFloorTile(int tileRow)
        {
            if (tileRow == 0)
            {
                SetColor(FloorTop);
                Console.Write("▄▄▄▄"); // top face top edge
            }
            else
            {
                SetColor(FloorSide);
                Console.Write("░░░░"); // subtle texture on floor face
            }
        }

        // Visited path — shows a trail dot in the centre
        private void DrawVisitedTile(int tileRow, int r, int c)
        {
            if (tileRow == 0)
            {
                SetColor(VisitedTop);
                Console.Write("▄▄▄▄");
            }
            else
            {
                SetColor(VisitedSide);
                Console.Write("─");
                SetColor(TrailMark);
                Console.Write("◦◦");     // trail crumbs
                SetColor(VisitedSide);
                Console.Write("─");
            }
        }

        // Wall — drawn taller via extra shading rows
        private void DrawWallTile(int tileRow, int r, int c)
        {
            // We reuse tileRow 0 and 1 to simulate a tall block
            if (tileRow == 0)
            {
                SetColor(WallTop);
                Console.Write("████"); // solid top
            }
            else
            {
                // Brick-like pattern alternating by row/col for texture
                bool odd = ((r + c) % 2 == 0);
                SetColor(odd ? WallFace : WallShadow);
                Console.Write("▓");
                SetColor(WallFace);
                Console.Write("██");
                SetColor(odd ? WallShadow : WallFace);
                Console.Write("▓");
            }
        }

        // Powerup — animated gem pulsing between colours
        private void DrawPowerupTile(int tileRow)
        {
            bool pulse = (_animTick % 4 < 2); // blink every 2 frames

            if (tileRow == 0)
            {
                SetColor(pulse ? GemTop : GemGlow);
                Console.Write("▄◆▄▄");
            }
            else
            {
                SetColor(pulse ? ConsoleColor.Yellow : ConsoleColor.DarkYellow);
                Console.Write("✦");
                SetColor(ConsoleColor.White);
                Console.Write("✧✦");
                SetColor(pulse ? ConsoleColor.DarkYellow : ConsoleColor.Yellow);
                Console.Write("✦");
            }
        }

        // Player cube — shows a "face" that changes based on movement direction
        private void DrawPlayerTile(int tileRow, Player player)
        {
            if (tileRow == 0)
            {
                // Shining top face
                SetColor(CubeShine);
                Console.Write("▄");
                SetColor(CubeTop);
                Console.Write("▄▄");
                SetColor(CubeShine);
                Console.Write("▄");
            }
            else
            {
                // Front face with directional "eyes" (arrow-like indicator)
                string face = player.LastDirection switch
                {
                    Direction.Up    => "▲",
                    Direction.Down  => "▼",
                    Direction.Left  => "◄",
                    Direction.Right => "►",
                    _               => "■"
                };

                SetColor(CubeFace);
                Console.Write("[");
                SetColor(CubeTop);
                Console.Write(face + " ");
                SetColor(CubeFace);
                Console.Write("]");
            }
        }

        // -----------------------------------------------------------------
        // Legend (below the grid)
        // -----------------------------------------------------------------

        private void DrawLegend(int gridHeight)
        {
            int legendRow = OriginRow + gridHeight * CellH + 2;
            Console.SetCursorPosition(0, legendRow);

            SetColor(ConsoleColor.DarkGray);
            Console.WriteLine("  ┌─────────────────────────────────────────────────────────┐");
            Console.Write("  │ ");
            SetColor(CubeTop);     Console.Write("[►]"); SetColor(ConsoleColor.Gray); Console.Write(" You  ");
            SetColor(FloorTop);    Console.Write("▄▄"); SetColor(ConsoleColor.Gray); Console.Write(" Path  ");
            SetColor(VisitedTop);  Console.Write("◦◦"); SetColor(ConsoleColor.Gray); Console.Write(" Visited  ");
            SetColor(WallFace);    Console.Write("██"); SetColor(ConsoleColor.Gray); Console.Write(" Wall  ");
            SetColor(GemTop);      Console.Write("✦"); SetColor(ConsoleColor.Gray); Console.Write(" Power+50");
            SetColor(ConsoleColor.DarkGray); Console.WriteLine("  │");
            Console.WriteLine("  │  W/↑ Up  S/↓ Down  A/← Left  D/→ Right  R=Restart  Q=Quit │");
            Console.WriteLine("  └─────────────────────────────────────────────────────────┘");
            Console.ResetColor();
        }

        // -----------------------------------------------------------------
        // Animation: slide-in wipe when loading a level
        // -----------------------------------------------------------------

        /// <summary>
        /// Plays a brief curtain-wipe animation before a new level starts.
        /// Draws horizontal scanlines top-to-bottom then clears them.
        /// </summary>
        public void PlayLevelIntroAnimation(int level)
        {
            Console.Clear();
            int consoleH = Math.Min(Console.WindowHeight, 30);
            int consoleW = Math.Min(Console.WindowWidth,  80);

            // Draw scanlines sweeping downward
            for (int row = 0; row < consoleH; row++)
            {
                Console.SetCursorPosition(0, row);
                ConsoleColor c = (row % 3 == 0) ? ConsoleColor.DarkCyan
                               : (row % 3 == 1) ? ConsoleColor.DarkBlue
                               :                  ConsoleColor.Black;
                SetColor(c);
                Console.Write(new string('▓', consoleW));
                Thread.Sleep(12);
            }

            // Flash the level number in the centre
            string msg    = $"  ▶  LEVEL {level}  ◀  ";
            int    msgCol = Math.Max(0, (consoleW - msg.Length) / 2);
            int    msgRow = consoleH / 2;

            Console.SetCursorPosition(msgCol, msgRow - 1);
            SetColor(ConsoleColor.Black);
            Console.Write(new string('█', msg.Length + 4));

            Console.SetCursorPosition(msgCol, msgRow);
            SetColor(ConsoleColor.Black);   Console.Write("██");
            SetColor(ConsoleColor.Yellow);  Console.Write(msg);
            SetColor(ConsoleColor.Black);   Console.Write("██");

            Console.SetCursorPosition(msgCol, msgRow + 1);
            SetColor(ConsoleColor.Black);
            Console.Write(new string('█', msg.Length + 4));

            Thread.Sleep(600);

            // Wipe back up
            for (int row = consoleH - 1; row >= 0; row--)
            {
                Console.SetCursorPosition(0, row);
                Console.Write(new string(' ', consoleW));
                Thread.Sleep(8);
            }

            Console.ResetColor();
        }

        // -----------------------------------------------------------------
        // Animation: powerup particle burst at player position
        // -----------------------------------------------------------------

        /// <summary>
        /// Renders a short sparkle effect when a powerup is collected.
        /// Runs for a fixed number of frames without blocking input.
        /// </summary>
        public void PlayPowerupEffect(Player player)
        {
            int screenRow = OriginRow + player.Position.Row * CellH;
            int screenCol = OriginCol + player.Position.Col * CellW;

            string[] frames = { "✦✦✦✦", "✧✦✧✦", "  ✦ ", "    " };

            foreach (var frame in frames)
            {
                Console.SetCursorPosition(screenCol, screenRow);
                SetColor(ConsoleColor.Yellow);
                Console.Write(frame);
                Console.ResetColor();
                Thread.Sleep(60);
            }
        }

        // -----------------------------------------------------------------
        // Animation: "stuck" red flash for invalid moves
        // -----------------------------------------------------------------

        /// <summary>
        /// Briefly flashes the player tile red when they can't move.
        /// </summary>
        public void PlayBlockedEffect(Player player)
        {
            int screenRow = OriginRow + player.Position.Row * CellH + 1;
            int screenCol = OriginCol + player.Position.Col * CellW;

            for (int i = 0; i < 2; i++)
            {
                Console.SetCursorPosition(screenCol, screenRow);
                SetColor(ConsoleColor.Red);
                Console.Write("XXXX");
                Thread.Sleep(70);
                Console.SetCursorPosition(screenCol, screenRow);
                SetColor(CubeFace);
                Console.Write("[■ ]");
                Thread.Sleep(70);
            }
            Console.ResetColor();
        }

        // -----------------------------------------------------------------
        // Full-screen animated win screen
        // -----------------------------------------------------------------

        public void PlayWinScreen(int level, int score, int bonus, int moves,
                                  int totalScore, bool gameOver)
        {
            Console.Clear();
            int w = Math.Min(Console.WindowWidth, 80);

            // Cascade of stars from top to bottom
            for (int row = 0; row < 6; row++)
            {
                Console.SetCursorPosition(0, row);
                ConsoleColor c = (row % 2 == 0) ? ConsoleColor.DarkGreen : ConsoleColor.Green;
                SetColor(c);
                Console.Write(new string(row % 2 == 0 ? '▓' : '░', w));
                Thread.Sleep(40);
            }

            Console.SetCursorPosition(2, 2);
            SetColor(ConsoleColor.Yellow);

            if (gameOver)
            {
                Console.Write("  🏆  ALL LEVELS COMPLETE — YOU WIN THE GAME!  🏆");
            }
            else
            {
                Console.Write($"  ✔  LEVEL {level} CLEARED!");
            }

            Thread.Sleep(300);

            Console.SetCursorPosition(0, 8);
            Console.ResetColor();

            PrintLine("  ┌──────────────────────────────────┐", ConsoleColor.DarkGreen);
            PrintLine($"  │  Level Score    +{levelStr(score)}", ConsoleColor.White);
            PrintLine($"  │  Efficiency     +{levelStr(bonus)}", ConsoleColor.Cyan);
            PrintLine($"  │  Moves Used      {moves,-17}│", ConsoleColor.Gray);
            PrintLine("  ├──────────────────────────────────┤", ConsoleColor.DarkGreen);
            PrintLine($"  │  TOTAL SCORE    {totalScore,-18}│", ConsoleColor.Yellow);
            PrintLine("  └──────────────────────────────────┘", ConsoleColor.DarkGreen);

            Console.WriteLine();
            SetColor(ConsoleColor.DarkGray);
            Console.WriteLine(gameOver
                ? "  Press any key to return to the menu..."
                : "  Press any key to continue to the next level...");
            Console.ResetColor();
        }

        // -----------------------------------------------------------------
        // Full-screen animated lose / stuck screen
        // -----------------------------------------------------------------

        public void PlayLoseScreen(int visited, int total)
        {
            Console.Clear();
            int w = Math.Min(Console.WindowWidth, 80);

            for (int row = 0; row < 5; row++)
            {
                Console.SetCursorPosition(0, row);
                SetColor(row % 2 == 0 ? ConsoleColor.DarkRed : ConsoleColor.Red);
                Console.Write(new string('▓', w));
                Thread.Sleep(40);
            }

            Console.SetCursorPosition(2, 2);
            SetColor(ConsoleColor.White);
            Console.Write("  ✖  STUCK! No valid moves remain.");

            Thread.Sleep(200);
            Console.SetCursorPosition(0, 7);
            Console.ResetColor();

            PrintLine("  ┌──────────────────────────────────┐", ConsoleColor.DarkRed);
            PrintLine($"  │  Cells Covered  {visited}/{total,-17}│", ConsoleColor.White);
            PrintLine($"  │  Remaining      {total - visited,-17}│", ConsoleColor.Red);
            PrintLine("  └──────────────────────────────────┘", ConsoleColor.DarkRed);

            Console.WriteLine();
            SetColor(ConsoleColor.DarkGray);
            Console.WriteLine("  [R] Retry Level    [Q] Return to Menu");
            Console.ResetColor();
        }

        // -----------------------------------------------------------------
        // Animated splash / main menu
        // -----------------------------------------------------------------

        public void DrawMenuTitle()
        {
            Console.Clear();

            // Animate the title dropping in line by line
            string[] art = {
                @"   ██████╗██╗   ██╗██████╗ ███████╗",
                @"  ██╔════╝██║   ██║██╔══██╗██╔════╝",
                @"  ██║     ██║   ██║██████╔╝█████╗  ",
                @"  ██║     ██║   ██║██╔══██╗██╔══╝  ",
                @"  ╚██████╗╚██████╔╝██████╔╝███████╗",
                @"   ╚═════╝ ╚═════╝ ╚═════╝ ╚══════╝",
                @"  ══════ P A T H   G A M E ══════"
            };

            ConsoleColor[] palette = {
                ConsoleColor.DarkCyan, ConsoleColor.DarkCyan,
                ConsoleColor.Cyan,     ConsoleColor.Cyan,
                ConsoleColor.White,    ConsoleColor.White,
                ConsoleColor.Yellow
            };

            for (int i = 0; i < art.Length; i++)
            {
                Console.SetCursorPosition(0, i + 1);
                SetColor(palette[i]);
                Console.WriteLine(art[i]);
                Thread.Sleep(45);
            }

            Console.ResetColor();
            Console.WriteLine();
        }

        // -----------------------------------------------------------------
        // Utility helpers
        // -----------------------------------------------------------------

        private static void SetColor(ConsoleColor color) =>
            Console.ForegroundColor = color;

        private static string levelStr(int pts) =>
            $"{pts} pts{new string(' ', Math.Max(0, 12 - pts.ToString().Length))}│";

        private static void PrintLine(string text, ConsoleColor color)
        {
            SetColor(color);
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
