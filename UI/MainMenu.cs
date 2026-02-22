// =============================================================================
// UI/MainMenu.cs
// Displays the animated title screen and handles menu navigation.
// Separated from game logic so the GameLoop stays uncluttered.
// =============================================================================

using System;
using CubePathGame.Rendering;

namespace CubePathGame.UI
{
    /// <summary>
    /// Possible outcomes from showing the main menu.
    /// </summary>
    public enum MenuChoice { StartGame, Instructions, Exit }

    /// <summary>
    /// Renders and handles input for the main menu screen.
    /// Uses the IsometricRenderer for the animated title so visual styles stay consistent.
    /// </summary>
    public class MainMenu
    {
        private readonly IsometricRenderer _renderer;

        public MainMenu(IsometricRenderer renderer)
        {
            _renderer = renderer;
        }

        /// <summary>
        /// Shows the menu and returns the player's choice.
        /// Loops until a valid option is selected.
        /// </summary>
        public MenuChoice Show()
        {
            while (true)
            {
                _renderer.DrawMenuTitle();

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  ┌─────────────────────────────┐");
                Console.WriteLine("  │                             │");
                Console.WriteLine("  │    1.  Start Game           │");
                Console.WriteLine("  │    2.  How To Play          │");
                Console.WriteLine("  │    3.  Exit                 │");
                Console.WriteLine("  │                             │");
                Console.WriteLine("  └─────────────────────────────┘");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("\n  Press [1], [2], or [3]: ");
                Console.ResetColor();

                var key = Console.ReadKey(intercept: true);
                if (key.KeyChar == '1') return MenuChoice.StartGame;
                if (key.KeyChar == '2') return MenuChoice.Instructions;
                if (key.KeyChar == '3') return MenuChoice.Exit;
                // Any other key: re-draw and ask again
            }
        }

        /// <summary>
        /// Prints the instructions page and waits for a keypress.
        /// </summary>
        public void ShowInstructions()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  ════════════════════════════════════════════");
            Console.WriteLine("           HOW  TO  PLAY  CUBE  PATH");
            Console.WriteLine("  ════════════════════════════════════════════");
            Console.ResetColor();

            string[] lines =
            {
                "",
                "  GOAL:",
                "    Move your cube over EVERY open floor tile on the grid.",
                "    You cannot step on a tile you've already visited.",
                "    Cover ALL tiles to complete the level!",
                "",
                "  CONTROLS:",
                "    W / ↑      Move Up",
                "    S / ↓      Move Down",
                "    A / ←      Move Left",
                "    D / →      Move Right",
                "    R          Restart current level",
                "    Q / Esc    Quit to main menu",
                "",
                "  TILES:",
                "    ▄▄▄▄  Floor — you must visit this",
                "    ◦◦    Visited — you've been here (no re-entry!)",
                "    ████  Wall — impassable raised block",
                "    ✦✦    Power-up — collect for +50 bonus points!",
                "    [►]   You — the cube!",
                "",
                "  SCORING:",
                "    +100 × level number when you complete the level",
                "    +500  efficiency bonus (≤ totalCells moves)",
                "    +200  efficiency bonus (≤ 2× totalCells moves)",
                "    +50   for each power-up collected",
                "",
                "  LEVELS: 5 total, grids grow from 5×5 up to 15×15.",
                "  Plan your route — every dead-end costs you!",
                ""
            };

            foreach (var line in lines)
            {
                Console.ForegroundColor = line.TrimStart().StartsWith("GOAL")
                                       || line.TrimStart().StartsWith("CONTROLS")
                                       || line.TrimStart().StartsWith("TILES")
                                       || line.TrimStart().StartsWith("SCORING")
                                       || line.TrimStart().StartsWith("LEVELS")
                                          ? ConsoleColor.Cyan
                                          : ConsoleColor.Gray;
                Console.WriteLine(line);
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Press any key to return to the menu...");
            Console.ResetColor();
            Console.ReadKey(intercept: true);
        }
    }
}
