using UnityEngine;

namespace Shizounu.Library.GameAI.GOAP
{
    /// <summary>
    /// Interface for GOAP actions.
    /// Actions have preconditions that must be met before they can execute,
    /// and effects that modify the world state when complete.
    /// </summary>
    public interface IGoapAction
    {
        /// <summary>
        /// The name of this action.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The cost of executing this action. Lower costs are preferred by the planner.
        /// </summary>
        float Cost { get; }

        /// <summary>
        /// The preconditions that must be met for this action to be available.
        /// </summary>
        WorldState Preconditions { get; }

        /// <summary>
        /// The effects this action has on the world state when executed.
        /// </summary>
        WorldState Effects { get; }

        /// <summary>
        /// Checks if this action is currently available to be executed.
        /// This is checked in addition to preconditions during planning.
        /// </summary>
        bool IsAvailable(GameObject agent);

        /// <summary>
        /// Called when the action is selected as part of a plan.
        /// </summary>
        void OnEnter(GameObject agent);

        /// <summary>
        /// Called every frame while the action is executing.
        /// Returns true when the action is complete.
        /// </summary>
        bool Execute(GameObject agent);

        /// <summary>
        /// Called when the action completes or is interrupted.
        /// </summary>
        void OnExit(GameObject agent);

        /// <summary>
        /// Creates a copy of this action.
        /// </summary>
        IGoapAction Clone();
    }
}
