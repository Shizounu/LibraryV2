using UnityEngine;

namespace Shizounu.Library.GameAI.GOAP.Examples
{
    /// <summary>
    /// Complete example demonstrating how to set up and use the GOAP system.
    /// This example creates a simple AI that gathers resources and crafts items.
    /// </summary>
    public class GoapUsageExample : MonoBehaviour
    {
        [SerializeField] private Vector3 _woodLocation = new Vector3(10, 0, 0);
        [SerializeField] private Vector3 _stoneLocation = new Vector3(-10, 0, 0);
        [SerializeField] private Vector3 _craftingLocation = new Vector3(0, 0, 10);

        private GoapAgent _agent;

        private void Start()
        {
            // Get or add the GOAP agent component
            _agent = GetComponent<GoapAgent>();
            if (_agent == null)
            {
                _agent = gameObject.AddComponent<GoapAgent>();
            }

            // Initialize world state
            InitializeWorldState();

            // Set up goals
            SetupGoals();

            // Set up actions
            SetupActions();

            // Subscribe to events
            _agent.OnPlanCreated += OnPlanCreated;
            _agent.OnPlanFailed += OnPlanFailed;
            _agent.OnActionStarted += OnActionStarted;
            _agent.OnActionCompleted += OnActionCompleted;

            Debug.Log("GOAP Example initialized!");
        }

        private void InitializeWorldState()
        {
            // Set initial world state
            _agent.SetWorldState("hasWood", false);
            _agent.SetWorldState("hasStone", false);
            _agent.SetWorldState("hasTool", false);
            _agent.SetWorldState("atWoodLocation", false);
            _agent.SetWorldState("atStoneLocation", false);
            _agent.SetWorldState("atCraftingLocation", false);
        }

        private void SetupGoals()
        {
            // Goal 1: Have a tool (highest priority)
            var toolGoal = new GoapGoal("HaveTool", priority: 10f)
                .WithCondition("hasTool", true);
            _agent.AddGoal(toolGoal);

            // Goal 2: Have wood (lower priority)
            var woodGoal = new GoapGoal("HaveWood", priority: 5f)
                .WithCondition("hasWood", true);
            _agent.AddGoal(woodGoal);

            // Goal 3: Have stone (lower priority)
            var stoneGoal = new GoapGoal("HaveStone", priority: 5f)
                .WithCondition("hasStone", true);
            _agent.AddGoal(stoneGoal);
        }

        private void SetupActions()
        {
            // Action: Move to wood location
            var moveToWood = new MoveToLocationAction("Wood", _woodLocation, "atWoodLocation");
            _agent.AddAction(moveToWood);

            // Action: Move to stone location
            var moveToStone = new MoveToLocationAction("Stone", _stoneLocation, "atStoneLocation");
            _agent.AddAction(moveToStone);

            // Action: Move to crafting location
            var moveToCrafting = new MoveToLocationAction("Crafting", _craftingLocation, "atCraftingLocation");
            _agent.AddAction(moveToCrafting);

            // Action: Gather wood
            var gatherWood = new GatherResourceAction("Wood", gatherTime: 2f);
            _agent.AddAction(gatherWood);

            // Action: Gather stone
            var gatherStone = new GatherResourceAction("Stone", gatherTime: 2f);
            _agent.AddAction(gatherStone);

            // Action: Craft tool (requires wood and stone)
            var craftTool = new CraftToolAction();
            _agent.AddAction(craftTool);
        }

        private void OnPlanCreated(System.Collections.Generic.List<IGoapAction> plan)
        {
            Debug.Log($"<color=green>New plan created with {plan.Count} actions</color>");
        }

        private void OnPlanFailed()
        {
            Debug.LogWarning("<color=yellow>Failed to create a plan!</color>");
        }

        private void OnActionStarted(IGoapAction action)
        {
            Debug.Log($"<color=cyan>Starting action: {action.Name}</color>");
        }

        private void OnActionCompleted(IGoapAction action)
        {
            Debug.Log($"<color=lime>Completed action: {action.Name}</color>");
        }

        private void OnDestroy()
        {
            if (_agent != null)
            {
                _agent.OnPlanCreated -= OnPlanCreated;
                _agent.OnPlanFailed -= OnPlanFailed;
                _agent.OnActionStarted -= OnActionStarted;
                _agent.OnActionCompleted -= OnActionCompleted;
            }
        }

        // Custom action for moving to locations
        private class MoveToLocationAction : GoapAction
        {
            private Vector3 _targetPosition;
            private string _locationKey;
            private float _moveSpeed = 5f;
            private float _arrivalDistance = 0.5f;

            public MoveToLocationAction(string locationName, Vector3 targetPosition, string locationKey) 
                : base($"MoveTo{locationName}", cost: 1f)
            {
                _targetPosition = targetPosition;
                _locationKey = locationKey;

                // Effect: At the target location
                AddEffect(_locationKey, true);
            }

            public override bool Execute(GameObject agent)
            {
                float distance = Vector3.Distance(agent.transform.position, _targetPosition);
                
                if (distance <= _arrivalDistance)
                {
                    return true;
                }

                Vector3 direction = (_targetPosition - agent.transform.position).normalized;
                agent.transform.position += direction * _moveSpeed * Time.deltaTime;
                
                return false;
            }
        }

        // Custom action for crafting a tool
        private class CraftToolAction : GoapAction
        {
            private float _craftTime = 3f;
            private float _elapsedTime;

            public CraftToolAction() : base("CraftTool", cost: 3f)
            {
                // Preconditions: Must have wood, stone, and be at crafting location
                AddPrecondition("hasWood", true);
                AddPrecondition("hasStone", true);
                AddPrecondition("atCraftingLocation", true);

                // Effects: Have tool, consume resources
                AddEffect("hasTool", true);
                AddEffect("hasWood", false);
                AddEffect("hasStone", false);
            }

            public override void OnEnter(GameObject agent)
            {
                _elapsedTime = 0f;
                Debug.Log("Started crafting tool...");
            }

            public override bool Execute(GameObject agent)
            {
                _elapsedTime += Time.deltaTime;
                
                if (_elapsedTime >= _craftTime)
                {
                    Debug.Log("Finished crafting tool!");
                    return true;
                }

                return false;
            }
        }
    }
}
