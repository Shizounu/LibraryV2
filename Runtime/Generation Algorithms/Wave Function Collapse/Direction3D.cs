using System;

namespace Shizounu.Library.GenerationAlgorithms.WaveFunctionCollapse
{
    public enum Direction3D
    {
        Up = 0,
        Down = 1,
        Forward = 2,
        Backward = 3,
        Right = 4,
        Left = 5
    }

    public static class Direction3DUtility
    {
        public static readonly Direction3D[] All =
        {
            Direction3D.Up,
            Direction3D.Down,
            Direction3D.Forward,
            Direction3D.Backward,
            Direction3D.Right,
            Direction3D.Left
        };

        public static Direction3D Opposite(Direction3D direction)
        {
            switch (direction)
            {
                case Direction3D.Up:
                    return Direction3D.Down;
                case Direction3D.Down:
                    return Direction3D.Up;
                case Direction3D.Forward:
                    return Direction3D.Backward;
                case Direction3D.Backward:
                    return Direction3D.Forward;
                case Direction3D.Right:
                    return Direction3D.Left;
                case Direction3D.Left:
                    return Direction3D.Right;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public static (int dx, int dy, int dz) Offset(Direction3D direction)
        {
            switch (direction)
            {
                case Direction3D.Up:
                    return (0, 0, 1);
                case Direction3D.Down:
                    return (0, 0, -1);
                case Direction3D.Forward:
                    return (0, 1, 0);
                case Direction3D.Backward:
                    return (0, -1, 0);
                case Direction3D.Right:
                    return (1, 0, 0);
                case Direction3D.Left:
                    return (-1, 0, 0);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}
