// =============================================================================
// UI/InputHandler.cs
// Translates raw ConsoleKey presses into strongly-typed GameAction values.
//
// Centralising key bindings here means changing a keybinding requires
// editing exactly ONE place instead of hunting through game logic.
// =============================================================================

using System;
using CubePathGame.Core;

namespace CubePathGame.UI
{
    /// <summary>
    /// Every action a player can request. The GameLoop switches on this enum
    /// rather than on raw ConsoleKey values — keeps game logic key-agnostic.
    /// </summary>
    public enum GameAction
    {
        None,
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        Restart,
        Quit,
        Confirm   // Enter / Space — used for "press any key" prompts
    }

    /// <summary>
    /// Reads a single keypress and returns the corresponding GameAction.
    /// </summary>
    public static class InputHandler
    {
        /// <summary>
        /// Blocks until a key is pressed, then maps it to a GameAction.
        /// </summary>
        public static GameAction ReadAction()
        {
            var key = Console.ReadKey(intercept: true); // intercept hides the keypress echo

            return key.Key switch
            {
                ConsoleKey.W         => GameAction.MoveUp,
                ConsoleKey.UpArrow   => GameAction.MoveUp,

                ConsoleKey.S         => GameAction.MoveDown,
                ConsoleKey.DownArrow => GameAction.MoveDown,

                ConsoleKey.A         => GameAction.MoveLeft,
                ConsoleKey.LeftArrow => GameAction.MoveLeft,

                ConsoleKey.D         => GameAction.MoveRight,
                ConsoleKey.RightArrow=> GameAction.MoveRight,

                ConsoleKey.R         => GameAction.Restart,
                ConsoleKey.Q         => GameAction.Quit,
                ConsoleKey.Escape    => GameAction.Quit,

                ConsoleKey.Enter     => GameAction.Confirm,
                ConsoleKey.Spacebar  => GameAction.Confirm,

                _                    => GameAction.None
            };
        }

        /// <summary>
        /// Converts a GameAction to the Direction enum used by Player.TryMove.
        /// Returns null for non-movement actions.
        /// </summary>
        public static Direction? ActionToDirection(GameAction action) =>
            action switch
            {
                GameAction.MoveUp    => Direction.Up,
                GameAction.MoveDown  => Direction.Down,
                GameAction.MoveLeft  => Direction.Left,
                GameAction.MoveRight => Direction.Right,
                _                    => null
            };
    }
}
