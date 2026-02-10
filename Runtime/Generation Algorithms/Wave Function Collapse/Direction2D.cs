using System;

namespace Shizounu.Library.GenerationAlgorithms.WaveFunctionCollapse
{
    public enum Direction2D
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
    }

    public static class Direction2DUtility
    {
        public static readonly Direction2D[] All =
        {
            Direction2D.Up,
            Direction2D.Right,
            Direction2D.Down,
            Direction2D.Left
        };

        public static Direction2D Opposite(Direction2D direction)
        {
            switch (direction)
            {
                case Direction2D.Up:
                    return Direction2D.Down;
                case Direction2D.Right:
                    return Direction2D.Left;
                case Direction2D.Down:
                    return Direction2D.Up;
                case Direction2D.Left:
                    return Direction2D.Right;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public static (int dx, int dy) Offset(Direction2D direction)
        {
            switch (direction)
            {
                case Direction2D.Up:
                    return (0, 1);
                case Direction2D.Right:
                    return (1, 0);
                case Direction2D.Down:
                    return (0, -1);
                case Direction2D.Left:
                    return (-1, 0);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}
