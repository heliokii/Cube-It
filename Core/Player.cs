// =============================================================================
// Core/Player.cs
// The player's state machine: position, score, move history.
// Single Responsibility: knows HOW the player moves and scores,
// but not HOW to draw the player (that's the renderer's job).
// =============================================================================

using System;

namespace CubePathGame.Core
{
    /// <summary>
    /// Direction the player wants to move. An enum keeps it readable
    /// everywhere rather than passing magic integers around.
    /// </summary>
    public enum Direction { Up, Down, Left, Right }

    /// <summary>
    /// Result of a movement attempt, so callers know exactly what happened.
    /// </summary>
    public enum MoveResult
    {
        /// <summary>Movement was successful.</summary>
        Success,
        /// <summary>Target cell is a wall or out of bounds.</summary>
        Blocked,
        /// <summary>Target cell was already visited (no re-entry rule).</summary>
        AlreadyVisited,
        /// <summary>Player picked up a powerup during this move.</summary>
        PickedUpPowerup
    }

    /// <summary>
    /// Manages the cube player's position on the grid, move history, and score.
    /// </summary>
    public class Player
    {
        // -----------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------

        public Position Position { get; private set; }
        public int Score         { get; private set; }
        public int MoveCount     { get; private set; }

        /// <summary>Direction the player last moved (for rendering the cube face).</summary>
        public Direction LastDirection { get; private set; } = Direction.Right;

        // -----------------------------------------------------------------
        // Initialisation
        // -----------------------------------------------------------------

        /// <summary>Place the player at a starting position without counting a move.</summary>
        public void PlaceAt(Position pos)
        {
            Position = pos;
        }

        /// <summary>Reset score and moves for a new game session.</summary>
        public void ResetStats()
        {
            Score     = 0;
            MoveCount = 0;
        }

        // -----------------------------------------------------------------
        // Movement
        // -----------------------------------------------------------------

        /// <summary>
        /// Attempts to move the player one step in the given direction.
        /// Modifies the Grid state if successful.
        /// Returns a MoveResult describing what happened.
        /// </summary>
        public MoveResult TryMove(Direction direction, Grid grid)
        {
            LastDirection = direction;

            // Work out which cell we'd land on
            Position target = direction switch
            {
                Direction.Up    => Position.Up(),
                Direction.Down  => Position.Down(),
                Direction.Left  => Position.Left(),
                Direction.Right => Position.Right(),
                _               => Position
            };

            // Boundary or wall
            if (!grid.IsInBounds(target))  return MoveResult.Blocked;
            var cellType = grid.GetCell(target);
            if (cellType == CellType.Wall)    return MoveResult.Blocked;
            if (cellType == CellType.Visited) return MoveResult.AlreadyVisited;

            // Legal move — update position and stamp the grid
            Position = target;
            MoveCount++;

            bool wasPowerup = grid.VisitCell(target);
            return wasPowerup ? MoveResult.PickedUpPowerup : MoveResult.Success;
        }

        // -----------------------------------------------------------------
        // Scoring
        // -----------------------------------------------------------------

        public void AddScore(int points) => Score += points;

        /// <summary>
        /// Calculates an efficiency bonus based on how few moves were used.
        /// Fewer moves relative to the number of cells = bigger bonus.
        /// </summary>
        public int CalculateEfficiencyBonus(int totalCells)
        {
            if (MoveCount <= totalCells)            return 500;
            if (MoveCount <= totalCells * 2)        return 200;
            return 50;
        }
    }
}
