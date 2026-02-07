using UnityEngine;

namespace Shizounu.Library.GameAI.GOAP.Examples
{
    /// <summary>
    /// Advanced GOAP example demonstrating dynamic goals and complex behaviors.
    /// This example shows an AI that manages health, gathers resources, and fights enemies.
    /// </summary>
    public class AdvancedGoapExample : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _currentHealth = 100f;
        [SerializeField] private GameObject _enemyTarget;

        private GoapAgent _agent;

        private void Start()
        {
            _agent = GetComponent<GoapAgent>();
            if (_agent == null)
            {
                _agent = gameObject.AddComponent<GoapAgent>();
            }

            InitializeWorldState();
            SetupDynamicGoals();
            SetupActions();

            Debug.Log("Advanced GOAP Example initialized!");
        }

        private void Update()
        {
            // Update world state based on current conditions
            UpdateWorldState();
        }

        private void InitializeWorldState()
        {
            _agent.SetWorldState("isHealthy", true);
            _agent.SetWorldState("hasWeapon", false);
            _agent.SetWorldState("enemyDefeated", false);
            _agent.SetWorldState("atSafeLocation", false);
            _agent.SetWorldState("inAttackRange", false);
        }

        private void UpdateWorldState()
        {
            // Update health status
            bool isHealthy = _currentHealth >= _maxHealth * 0.5f;
            _agent.SetWorldState("isHealthy", isHealthy);

            // Update enemy status
            bool enemyExists = _enemyTarget != null;
            _agent.SetWorldState("enemyDefeated", !enemyExists);

            // Update attack range (simplified)
            if (_enemyTarget != null)
            {
                float distanceToEnemy = Vector3.Distance(transform.position, _enemyTarget.transform.position);
                _agent.SetWorldState("inAttackRange", distanceToEnemy < 3f);
            }
        }

        private void SetupDynamicGoals()
        {
            // High priority: Survive (be healthy)
            var surviveGoal = new DynamicHealthGoal(this);
            _agent.AddGoal(surviveGoal);

            // Medium priority: Defeat enemy
            var defeatEnemyGoal = new DynamicEnemyGoal(this);
            _agent.AddGoal(defeatEnemyGoal);

            // Low priority: Get weapon
            var getWeaponGoal = new GoapGoal("GetWeapon", priority: 3f)
                .WithCondition("hasWeapon", true);
            _agent.AddGoal(getWeaponGoal);
        }

        private void SetupActions()
        {
            // Rest to restore health
            _agent.AddAction(new RestAction(restTime: 3f, healthRestoreAmount: 50f));

            // Find safe location
            _agent.AddAction(new FindSafeLocationAction());

            // Get weapon
            _agent.AddAction(new GetWeaponAction());

            // Move to attack range
            _agent.AddAction(new MoveToAttackRangeAction(_enemyTarget));

            // Attack enemy
            _agent.AddAction(new AttackEnemyAction(_enemyTarget, damage: 10f));
        }

        // Dynamic goal that increases priority when health is low
        private class DynamicHealthGoal : GoapGoal
        {
            private AdvancedGoapExample _example;

            public DynamicHealthGoal(AdvancedGoapExample example) 
                : base("StayHealthy", priority: 5f)
            {
                _example = example;
                WithCondition("isHealthy", true);
            }

            public override bool IsRelevant(GameObject agent)
            {
                // Priority increases as health decreases
                float healthPercent = _example._currentHealth / _example._maxHealth;
                WithPriority(healthPercent < 0.5f ? 15f : 5f);
                
                return _example._currentHealth < _example._maxHealth;
            }
        }

        // Dynamic goal for defeating enemies
        private class DynamicEnemyGoal : GoapGoal
        {
            private AdvancedGoapExample _example;

            public DynamicEnemyGoal(AdvancedGoapExample example) 
                : base("DefeatEnemy", priority: 8f)
            {
                _example = example;
                WithCondition("enemyDefeated", true);
            }

            public override bool IsRelevant(GameObject agent)
            {
                // Only relevant when enemy exists and we're healthy
                bool enemyExists = _example._enemyTarget != null;
                bool isHealthy = _example._currentHealth >= _example._maxHealth * 0.3f;
                
                return enemyExists && isHealthy;
            }
        }

        // Custom actions
        private class FindSafeLocationAction : GoapAction
        {
            private float _moveTime = 2f;
            private float _elapsed;

            public FindSafeLocationAction() : base("FindSafeLocation", cost: 1f)
            {
                AddEffect("atSafeLocation", true);
            }

            public override void OnEnter(GameObject agent)
            {
                _elapsed = 0f;
                Debug.Log("Moving to safe location...");
            }

            public override bool Execute(GameObject agent)
            {
                _elapsed += Time.deltaTime;
                return _elapsed >= _moveTime;
            }
        }

        private class GetWeaponAction : GoapAction
        {
            private float _getTime = 1f;
            private float _elapsed;

            public GetWeaponAction() : base("GetWeapon", cost: 2f)
            {
                AddEffect("hasWeapon", true);
            }

            public override void OnEnter(GameObject agent)
            {
                _elapsed = 0f;
                Debug.Log("Getting weapon...");
            }

            public override bool Execute(GameObject agent)
            {
                _elapsed += Time.deltaTime;
                return _elapsed >= _getTime;
            }
        }

        private class MoveToAttackRangeAction : GoapAction
        {
            private GameObject _target;
            private float _moveTime = 1.5f;
            private float _elapsed;

            public MoveToAttackRangeAction(GameObject target) 
                : base("MoveToAttackRange", cost: 1f)
            {
                _target = target;
                AddPrecondition("hasWeapon", true);
                AddEffect("inAttackRange", true);
            }

            public override bool IsAvailable(GameObject agent)
            {
                return _target != null;
            }

            public override void OnEnter(GameObject agent)
            {
                _elapsed = 0f;
                Debug.Log("Moving to attack range...");
            }

            public override bool Execute(GameObject agent)
            {
                _elapsed += Time.deltaTime;
                return _elapsed >= _moveTime;
            }
        }
    }
}
