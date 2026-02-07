using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shizounu.Library.GameAI.GOAP
{
    /// <summary>
    /// GOAP Agent that manages goals, actions, and planning.
    /// Attach this to a GameObject to enable GOAP AI behavior.
    /// </summary>
    public class GoapAgent : MonoBehaviour
    {
        [SerializeField] private bool _debugMode = false;
        [SerializeField] private float _replanInterval = 1f;

        private List<IGoapAction> _availableActions;
        private List<GoapGoal> _goals;
        private GoapPlanner _planner;
        private WorldState _worldState;

        private GoapGoal _currentGoal;
        private List<IGoapAction> _currentPlan;
        private int _currentActionIndex;
        private IGoapAction _currentAction;
        private float _lastReplanTime;

        public GoapGoal CurrentGoal => _currentGoal;
        public IGoapAction CurrentAction => _currentAction;
        public WorldState WorldState => _worldState;
        public bool HasPlan => _currentPlan != null && _currentPlan.Count > 0;
        public bool IsExecutingAction => _currentAction != null;

        /// <summary>
        /// Event triggered when a new plan is created.
        /// </summary>
        public event Action<List<IGoapAction>> OnPlanCreated;

        /// <summary>
        /// Event triggered when a plan fails.
        /// </summary>
        public event Action OnPlanFailed;

        /// <summary>
        /// Event triggered when an action starts executing.
        /// </summary>
        public event Action<IGoapAction> OnActionStarted;

        /// <summary>
        /// Event triggered when an action completes.
        /// </summary>
        public event Action<IGoapAction> OnActionCompleted;

        private void Awake()
        {
            _availableActions = new List<IGoapAction>();
            _goals = new List<GoapGoal>();
            _planner = new GoapPlanner();
            
            if (_worldState == null)
                _worldState = new WorldState();
        }

        /// <summary>
        /// Initializes the agent with an existing blackboard.
        /// Useful for integrating GOAP with other AI systems that share state.
        /// Call this before the agent starts executing.
        /// </summary>
        public void InitializeWithBlackboard(Blackboard blackboard)
        {
            if (blackboard == null)
                throw new ArgumentNullException(nameof(blackboard));
            
            _worldState = new WorldState(blackboard);
        }

        private void Update()
        {
            // Periodic replanning
            if (Time.time - _lastReplanTime > _replanInterval)
            {
                _lastReplanTime = Time.time;
                
                // Check if current plan is still valid
                if (_currentGoal != null && !_currentGoal.IsSatisfied(_worldState))
                {
                    if (!HasPlan || !IsPlanStillValid())
                    {
                        CreatePlan();
                    }
                }
                else if (_currentGoal == null || _currentGoal.IsSatisfied(_worldState))
                {
                    // Current goal is satisfied or no goal, select new goal
                    SelectGoal();
                    if (_currentGoal != null)
                    {
                        CreatePlan();
                    }
                }
            }

            // Execute current action
            if (_currentAction != null)
            {
                bool actionComplete = _currentAction.Execute(gameObject);
                
                if (actionComplete)
                {
                    _currentAction.OnExit(gameObject);
                    
                    if (_debugMode)
                        Debug.Log($"[GOAP] Completed action: {_currentAction.Name}");
                    
                    OnActionCompleted?.Invoke(_currentAction);
                    
                    // Apply action effects to world state
                    _worldState.ApplyEffects(_currentAction.Effects);
                    
                    _currentAction = null;
                    _currentActionIndex++;

                    // Move to next action or complete plan
                    if (_currentActionIndex < _currentPlan.Count)
                    {
                        StartNextAction();
                    }
                    else
                    {
                        // Plan completed
                        _currentPlan = null;
                        _currentActionIndex = 0;
                    }
                }
            }
            else if (HasPlan && _currentActionIndex < _currentPlan.Count)
            {
                StartNextAction();
            }
        }

        /// <summary>
        /// Adds an action to the agent's available actions.
        /// </summary>
        public void AddAction(IGoapAction action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            
            _availableActions.Add(action);
        }

        /// <summary>
        /// Removes an action from the agent's available actions.
        /// </summary>
        public void RemoveAction(IGoapAction action)
        {
            _availableActions.Remove(action);
        }

        /// <summary>
        /// Adds a goal to the agent.
        /// </summary>
        public void AddGoal(GoapGoal goal)
        {
            if (goal == null)
                throw new ArgumentNullException(nameof(goal));
            
            _goals.Add(goal);
        }

        /// <summary>
        /// Removes a goal from the agent.
        /// </summary>
        public void RemoveGoal(GoapGoal goal)
        {
            _goals.Remove(goal);
        }

        /// <summary>
        /// Updates the world state with a new value.
        /// </summary>
        public void SetWorldState<T>(string key, T value)
        {
            _worldState.SetValue(key, value);
        }

        /// <summary>
        /// Gets a value from the world state.
        /// </summary>
        public T GetWorldState<T>(string key)
        {
            return _worldState.GetValue<T>(key);
        }

        /// <summary>
        /// Forces the agent to create a new plan immediately.
        /// </summary>
        public void ForceReplan()
        {
            SelectGoal();
            CreatePlan();
        }

        /// <summary>
        /// Stops the current plan and clears the current action.
        /// </summary>
        public void StopPlan()
        {
            if (_currentAction != null)
            {
                _currentAction.OnExit(gameObject);
                _currentAction = null;
            }
            
            _currentPlan = null;
            _currentActionIndex = 0;
            _currentGoal = null;
        }

        private void SelectGoal()
        {
            GoapGoal bestGoal = null;
            float highestPriority = float.MinValue;

            foreach (var goal in _goals)
            {
                if (!goal.IsRelevant(gameObject))
                    continue;

                if (goal.IsSatisfied(_worldState))
                    continue;

                if (goal.Priority > highestPriority)
                {
                    highestPriority = goal.Priority;
                    bestGoal = goal;
                }
            }

            _currentGoal = bestGoal;

            if (_debugMode && _currentGoal != null)
                Debug.Log($"[GOAP] Selected goal: {_currentGoal.Name}");
        }

        private void CreatePlan()
        {
            if (_currentGoal == null)
                return;

            // Stop current action if any
            if (_currentAction != null)
            {
                _currentAction.OnExit(gameObject);
                _currentAction = null;
            }

            // Create a new plan
            var plan = _planner.Plan(gameObject, _worldState, _currentGoal, _availableActions);

            if (plan != null && plan.Count > 0)
            {
                _currentPlan = plan;
                _currentActionIndex = 0;

                if (_debugMode)
                {
                    Debug.Log($"[GOAP] Created plan for goal '{_currentGoal.Name}': " +
                             string.Join(" -> ", plan.Select(a => a.Name)));
                }

                OnPlanCreated?.Invoke(plan);
            }
            else
            {
                _currentPlan = null;
                
                if (_debugMode)
                    Debug.LogWarning($"[GOAP] Failed to create plan for goal: {_currentGoal.Name}");
                
                OnPlanFailed?.Invoke();
            }
        }

        private void StartNextAction()
        {
            if (_currentPlan == null || _currentActionIndex >= _currentPlan.Count)
                return;

            _currentAction = _currentPlan[_currentActionIndex];
            _currentAction.OnEnter(gameObject);

            if (_debugMode)
                Debug.Log($"[GOAP] Starting action: {_currentAction.Name}");

            OnActionStarted?.Invoke(_currentAction);
        }

        private bool IsPlanStillValid()
        {
            if (_currentPlan == null || _currentPlan.Count == 0)
                return false;

            // Simulate the plan from current state
            var simulatedState = _worldState.Clone();

            for (int i = _currentActionIndex; i < _currentPlan.Count; i++)
            {
                var action = _currentPlan[i];

                // Check if action is still available
                if (!action.IsAvailable(gameObject))
                    return false;

                // Check if preconditions are still met
                if (!simulatedState.MeetsConditions(action.Preconditions))
                    return false;

                // Apply effects
                simulatedState.ApplyEffects(action.Effects);
            }

            // Check if goal would still be satisfied
            return _currentGoal.IsSatisfied(simulatedState);
        }

        private void OnDrawGizmos()
        {
            if (!_debugMode || !Application.isPlaying)
                return;

            // Display current goal and action above the agent
            if (_currentGoal != null || _currentAction != null)
            {
                var position = transform.position + Vector3.up * 2;
                var text = "";
                
                if (_currentGoal != null)
                    text += $"Goal: {_currentGoal.Name}\n";
                
                if (_currentAction != null)
                    text += $"Action: {_currentAction.Name}";

                #if UNITY_EDITOR
                UnityEditor.Handles.Label(position, text);
                #endif
            }
        }
    }
}
