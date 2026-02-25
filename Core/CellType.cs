

namespace CubePathGame.Core
{
    /// <summary>
    /// Represents what kind of tile occupies a position in the grid.
    /// </summary>
    public enum CellType
    {
        /// <summary>Open floor the player can walk on. Shown as an unvisited tile.</summary>
        Path,

        /// <summary>Solid wall the player cannot enter. Drawn as a raised block.</summary>
        Wall,

        /// <summary>A Path the player has already stepped on. Shown with a trail marker.</summary>
        Visited,

        /// <summary>Collectible bonus item worth extra points. Shown as a glowing gem.</summary>
        Powerup
    }
}
