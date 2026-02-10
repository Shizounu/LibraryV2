using System.Collections.Generic;
using UnityEngine;

namespace Shizounu.Library.GenerationAlgorithms.BSP
{
    /// <summary>
    /// Represents a node in a 3D Binary Space Partition tree.
    /// Each node divides 3D space along one of the three axes.
    /// </summary>
    public class BspNode3D
    {
        /// <summary>Gets the axis along which this node divides space (X, Y, or Z)</summary>
        public SplitAxis Axis { get; private set; }

        /// <summary>Gets the position along the split axis where the division occurs</summary>
        public float SplitPosition { get; private set; }

        /// <summary>Gets the bounding box of this node's space</summary>
        public Bounds Bounds { get; private set; }

        /// <summary>Gets the left/near child node (closer to origin)</summary>
        public BspNode3D LeftChild { get; private set; }

        /// <summary>Gets the right/far child node (further from origin)</summary>
        public BspNode3D RightChild { get; private set; }

        /// <summary>Gets whether this node is a leaf (has no children)</summary>
        public bool IsLeaf => LeftChild == null && RightChild == null;

        /// <summary>Gets the custom data stored in this node (for leaf nodes)</summary>
        public object UserData { get; set; }

        private BspNode3D(Bounds bounds)
        {
            Bounds = bounds;
        }

        /// <summary>
        /// Creates a leaf node with the given bounds.
        /// </summary>
        public static BspNode3D CreateLeaf(Bounds bounds)
        {
            return new BspNode3D(bounds);
        }

        /// <summary>
        /// Creates an internal node that splits along the given axis.
        /// </summary>
        public static BspNode3D CreateInternal(Bounds bounds, SplitAxis axis, float splitPosition,
            BspNode3D leftChild, BspNode3D rightChild)
        {
            var node = new BspNode3D(bounds)
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
        public BspNode3D FindLeafAt(Vector3 point)
        {
            if (!Bounds.Contains(point))
                return null;

            if (IsLeaf)
                return this;

            float c = Axis switch
            {
                SplitAxis.X => point.x,
                SplitAxis.Y => point.y,
                SplitAxis.Z => point.z,
                _ => point.x
            };

            return c <= SplitPosition 
                ? LeftChild?.FindLeafAt(point) 
                : RightChild?.FindLeafAt(point);
        }

        /// <summary>
        /// Gets all leaf nodes in this subtree.
        /// </summary>
        public void GetAllLeaves(List<BspNode3D> leaves)
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
