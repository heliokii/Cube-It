// =============================================================================
// Game/GameLoop.cs
// The central game state machine.
//
// The GameLoop's job is purely coordination:
//   "Given the current state, decide what happens next."
//
// It creates no UI text itself — it calls the Renderer.
// It generates no grids itself — it calls the LevelGenerator.
// It moves no player itself — it calls Player.TryMove().
//
// This is the Mediator / Façade pattern:
//   every other component is self-contained, and the GameLoop wires them together.
// =============================================================================

using System;
using System.Threading;
using CubePathGame.Core;
using CubePathGame.Generation;
using CubePathGame.Rendering;
using CubePathGame.UI;

namespace CubePathGame.Game
{
    /// <summary>
    /// Possible high-level states the game can be in.
    /// The loop switch on this each iteration.
    /// </summary>
    public enum GameState
    {
        Playing,    // Normal gameplay — accepting input
        LevelWon,   // Player just covered the last cell
        LevelLost,  // Player is stuck (no walkable neighbours)
        GameOver,   // All 5 levels cleared
        Quitting    // Player pressed Q
    }

    /// <summary>
    /// Orchestrates a full game session from level 1 to level 5 (or quit).
    /// </summary>
    public class GameLoop
    {
        // -----------------------------------------------------------------
        // Dependencies (injected in constructor — Dependency Inversion)
        // -----------------------------------------------------------------

        private readonly IsometricRenderer _renderer;
        private readonly LevelGenerator    _levelGen;
        private readonly Player            _player;

        // -----------------------------------------------------------------
        // Mutable session state
        // -----------------------------------------------------------------

        private Grid      _grid       = null!;  // set in LoadLevel
        private int       _level      = 1;
        private GameState _state      = GameState.Playing;
        private string    _statusMsg  = "";     // message shown in the HUD

        // Maximum levels in a game session
        private const int MaxLevels = 5;

        // -----------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------

        public GameLoop(IsometricRenderer renderer, LevelGenerator levelGen)
        {
            _renderer = renderer;
            _levelGen = levelGen;
            _player   = new Player();
        }

        // -----------------------------------------------------------------
        // Public entry point
        // -----------------------------------------------------------------

        /// <summary>
        /// Runs a complete game session from level 1 until the player wins,
        /// quits, or finishes all levels.
        /// </summary>
        public void Run()
        {
            _level = 1;
            _player.ResetStats();
            LoadLevel(_level);

            // The main loop — runs until the session ends
            while (_state != GameState.Quitting && _state != GameState.GameOver)
            {
                switch (_state)
                {
                    case GameState.Playing:
                        HandlePlaying();
                        break;

                    case GameState.LevelWon:
                        HandleLevelWon();
                        break;

                    case GameState.LevelLost:
                        HandleLevelLost();
                        break;
                }
            }

            if (_state == GameState.GameOver)
                HandleGameOver();
        }

        // -----------------------------------------------------------------
        // State handlers
        // -----------------------------------------------------------------

        /// <summary>
        /// Normal gameplay: render the frame, read input, apply the move.
        /// </summary>
        private void HandlePlaying()
        {
            // Draw the current frame
            _renderer.RenderFrame(_grid, _player, _level, _statusMsg);
            _statusMsg = ""; // clear one-frame messages after drawing

            // Read the next action
            var action = InputHandler.ReadAction();

            if (action == GameAction.Quit)    { _state = GameState.Quitting; return; }
            if (action == GameAction.Restart) { LoadLevel(_level); return; }

            // Try to move
            var dir = InputHandler.ActionToDirection(action);
            if (dir == null) return;  // not a movement key — do nothing

            var result = _player.TryMove(dir.Value, _grid);

            switch (result)
            {
                case MoveResult.Blocked:
                    // Wall or boundary — short flash, no re-render needed here
                    _renderer.PlayBlockedEffect(_player);
                    _statusMsg = "  ✖ Blocked!";
                    break;

                case MoveResult.AlreadyVisited:
                    _renderer.PlayBlockedEffect(_player);
                    _statusMsg = "  ✖ Already visited!";
                    break;

                case MoveResult.PickedUpPowerup:
                    _player.AddScore(50);
                    _renderer.PlayPowerupEffect(_player);
                    _statusMsg = "  ✦ +50 Powerup!";
                    break;

                case MoveResult.Success:
                    // Check if that last move finished the level
                    if (_grid.IsComplete)
                    {
                        _state = GameState.LevelWon;
                        return;
                    }
                    // Check if the player is now stuck
                    if (!_grid.HasAnyWalkableNeighbour(_player.Position))
                    {
                        // Only call it a loss if there are still uncovered cells
                        if (!_grid.IsComplete)
                            _state = GameState.LevelLost;
                    }
                    break;
            }
        }

        /// <summary>
        /// Player completed the level: award score, animate, advance.
        /// </summary>
        private void HandleLevelWon()
        {
            int bonus      = _player.CalculateEfficiencyBonus(_grid.TotalPathCells);
            int levelScore = 100 * _level + bonus;
            _player.AddScore(levelScore);

            bool isLastLevel = (_level >= MaxLevels);

            _renderer.PlayWinScreen(
                level:      _level,
                score:      levelScore,
                bonus:      bonus,
                moves:      _player.MoveCount,
                totalScore: _player.Score,
                gameOver:   isLastLevel
            );

            Console.ReadKey(intercept: true);

            if (isLastLevel)
            {
                _state = GameState.GameOver;
            }
            else
            {
                _level++;
                LoadLevel(_level);
            }
        }

        /// <summary>
        /// Player is stuck — show lose screen and await retry / quit decision.
        /// </summary>
        private void HandleLevelLost()
        {
            _renderer.PlayLoseScreen(_grid.VisitedPathCells, _grid.TotalPathCells);

            while (true)
            {
                var action = InputHandler.ReadAction();
                if (action == GameAction.Restart)
                {
                    LoadLevel(_level);
                    return;
                }
                if (action == GameAction.Quit)
                {
                    _state = GameState.Quitting;
                    return;
                }
                // R / Q only on this screen
            }
        }

        /// <summary>
        /// All levels cleared — congratulations pause then back to menu.
        /// </summary>
        private void HandleGameOver()
        {
            // The win screen with gameOver=true already showed.
            // Nothing else needed here — Run() will return and the menu will reappear.
        }

        // -----------------------------------------------------------------
        // Level loading
        // -----------------------------------------------------------------

        private void LoadLevel(int level)
        {
            _state = GameState.Playing;
            _statusMsg = "";

            // Play the curtain-wipe animation
            _renderer.PlayLevelIntroAnimation(level);

            // Generate a fresh randomised grid
            var (grid, startPos) = _levelGen.GenerateLevel(level);
            _grid = grid;

            // Place the player and mark the starting cell as visited
            _player.PlaceAt(startPos);
            _grid.VisitCell(startPos);
        }
    }
}
