using System;
using System.Collections.Generic;
using UnityEngine;
using Shizounu.Library.RandomSystem;
using Shizounu.Library.GenerationAlgorithms.Shared;

namespace Shizounu.Library.GenerationAlgorithms.BSP
{
    /// <summary>
    /// Builds a Binary Space Partition tree for 2D space.
    /// Supports various splitting strategies and constraints.
    /// </summary>
    public class BspBuilder : IRngProvider
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

        private readonly AxisSelection _axisSelection;
        private readonly SplitPositionStrategy _splitPosition;
        private readonly int _minNodeSize;
        private readonly IRngSource _rngSource;

        /// <summary>
        /// Gets the RNG source used by this builder.
        /// </summary>
        public IRngSource RngSource => _rngSource;

        /// <summary>
        /// Creates a new BSP builder with specified configuration.
        /// </summary>
        /// <param name="axisSelection">How to choose the split axis</param>
        /// <param name="splitPosition">Where to place the split</param>
        /// <param name="minNodeSize">Minimum size (width or height) of a leaf node</param>
        /// <param name="seed">Random seed (use -1 for unseeded)</param>
        public BspBuilder(AxisSelection axisSelection = AxisSelection.Alternating,
            SplitPositionStrategy splitPosition = SplitPositionStrategy.Middle,
            int minNodeSize = 1,
            int seed = -1)
        {
            _axisSelection = axisSelection;
            _splitPosition = splitPosition;
            _minNodeSize = Mathf.Max(1, minNodeSize);
            _rngSource = GenerationRng.Create(seed);
        }

        /// <summary>
        /// Creates a new BSP builder with a provided RNG source.
        /// </summary>
        /// <param name="rngSource">RNG source to use</param>
        /// <param name="axisSelection">How to choose the split axis</param>
        /// <param name="splitPosition">Where to place the split</param>
        /// <param name="minNodeSize">Minimum size (width or height) of a leaf node</param>
        public BspBuilder(IRngSource rngSource,
            AxisSelection axisSelection = AxisSelection.Alternating,
            SplitPositionStrategy splitPosition = SplitPositionStrategy.Middle,
            int minNodeSize = 1)
        {
            _rngSource = rngSource ?? throw new ArgumentNullException(nameof(rngSource));
            _axisSelection = axisSelection;
            _splitPosition = splitPosition;
            _minNodeSize = Mathf.Max(1, minNodeSize);
        }

        /// <summary>
        /// Generates a complete BSP tree for the given rectangular area.
        /// </summary>
        public BspNode Generate(Rect bounds)
        {
            return GenerateRecursive(bounds, 0);
        }

        private BspNode GenerateRecursive(Rect bounds, int depth)
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
            BspNode leftChild = GenerateRecursive(leftBounds, depth + 1);
            BspNode rightChild = GenerateRecursive(rightBounds, depth + 1);

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
            float randomValue = _rngSource.NextFloat();
            
            if (axis == SplitAxis.X)
            {
                return BspUtilities.CalculateSplitPosition(bounds.xMin, bounds.width, _splitPosition, randomValue);
            }
            else
            {
                return BspUtilities.CalculateSplitPosition(bounds.yMin, bounds.height, _splitPosition, randomValue);
            }
        }

        /// <summary>
        /// Generates a BSP tree with a specific target number of leaf nodes.
        /// Continues splitting until reaching approximately the target count.
        /// </summary>
        public BspNode GenerateToTargetLeaves(Rect bounds, int targetLeafCount)
        {
            if (targetLeafCount < 1)
                throw new ArgumentException("Target leaf count must be at least 1", nameof(targetLeafCount));

            var root = Generate(bounds);

            // If we already have enough leaves, return as is
            if (root.CountLeaves() >= targetLeafCount)
                return root;

            // Otherwise, subdivide further by lowering min node size
            for (int minSize = _minNodeSize - 1; minSize > 0; minSize--)
            {
                var builder = new BspBuilder(_rngSource.Clone() as IRngSource, _axisSelection, _splitPosition, minSize);
                root = builder.Generate(bounds);
                if (root.CountLeaves() >= targetLeafCount)
                    return root;
            }

            return root;
        }
    }
}
