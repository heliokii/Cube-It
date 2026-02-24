// =============================================================================
// Generation/LevelGenerator.cs
// Procedurally creates randomised, solvable grid layouts.
//
// Algorithm:
//   1. Fill grid randomly with walls at the configured density.
//   2. Pick a random walkable cell as the player start.
//   3. BFS from start to count reachable walkable cells.
//   4. If every walkable cell is reachable → solvable, keep it.
//      Otherwise regenerate (up to 100 attempts).
//   5. Sprinkle powerups onto random reachable cells.
// =============================================================================

using System;
using System.Collections.Generic;
using CubePathGame.Core;

namespace CubePathGame.Generation
{
    /// <summary>
    /// Produces randomised, connectivity-guaranteed Grid levels.
    /// Each call to GenerateLevel returns a fresh layout scaled to the
    /// requested difficulty level (1–5).
    /// </summary>
    public class LevelGenerator
    {
        // -----------------------------------------------------------------
        // Level scaling constants
        // -----------------------------------------------------------------

        private const int BaseSize       = 5;   // 5×5 at level 1
        private const int SizePerLevel   = 2;   // +2 columns/rows each level
        private const int MaxSize        = 15;  // cap at 15×15
        private const double BaseWalls   = 0.20;
        private const double WallPerLevel= 0.02;
        private const double MaxWalls    = 0.30;

        private readonly Random _rng;

        public LevelGenerator(Random? rng = null)
        {
            _rng = rng ?? new Random();
        }

        // -----------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------

        /// <summary>
        /// Generates a solvable level appropriate for the given level number.
        /// Returns the grid and the player's starting position.
        /// </summary>
        public (Grid grid, Position startPos) GenerateLevel(int levelNumber)
        {
            int size        = Math.Min(BaseSize + (levelNumber - 1) * SizePerLevel, MaxSize);
            double wallRate = Math.Min(BaseWalls + (levelNumber - 1) * WallPerLevel, MaxWalls);
            int powerups    = Math.Min((levelNumber / 2) + 1, 6);

            Grid?    grid     = null;
            Position startPos = default;
            int      attempts = 0;

            do
            {
                (grid, startPos) = BuildRandomGrid(size, size, wallRate, powerups);
                attempts++;

                // After many failures reduce wall density to guarantee a solvable map
                if (attempts == 60) wallRate = 0.15;

            } while (!IsSolvable(grid, startPos) && attempts < 100);

            // Final safety: if we still couldn't solve it after 100 attempts,
            // clear all walls and return an open grid (gameplay over perfection)
            if (!IsSolvable(grid!, startPos))
            {
                (grid, startPos) = BuildOpenGrid(size, size);
            }

            grid!.RecalculateTotalPathCells();
            return (grid, startPos);
        }

        // -----------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------

        private (Grid, Position) BuildRandomGrid(int width, int height,
                                                 double wallRate, int powerupCount)
        {
            var grid = new Grid(width, height);

            // Step 1: randomly place walls
            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                    grid.SetCell(new Position(r, c),
                        _rng.NextDouble() < wallRate ? CellType.Wall : CellType.Path);

            // Step 2: collect walkable cells and pick a start
            var walkable = new List<Position>();
            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                    if (grid.GetCell(new Position(r, c)) == CellType.Path)
                        walkable.Add(new Position(r, c));

            if (walkable.Count == 0)
            {
                // Degenerate case: force a path in the middle
                var mid = new Position(height / 2, width / 2);
                grid.SetCell(mid, CellType.Path);
                walkable.Add(mid);
            }

            var startPos = walkable[_rng.Next(walkable.Count)];

            // Step 3: place powerups on random non-start walkable cells
            var candidates = new List<Position>(walkable);
            candidates.Remove(startPos);
            Shuffle(candidates);
            int placed = 0;
            foreach (var pos in candidates)
            {
                if (placed >= powerupCount) break;
                grid.SetCell(pos, CellType.Powerup);
                placed++;
            }

            return (grid, startPos);
        }

        private (Grid, Position) BuildOpenGrid(int width, int height)
        {
            // Fallback: fully open grid, start top-left
            var grid = new Grid(width, height);
            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                    grid.SetCell(new Position(r, c), CellType.Path);
            return (grid, new Position(0, 0));
        }

        /// <summary>
        /// BFS from startPos to verify every walkable cell is reachable.
        /// If any walkable cell is isolated, the level cannot be completed.
        /// </summary>
        private bool IsSolvable(Grid grid, Position startPos)
        {
            int totalWalkable = 0;
            for (int r = 0; r < grid.Height; r++)
                for (int c = 0; c < grid.Width; c++)
                {
                    var t = grid.GetCell(new Position(r, c));
                    if (t == CellType.Path || t == CellType.Powerup)
                        totalWalkable++;
                }

            if (totalWalkable == 0) return false;

            // BFS
            var visited = new bool[grid.Height, grid.Width];
            var queue   = new Queue<Position>();
            queue.Enqueue(startPos);
            visited[startPos.Row, startPos.Col] = true;
            int reachable = 1;

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                foreach (var next in cur.Neighbours())
                {
                    if (!grid.IsInBounds(next)) continue;
                    if (visited[next.Row, next.Col]) continue;
                    var t = grid.GetCell(next);
                    if (t == CellType.Path || t == CellType.Powerup)
                    {
                        visited[next.Row, next.Col] = true;
                        queue.Enqueue(next);
                        reachable++;
                    }
                }
            }

            return reachable == totalWalkable;
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
