using System;
using System.Collections.Generic;
using UnityEngine;
using Shizounu.Library.RandomSystem;
using Shizounu.Library.GenerationAlgorithms.Shared;

namespace Shizounu.Library.GenerationAlgorithms.BSP
{
    /// <summary>
    /// Builds a Binary Space Partition tree for 3D space.
    /// Supports various splitting strategies and constraints.
    /// </summary>
    public class BspBuilder3D : IRngProvider
    {
        /// <summary>Defines how the builder selects the split axis</summary>
        public enum AxisSelection
        {
            /// <summary>Cycles through X, Y, Z axes level by level</summary>
            Cycling,

            /// <summary>Always splits along the longest dimension</summary>
            LongestSide,

            /// <summary>Always splits along the X axis</summary>
            AlwaysX,

            /// <summary>Always splits along the Y axis</summary>
            AlwaysY,

            /// <summary>Always splits along the Z axis</summary>
            AlwaysZ
        }

        private readonly AxisSelection _axisSelection;
        private readonly SplitPositionStrategy _splitPosition;
        private readonly float _minNodeSize;
        private readonly IRngSource _rngSource;

        /// <summary>
        /// Gets the RNG source used by this builder.
        /// </summary>
        public IRngSource RngSource => _rngSource;

        /// <summary>
        /// Creates a new 3D BSP builder with specified configuration.
        /// </summary>
        /// <param name="axisSelection">How to choose the split axis</param>
        /// <param name="splitPosition">Where to place the split</param>
        /// <param name="minNodeSize">Minimum size of a leaf node (on any axis)</param>
        /// <param name="seed">Random seed (use -1 for unseeded)</param>
        public BspBuilder3D(AxisSelection axisSelection = AxisSelection.Cycling,
            SplitPositionStrategy splitPosition = SplitPositionStrategy.Middle,
            float minNodeSize = 1f,
            int seed = -1)
        {
            _axisSelection = axisSelection;
            _splitPosition = splitPosition;
            _minNodeSize = Mathf.Max(0.1f, minNodeSize);
            _rngSource = GenerationRng.Create(seed);
        }

        /// <summary>
        /// Creates a new 3D BSP builder with a provided RNG source.
        /// </summary>
        /// <param name="rngSource">RNG source to use</param>
        /// <param name="axisSelection">How to choose the split axis</param>
        /// <param name="splitPosition">Where to place the split</param>
        /// <param name="minNodeSize">Minimum size of a leaf node (on any axis)</param>
        public BspBuilder3D(IRngSource rngSource,
            AxisSelection axisSelection = AxisSelection.Cycling,
            SplitPositionStrategy splitPosition = SplitPositionStrategy.Middle,
            float minNodeSize = 1f)
        {
            _rngSource = rngSource ?? throw new ArgumentNullException(nameof(rngSource));
            _axisSelection = axisSelection;
            _splitPosition = splitPosition;
            _minNodeSize = Mathf.Max(0.1f, minNodeSize);
        }

        /// <summary>
        /// Generates a complete BSP tree for the given 3D volume.
        /// </summary>
        public BspNode3D Generate(Bounds bounds)
        {
            return GenerateRecursive(bounds, 0);
        }

        private BspNode3D GenerateRecursive(Bounds bounds, int depth)
        {
            // Determine split axis
            SplitAxis axis = SelectAxis(bounds, depth);

            // Get the size along the split axis
            Vector3 size = bounds.size;
            float dimension = axis switch
            {
                SplitAxis.X => size.x,
                SplitAxis.Y => size.y,
                SplitAxis.Z => size.z,
                _ => size.x
            };

            // Check if we can split further
            if (dimension / 2f < _minNodeSize)
            {
                return BspNode3D.CreateLeaf(bounds);
            }

            // Calculate split position
            float split = CalculateSplitPosition(bounds, axis);

            // Create child bounds
            Vector3 center = bounds.center;
            Vector3 childSize = size;

            BspNode3D leftChild, rightChild;

            if (axis == SplitAxis.X)
            {
                childSize.x /= 2f;
                var leftBounds = new Bounds(center - Vector3.right * childSize.x / 2f, childSize);
                var rightBounds = new Bounds(center + Vector3.right * childSize.x / 2f, childSize);
                leftChild = GenerateRecursive(leftBounds, depth + 1);
                rightChild = GenerateRecursive(rightBounds, depth + 1);
            }
            else if (axis == SplitAxis.Y)
            {
                childSize.y /= 2f;
                var leftBounds = new Bounds(center - Vector3.up * childSize.y / 2f, childSize);
                var rightBounds = new Bounds(center + Vector3.up * childSize.y / 2f, childSize);
                leftChild = GenerateRecursive(leftBounds, depth + 1);
                rightChild = GenerateRecursive(rightBounds, depth + 1);
            }
            else // Z
            {
                childSize.z /= 2f;
                var leftBounds = new Bounds(center - Vector3.forward * childSize.z / 2f, childSize);
                var rightBounds = new Bounds(center + Vector3.forward * childSize.z / 2f, childSize);
                leftChild = GenerateRecursive(leftBounds, depth + 1);
                rightChild = GenerateRecursive(rightBounds, depth + 1);
            }

            return BspNode3D.CreateInternal(bounds, axis, split, leftChild, rightChild);
        }

        private SplitAxis SelectAxis(Bounds bounds, int depth)
        {
            return _axisSelection switch
            {
                AxisSelection.Cycling => (SplitAxis)(depth % 3),
                AxisSelection.LongestSide => GetLongestAxis(bounds),
                AxisSelection.AlwaysX => SplitAxis.X,
                AxisSelection.AlwaysY => SplitAxis.Y,
                AxisSelection.AlwaysZ => SplitAxis.Z,
                _ => SplitAxis.X
            };
        }

        private SplitAxis GetLongestAxis(Bounds bounds)
        {
            Vector3 size = bounds.size;
            if (size.x >= size.y && size.x >= size.z)
                return SplitAxis.X;
            if (size.y >= size.z)
                return SplitAxis.Y;
            return SplitAxis.Z;
        }

        private float CalculateSplitPosition(Bounds bounds, SplitAxis axis)
        {
            Vector3 min = bounds.min;
            Vector3 size = bounds.size;
            float randomValue = _rngSource.NextFloat();

            if (axis == SplitAxis.X)
            {
                return BspUtilities.CalculateSplitPosition(min.x, size.x, _splitPosition, randomValue);
            }
            else if (axis == SplitAxis.Y)
            {
                return BspUtilities.CalculateSplitPosition(min.y, size.y, _splitPosition, randomValue);
            }
            else // Z
            {
                return BspUtilities.CalculateSplitPosition(min.z, size.z, _splitPosition, randomValue);
            }
        }

        /// <summary>
        /// Generates a BSP tree with a specific target number of leaf nodes.
        /// Continues splitting until reaching approximately the target count.
        /// </summary>
        public BspNode3D GenerateToTargetLeaves(Bounds bounds, int targetLeafCount)
        {
            if (targetLeafCount < 1)
                throw new ArgumentException("Target leaf count must be at least 1", nameof(targetLeafCount));

            var root = Generate(bounds);

            // If we already have enough leaves, return as is
            if (root.CountLeaves() >= targetLeafCount)
                return root;

            // Otherwise, subdivide further by lowering min node size
            for (float minSize = _minNodeSize * 0.5f; minSize > 0.01f; minSize *= 0.5f)
            {
                var builder = new BspBuilder3D(_rngSource.Clone() as IRngSource, _axisSelection, _splitPosition, minSize);
                root = builder.Generate(bounds);
                if (root.CountLeaves() >= targetLeafCount)
                    return root;
            }

            return root;
        }
    }
}
