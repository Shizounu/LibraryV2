using UnityEngine;
using Shizounu.Library.GameAI.StateMachine;

namespace Shizounu.Library.GameAI.GOAP.Examples
{
    /// <summary>
    /// Example demonstrating integration between GOAP and other AI systems
    /// using a shared Blackboard for state management.
    /// </summary>
    public class SharedBlackboardExample : MonoBehaviour
    {
        private GoapAgent _goapAgent;
        private StateMachine.StateMachine _stateMachine;
        private Blackboard _sharedBlackboard;

        private void Start()
        {
            // Create a shared blackboard that both systems can access
            _sharedBlackboard = new SimpleBlackboard();

            // Initialize GOAP agent with shared blackboard
            _goapAgent = gameObject.AddComponent<GoapAgent>();
            _goapAgent.InitializeWithBlackboard(_sharedBlackboard);

            // Initialize state machine (it creates its own blackboard in Awake)
            // But we can use the shared one by accessing it through the agent

            SetupSharedState();
            SetupGoapGoalsAndActions();

            Debug.Log("Shared Blackboard Integration Example initialized!");
            Debug.Log("Both GOAP and other systems can now read/write to the same blackboard.");
        }

        private void SetupSharedState()
        {
            // Set initial state that both systems can access
            _sharedBlackboard.SetValue("health", 100f);
            _sharedBlackboard.SetValue("ammo", 30);
            _sharedBlackboard.SetValue("hasWeapon", true);
            _sharedBlackboard.SetValue("enemiesNearby", false);
            _sharedBlackboard.SetValue("inCover", false);

            // Subscribe to state changes
            _sharedBlackboard.Subscribe("health", OnHealthChanged);
            _sharedBlackboard.Subscribe("ammo", OnAmmoChanged);
        }

        private void SetupGoapGoalsAndActions()
        {
            // Create goals that use the shared state
            var stayAliveGoal = new GoapGoal("StayAlive", priority: 10f)
                .WithCondition("health", 100f);
            _goapAgent.AddGoal(stayAliveGoal);

            var stayArmedGoal = new GoapGoal("StayArmed", priority: 7f)
                .WithCondition("hasWeapon", true)
                .WithCondition("ammo", 30);
            _goapAgent.AddGoal(stayArmedGoal);

            // Add actions that modify the shared state
            _goapAgent.AddAction(new FindHealthPackAction());
            _goapAgent.AddAction(new TakeCoverAction());
            _goapAgent.AddAction(new FindAmmoAction());
        }

        private void Update()
        {
            // Simulate external systems modifying the shared blackboard
            // In a real game, these would be updated by your game systems

            // Simulate damage
            if (Input.GetKeyDown(KeyCode.D))
            {
                var health = _sharedBlackboard.GetValue<float>("health");
                _sharedBlackboard.SetValue("health", Mathf.Max(0, health - 20f));
                Debug.Log($"<color=red>Took damage! Health: {_sharedBlackboard.GetValue<float>("health")}</color>");
            }

            // Simulate firing weapon
            if (Input.GetKeyDown(KeyCode.F))
            {
                var ammo = _sharedBlackboard.GetValue<int>("ammo");
                if (ammo > 0)
                {
                    _sharedBlackboard.SetValue("ammo", ammo - 1);
                    Debug.Log($"<color=yellow>Fired weapon! Ammo: {_sharedBlackboard.GetValue<int>("ammo")}</color>");
                }
            }

            // Toggle enemies nearby
            if (Input.GetKeyDown(KeyCode.E))
            {
                bool enemiesNearby = _sharedBlackboard.GetValue<bool>("enemiesNearby");
                _sharedBlackboard.SetValue("enemiesNearby", !enemiesNearby);
                Debug.Log($"<color=orange>Enemies nearby: {!enemiesNearby}</color>");
            }

            // Show current state
            if (Input.GetKeyDown(KeyCode.S))
            {
                ShowCurrentState();
            }
        }

        private void OnHealthChanged(object newValue)
        {
            float health = (float)newValue;
            Debug.Log($"<color=cyan>[Blackboard Event] Health changed to: {health}</color>");

            if (health < 50f)
            {
                Debug.LogWarning("Low health detected via blackboard subscription!");
            }
        }

        private void OnAmmoChanged(object newValue)
        {
            int ammo = (int)newValue;
            Debug.Log($"<color=cyan>[Blackboard Event] Ammo changed to: {ammo}</color>");

            if (ammo < 10)
            {
                Debug.LogWarning("Low ammo detected via blackboard subscription!");
            }
        }

        private void ShowCurrentState()
        {
            Debug.Log("=== Current Shared State ===");
            foreach (var entry in _sharedBlackboard.GetAllEntries())
            {
                Debug.Log($"{entry.Key} = {entry.Value}");
            }
            Debug.Log("==========================");
        }

        private void OnDestroy()
        {
            if (_sharedBlackboard != null)
            {
                _sharedBlackboard.Unsubscribe("health", OnHealthChanged);
                _sharedBlackboard.Unsubscribe("ammo", OnAmmoChanged);
            }
        }

        // Example actions that work with shared blackboard
        private class FindHealthPackAction : GoapAction
        {
            private float _healTime = 2f;
            private float _elapsed;

            public FindHealthPackAction() : base("FindHealthPack", cost: 3f)
            {
                // Only available when health is low
                AddEffect("health", 100f);
            }

            public override bool IsAvailable(GameObject agent)
            {
                var goapAgent = agent.GetComponent<GoapAgent>();
                var health = goapAgent.GetWorldState<float>("health");
                return health < 100f;
            }

            public override void OnEnter(GameObject agent)
            {
                _elapsed = 0f;
                Debug.Log("<color=green>Searching for health pack...</color>");
            }

            public override bool Execute(GameObject agent)
            {
                _elapsed += Time.deltaTime;
                
                if (_elapsed >= _healTime)
                {
                    Debug.Log("<color=green>Found and used health pack!</color>");
                    return true;
                }

                return false;
            }
        }

        private class TakeCoverAction : GoapAction
        {
            private float _moveTime = 1.5f;
            private float _elapsed;

            public TakeCoverAction() : base("TakeCover", cost: 2f)
            {
                AddPrecondition("enemiesNearby", true);
                AddEffect("inCover", true);
            }

            public override void OnEnter(GameObject agent)
            {
                _elapsed = 0f;
                Debug.Log("<color=blue>Moving to cover...</color>");
            }

            public override bool Execute(GameObject agent)
            {
                _elapsed += Time.deltaTime;
                
                if (_elapsed >= _moveTime)
                {
                    Debug.Log("<color=blue>In cover!</color>");
                    return true;
                }

                return false;
            }
        }

        private class FindAmmoAction : GoapAction
        {
            private float _searchTime = 2f;
            private float _elapsed;

            public FindAmmoAction() : base("FindAmmo", cost: 3f)
            {
                AddEffect("ammo", 30);
            }

            public override bool IsAvailable(GameObject agent)
            {
                var goapAgent = agent.GetComponent<GoapAgent>();
                var ammo = goapAgent.GetWorldState<int>("ammo");
                return ammo < 30;
            }

            public override void OnEnter(GameObject agent)
            {
                _elapsed = 0f;
                Debug.Log("<color=yellow>Searching for ammo...</color>");
            }

            public override bool Execute(GameObject agent)
            {
                _elapsed += Time.deltaTime;
                
                if (_elapsed >= _searchTime)
                {
                    Debug.Log("<color=yellow>Found ammo!</color>");
                    return true;
                }

                return false;
            }
        }
    }
}
