// =============================================================================
// Core/Grid.cs
// The game's data model: a 2D array of CellType with traversal tracking.
// This class knows NOTHING about how to render itself — that's the
// Renderer's job. This separation is the Open/Closed Principle in action.
// =============================================================================

using System.Collections.Generic;

namespace CubePathGame.Core
{
    /// <summary>
    /// Stores the state of every cell in the level grid and tracks progress.
    /// Renderers read from this class; the GameLoop writes to it.
    /// </summary>
    public class Grid
    {
        // -----------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------

        public int Width  { get; }
        public int Height { get; }

        /// <summary>Total walkable cells (Path + Powerup) the player must visit.</summary>
        public int TotalPathCells { get; private set; }

        /// <summary>How many of those cells have been visited so far.</summary>
        public int VisitedPathCells { get; private set; }

        /// <summary>True when every required cell has been stepped on.</summary>
        public bool IsComplete => VisitedPathCells >= TotalPathCells;

        // The internal 2D cell array — private so only Grid controls it
        private readonly CellType[,] _cells;

        // -----------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------

        public Grid(int width, int height)
        {
            Width  = width;
            Height = height;
            _cells = new CellType[height, width];
        }

        // -----------------------------------------------------------------
        // Cell Access (read/write)
        // -----------------------------------------------------------------

        /// <summary>Gets the type of cell at the given position.</summary>
        public CellType GetCell(Position pos) => _cells[pos.Row, pos.Col];

        /// <summary>Sets the type of cell at the given position.</summary>
        public void SetCell(Position pos, CellType type) =>
            _cells[pos.Row, pos.Col] = type;

        // -----------------------------------------------------------------
        // Boundary & Walkability
        // -----------------------------------------------------------------

        /// <summary>Returns true if the position falls within the grid.</summary>
        public bool IsInBounds(Position pos) =>
            pos.Row >= 0 && pos.Row < Height &&
            pos.Col >= 0 && pos.Col < Width;

        /// <summary>
        /// Returns true if the player can move onto this cell.
        /// A Visited cell cannot be re-entered — that is the core puzzle rule.
        /// </summary>
        public bool IsWalkable(Position pos)
        {
            if (!IsInBounds(pos)) return false;
            var cell = GetCell(pos);
            return cell == CellType.Path || cell == CellType.Powerup;
        }

        /// <summary>Returns true if the player can move in at least one direction.</summary>
        public bool HasAnyWalkableNeighbour(Position from)
        {
            foreach (var neighbour in from.Neighbours())
                if (IsWalkable(neighbour)) return true;
            return false;
        }

        // -----------------------------------------------------------------
        // Progress Tracking
        // -----------------------------------------------------------------

        /// <summary>
        /// Counts and caches the total number of path+powerup cells.
        /// Call this once after the level is fully generated.
        /// </summary>
        public void RecalculateTotalPathCells()
        {
            TotalPathCells = 0;
            for (int r = 0; r < Height; r++)
                for (int c = 0; c < Width; c++)
                {
                    var t = _cells[r, c];
                    if (t == CellType.Path || t == CellType.Powerup)
                        TotalPathCells++;
                }
        }

        /// <summary>
        /// Marks a cell as visited and increments the visited counter.
        /// Returns true if the cell was a Powerup (for bonus scoring).
        /// </summary>
        public bool VisitCell(Position pos)
        {
            bool wasPowerup = _cells[pos.Row, pos.Col] == CellType.Powerup;
            _cells[pos.Row, pos.Col] = CellType.Visited;
            VisitedPathCells++;
            return wasPowerup;
        }

        // -----------------------------------------------------------------
        // Utility
        // -----------------------------------------------------------------

        /// <summary>Returns all positions with a given cell type.</summary>
        public List<Position> GetAllCellsOfType(CellType type)
        {
            var result = new List<Position>();
            for (int r = 0; r < Height; r++)
                for (int c = 0; c < Width; c++)
                    if (_cells[r, c] == type)
                        result.Add(new Position(r, c));
            return result;
        }
    }
}
