using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shizounu.Library.GameAI.GOAP
{
    /// <summary>
    /// Plans a sequence of actions to achieve a goal using A* pathfinding.
    /// </summary>
    public class GoapPlanner
    {
        private class PlanNode
        {
            public WorldState State;
            public GoapAction Action;
            public PlanNode Parent;
            public float GCost; // Cost from start
            public float HCost; // Heuristic cost to goal
            public float FCost => GCost + HCost;

            public PlanNode(WorldState state, GoapAction action, PlanNode parent, float gCost, float hCost)
            {
                State = state;
                Action = action;
                Parent = parent;
                GCost = gCost;
                HCost = hCost;
            }
        }

        /// <summary>
        /// Plans a sequence of actions to achieve the goal from the current state.
        /// Returns null if no plan can be found.
        /// </summary>
        public List<GoapAction> Plan(
            GameObject agent,
            WorldState currentState,
            GoapGoal goal,
            List<GoapAction> availableActions,
            int maxIterations = 1000)
        {
            if (currentState == null || goal == null || availableActions == null)
                return null;

            // Check if goal is already satisfied
            if (goal.IsSatisfied(currentState))
                return new List<GoapAction>();

            var openSet = new List<PlanNode>();
            var closedSet = new HashSet<WorldState>();

            // Start node
            var startNode = new PlanNode(
                currentState.Clone(),
                null,
                null,
                0f,
                CalculateHeuristic(currentState, goal.DesiredState)
            );

            openSet.Add(startNode);

            int iterations = 0;
            while (openSet.Count > 0 && iterations < maxIterations)
            {
                iterations++;

                // Get node with lowest F cost
                var currentNode = GetLowestFCostNode(openSet);
                openSet.Remove(currentNode);

                // Check if we've reached the goal
                if (goal.IsSatisfied(currentNode.State))
                {
                    return ReconstructPlan(currentNode);
                }

                closedSet.Add(currentNode.State);

                // Explore neighbors (available actions)
                foreach (var action in availableActions)
                {
                    // Check if action is available
                    if (!action.IsAvailable(agent))
                        continue;

                    // Check if preconditions are met
                    if (!currentNode.State.MeetsConditions(action.Preconditions))
                        continue;

                    // Apply action effects to create new state
                    var newState = currentNode.State.Clone();
                    newState.ApplyEffects(action.Effects);

                    // Skip if we've already evaluated this state
                    if (IsStateInSet(closedSet, newState))
                        continue;

                    float newGCost = currentNode.GCost + action.Cost;
                    float hCost = CalculateHeuristic(newState, goal.DesiredState);

                    // Check if this state is already in the open set
                    var existingNode = FindNodeWithState(openSet, newState);
                    if (existingNode != null)
                    {
                        // Update if we found a better path
                        if (newGCost < existingNode.GCost)
                        {
                            existingNode.GCost = newGCost;
                            existingNode.Parent = currentNode;
                            existingNode.Action = action;
                        }
                    }
                    else
                    {
                        // Add new node to open set
                        var newNode = new PlanNode(newState, action, currentNode, newGCost, hCost);
                        openSet.Add(newNode);
                    }
                }
            }

            // No plan found
            return null;
        }

        private PlanNode GetLowestFCostNode(List<PlanNode> nodes)
        {
            PlanNode lowest = nodes[0];
            for (int i = 1; i < nodes.Count; i++)
            {
                if (nodes[i].FCost < lowest.FCost)
                    lowest = nodes[i];
            }
            return lowest;
        }

        private float CalculateHeuristic(WorldState current, WorldState goal)
        {
            // Simple heuristic: count how many goal conditions are not met
            float unmetConditions = 0;
            foreach (var key in goal.GetAllKeys())
            {
                if (!current.HasKey(key))
                {
                    unmetConditions++;
                }
                else
                {
                    var currentValue = current.GetValue<object>(key);
                    var goalValue = goal.GetValue<object>(key);
                    if (!currentValue.Equals(goalValue))
                        unmetConditions++;
                }
            }
            return unmetConditions;
        }

        private bool IsStateInSet(HashSet<WorldState> set, WorldState state)
        {
            // Since WorldState doesn't implement proper equality, we need to check manually
            foreach (var s in set)
            {
                if (StatesAreEqual(s, state))
                    return true;
            }
            return false;
        }

        private PlanNode FindNodeWithState(List<PlanNode> nodes, WorldState state)
        {
            foreach (var node in nodes)
            {
                if (StatesAreEqual(node.State, state))
                    return node;
            }
            return null;
        }

        private bool StatesAreEqual(WorldState a, WorldState b)
        {
            var aKeys = a.GetAllKeys().ToList();
            var bKeys = b.GetAllKeys().ToList();

            if (aKeys.Count != bKeys.Count)
                return false;

            foreach (var key in aKeys)
            {
                if (!b.HasKey(key))
                    return false;

                var aValue = a.GetValue<object>(key);
                var bValue = b.GetValue<object>(key);

                if (!aValue.Equals(bValue))
                    return false;
            }

            return true;
        }

        private List<GoapAction> ReconstructPlan(PlanNode goalNode)
        {
            var plan = new List<GoapAction>();
            var currentNode = goalNode;

            while (currentNode.Parent != null)
            {
                plan.Add(currentNode.Action);
                currentNode = currentNode.Parent;
            }

            plan.Reverse();
            return plan;
        }
    }
}
