using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shizounu.Library.GenerationAlgorithms.BSP
{
    /// <summary>
    /// Builds a Binary Space Partition tree for 2D space.
    /// Supports various splitting strategies and constraints.
    /// </summary>
    public class BspBuilder
    {
        /// <summary>Defines how the builder selects the split axis</summary>
        public enum AxisSelection
        {
            /// <summary>Alternates between X and Y axes level by level</summary>
            Alternating,

            /// <summary>Always splits along the longest dimension</summary>
            LongestSide,

            /// <summary>Always splits along the X axis</summary>
            AlwaysX,

            /// <summary>Always splits along the Y axis</summary>
            AlwaysY
        }

        /// <summary>Defines where along the chosen axis to perform the split</summary>
        public enum SplitPosition
        {
            /// <summary>Split at the middle position</summary>
            Middle,

            /// <summary>Split at a random position</summary>
            Random,

            /// <summary>Split at a position that creates a golden ratio division</summary>
            GoldenRatio
        }

        private readonly AxisSelection _axisSelection;
        private readonly SplitPosition _splitPosition;
        private readonly int _minNodeSize;
        private readonly System.Random _random;

        /// <summary>
        /// Creates a new BSP builder with specified configuration.
        /// </summary>
        /// <param name="axisSelection">How to choose the split axis</param>
        /// <param name="splitPosition">Where to place the split</param>
        /// <param name="minNodeSize">Minimum size (width or height) of a leaf node</param>
        /// <param name="seed">Random seed (use -1 for unseeded)</param>
        public BspBuilder(AxisSelection axisSelection = AxisSelection.Alternating,
            SplitPosition splitPosition = SplitPosition.Middle,
            int minNodeSize = 1,
            int seed = -1)
        {
            _axisSelection = axisSelection;
            _splitPosition = splitPosition;
            _minNodeSize = Mathf.Max(1, minNodeSize);
            _random = seed < 0 ? new System.Random() : new System.Random(seed);
        }

        /// <summary>
        /// Builds a complete BSP tree for the given rectangular area.
        /// </summary>
        public BspNode Build(Rect bounds)
        {
            return BuildRecursive(bounds, 0);
        }

        private BspNode BuildRecursive(Rect bounds, int depth)
        {
            // Determine split axis
            SplitAxis axis = SelectAxis(bounds, depth);

            // Check if we can split further
            float dimension = axis == SplitAxis.X ? bounds.width : bounds.height;
            if (dimension / 2f < _minNodeSize)
            {
                return BspNode.CreateLeaf(bounds);
            }

            // Calculate split position
            float split = CalculateSplitPosition(bounds, axis);

            // Create child bounds
            Rect leftBounds, rightBounds;
            if (axis == SplitAxis.X)
            {
                float leftWidth = split - bounds.xMin;
                leftBounds = new Rect(bounds.xMin, bounds.yMin, leftWidth, bounds.height);
                rightBounds = new Rect(split, bounds.yMin, bounds.width - leftWidth, bounds.height);
            }
            else
            {
                float bottomHeight = split - bounds.yMin;
                leftBounds = new Rect(bounds.xMin, bounds.yMin, bounds.width, bottomHeight);
                rightBounds = new Rect(bounds.xMin, split, bounds.width, bounds.height - bottomHeight);
            }

            // Recursively build children
            BspNode leftChild = BuildRecursive(leftBounds, depth + 1);
            BspNode rightChild = BuildRecursive(rightBounds, depth + 1);

            return BspNode.CreateInternal(bounds, axis, split, leftChild, rightChild);
        }

        private SplitAxis SelectAxis(Rect bounds, int depth)
        {
            return _axisSelection switch
            {
                AxisSelection.Alternating => (depth % 2 == 0) ? SplitAxis.X : SplitAxis.Y,
                AxisSelection.LongestSide => bounds.width >= bounds.height ? SplitAxis.X : SplitAxis.Y,
                AxisSelection.AlwaysX => SplitAxis.X,
                AxisSelection.AlwaysY => SplitAxis.Y,
                _ => SplitAxis.X
            };
        }

        private float CalculateSplitPosition(Rect bounds, SplitAxis axis)
        {
            if (axis == SplitAxis.X)
            {
                return _splitPosition switch
                {
                    SplitPosition.Middle => bounds.xMin + bounds.width / 2f,
                    SplitPosition.Random => bounds.xMin + (float)_random.NextDouble() * bounds.width,
                    SplitPosition.GoldenRatio => bounds.xMin + bounds.width / 2.618f,
                    _ => bounds.xMin + bounds.width / 2f
                };
            }
            else
            {
                return _splitPosition switch
                {
                    SplitPosition.Middle => bounds.yMin + bounds.height / 2f,
                    SplitPosition.Random => bounds.yMin + (float)_random.NextDouble() * bounds.height,
                    SplitPosition.GoldenRatio => bounds.yMin + bounds.height / 2.618f,
                    _ => bounds.yMin + bounds.height / 2f
                };
            }
        }

        /// <summary>
        /// Builds a BSP tree with a specific target number of leaf nodes.
        /// Continues splitting until reaching approximately the target count.
        /// </summary>
        public BspNode BuildToTargetLeaves(Rect bounds, int targetLeafCount)
        {
            if (targetLeafCount < 1)
                throw new ArgumentException("Target leaf count must be at least 1", nameof(targetLeafCount));

            var root = Build(bounds);

            // If we already have enough leaves, return as is
            if (root.CountLeaves() >= targetLeafCount)
                return root;

            // Otherwise, subdivide further by lowering min node size
            for (int minSize = _minNodeSize - 1; minSize > 0; minSize--)
            {
                var builder = new BspBuilder(_axisSelection, _splitPosition, minSize, _random.Next());
                root = builder.Build(bounds);
                if (root.CountLeaves() >= targetLeafCount)
                    return root;
            }

            return root;
        }
    }
}
