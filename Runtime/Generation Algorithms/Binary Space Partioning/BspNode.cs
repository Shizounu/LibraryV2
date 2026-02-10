using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shizounu.Library.GenerationAlgorithms.BSP
{
    /// <summary>
    /// Represents a node in a 2D Binary Space Partition tree.
    /// Each node divides space either horizontally or vertically.
    /// </summary>
    public class BspNode
    {
        /// <summary>Gets the axis along which this node divides space (X or Y)</summary>
        public SplitAxis Axis { get; private set; }

        /// <summary>Gets the position along the split axis where the division occurs</summary>
        public float SplitPosition { get; private set; }

        /// <summary>Gets the rectangular bounds of this node's space</summary>
        public Rect Bounds { get; private set; }

        /// <summary>Gets the left/bottom child node (closer to origin)</summary>
        public BspNode LeftChild { get; private set; }

        /// <summary>Gets the right/top child node (further from origin)</summary>
        public BspNode RightChild { get; private set; }

        /// <summary>Gets whether this node is a leaf (has no children)</summary>
        public bool IsLeaf => LeftChild == null && RightChild == null;

        /// <summary>Gets the custom data stored in this node (for leaf nodes)</summary>
        public object UserData { get; set; }

        private BspNode(Rect bounds)
        {
            Bounds = bounds;
        }

        /// <summary>
        /// Creates a leaf node with the given bounds.
        /// </summary>
        public static BspNode CreateLeaf(Rect bounds)
        {
            return new BspNode(bounds);
        }

        /// <summary>
        /// Creates an internal node that splits along the given axis.
        /// </summary>
        public static BspNode CreateInternal(Rect bounds, SplitAxis axis, float splitPosition, 
            BspNode leftChild, BspNode rightChild)
        {
            var node = new BspNode(bounds)
            {
                Axis = axis,
                SplitPosition = splitPosition,
                LeftChild = leftChild,
                RightChild = rightChild
            };
            return node;
        }

        /// <summary>
        /// Finds the leaf node at the given point.
        /// Returns null if the point is outside this node's bounds.
        /// </summary>
        public BspNode FindLeafAt(Vector2 point)
        {
            if (!Bounds.Contains(point))
                return null;

            if (IsLeaf)
                return this;

            if (Axis == SplitAxis.X)
            {
                return point.x <= SplitPosition 
                    ? LeftChild?.FindLeafAt(point) 
                    : RightChild?.FindLeafAt(point);
            }
            else
            {
                return point.y <= SplitPosition 
                    ? LeftChild?.FindLeafAt(point) 
                    : RightChild?.FindLeafAt(point);
            }
        }

        /// <summary>
        /// Gets all leaf nodes in this subtree.
        /// </summary>
        public void GetAllLeaves(List<BspNode> leaves)
        {
            if (IsLeaf)
            {
                leaves.Add(this);
            }
            else
            {
                LeftChild?.GetAllLeaves(leaves);
                RightChild?.GetAllLeaves(leaves);
            }
        }

        /// <summary>
        /// Gets the depth of this node in the tree (leaf nodes have depth 0).
        /// </summary>
        public int GetDepth()
        {
            if (IsLeaf)
                return 0;

            int leftDepth = LeftChild?.GetDepth() ?? 0;
            int rightDepth = RightChild?.GetDepth() ?? 0;
            return 1 + Mathf.Max(leftDepth, rightDepth);
        }

        /// <summary>
        /// Counts the number of leaf nodes in this subtree.
        /// </summary>
        public int CountLeaves()
        {
            if (IsLeaf)
                return 1;

            int count = 0;
            if (LeftChild != null) count += LeftChild.CountLeaves();
            if (RightChild != null) count += RightChild.CountLeaves();
            return count;
        }
    }
}
