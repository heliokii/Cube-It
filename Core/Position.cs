

namespace CubePathGame.Core
{
    /// <summary>
    /// Represents a 2D grid coordinate (row, column).
    /// Structs in C# are copied by value, so passing a Position around
    /// never accidentally mutates the original.
    /// </summary>
    public readonly struct Position
    {
        public int Row { get; }
        public int Col { get; }

        public Position(int row, int col)
        {
            Row = row;
            Col = col;
        }


        public Position Up()    => new Position(Row - 1, Col);
        public Position Down()  => new Position(Row + 1, Col);
        public Position Left()  => new Position(Row, Col - 1);
        public Position Right() => new Position(Row, Col + 1);


        public Position[] Neighbours() => new[]
        {
            Up(), Down(), Left(), Right()
        };


        public override bool Equals(object? obj) =>
            obj is Position p && p.Row == Row && p.Col == Col;

        public override int GetHashCode() => HashCode.Combine(Row, Col);

        public static bool operator ==(Position a, Position b) => a.Equals(b);
        public static bool operator !=(Position a, Position b) => !a.Equals(b);

        public override string ToString() => $"({Row},{Col})";
    }
}
