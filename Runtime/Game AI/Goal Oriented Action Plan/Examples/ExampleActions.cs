using UnityEngine;

namespace Shizounu.Library.GameAI.GOAP.Examples
{
    /// <summary>
    /// Example action: Move to a target location.
    /// </summary>
    public class MoveToAction : GoapAction
    {
        private Vector3 _targetPosition;
        private float _moveSpeed;
        private float _arrivalDistance;

        public MoveToAction(Vector3 targetPosition, float moveSpeed = 5f, float arrivalDistance = 0.5f) 
            : base("MoveTo", cost: 1f)
        {
            _targetPosition = targetPosition;
            _moveSpeed = moveSpeed;
            _arrivalDistance = arrivalDistance;

            // Effects: Agent is at target location
            AddEffect("atTargetLocation", true);
        }

        public override bool Execute(GameObject agent)
        {
            float distance = Vector3.Distance(agent.transform.position, _targetPosition);
            
            if (distance <= _arrivalDistance)
            {
                return true; // Action complete
            }

            // Move towards target
            Vector3 direction = (_targetPosition - agent.transform.position).normalized;
            agent.transform.position += direction * _moveSpeed * Time.deltaTime;
            
            return false; // Still moving
        }
    }

    /// <summary>
    /// Example action: Gather a resource.
    /// </summary>
    public class GatherResourceAction : GoapAction
    {
        private string _resourceType;
        private float _gatherTime;
        private float _elapsedTime;

        public GatherResourceAction(string resourceType, float gatherTime = 2f) 
            : base($"Gather{resourceType}", cost: 2f)
        {
            _resourceType = resourceType;
            _gatherTime = gatherTime;

            // Preconditions: Must be at resource location
            AddPrecondition("atResourceLocation", true);

            // Effects: Has resource
            AddEffect($"has{resourceType}", true);
            AddEffect("atResourceLocation", false);
        }

        public override void OnEnter(GameObject agent)
        {
            _elapsedTime = 0f;
            Debug.Log($"Started gathering {_resourceType}");
        }

        public override bool Execute(GameObject agent)
        {
            _elapsedTime += Time.deltaTime;
            
            if (_elapsedTime >= _gatherTime)
            {
                Debug.Log($"Finished gathering {_resourceType}");
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Example action: Craft an item.
    /// </summary>
    public class CraftItemAction : GoapAction
    {
        private string _itemName;
        private string[] _requiredResources;
        private float _craftTime;
        private float _elapsedTime;

        public CraftItemAction(string itemName, string[] requiredResources, float craftTime = 3f) 
            : base($"Craft{itemName}", cost: 3f)
        {
            _itemName = itemName;
            _requiredResources = requiredResources;
            _craftTime = craftTime;

            // Preconditions: Must have all required resources
            foreach (var resource in _requiredResources)
            {
                AddPrecondition($"has{resource}", true);
            }

            // Effects: Has crafted item, consumes resources
            AddEffect($"has{itemName}", true);
            foreach (var resource in _requiredResources)
            {
                AddEffect($"has{resource}", false);
            }
        }

        public override void OnEnter(GameObject agent)
        {
            _elapsedTime = 0f;
            Debug.Log($"Started crafting {_itemName}");
        }

        public override bool Execute(GameObject agent)
        {
            _elapsedTime += Time.deltaTime;
            
            if (_elapsedTime >= _craftTime)
            {
                Debug.Log($"Finished crafting {_itemName}");
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Example action: Attack an enemy.
    /// </summary>
    public class AttackEnemyAction : GoapAction
    {
        private GameObject _target;
        private float _damage;
        private float _attackInterval;
        private float _lastAttackTime;

        public AttackEnemyAction(GameObject target, float damage = 10f, float attackInterval = 1f) 
            : base("AttackEnemy", cost: 1f)
        {
            _target = target;
            _damage = damage;
            _attackInterval = attackInterval;

            // Preconditions: Must be in attack range
            AddPrecondition("inAttackRange", true);

            // Effects: Enemy is defeated
            AddEffect("enemyDefeated", true);
        }

        public override bool IsAvailable(GameObject agent)
        {
            return _target != null;
        }

        public override bool Execute(GameObject agent)
        {
            if (_target == null)
                return true;

            if (Time.time - _lastAttackTime >= _attackInterval)
            {
                _lastAttackTime = Time.time;
                Debug.Log($"Attacking enemy for {_damage} damage");
                
                // Check if enemy is defeated (simplified)
                // In real implementation, check enemy's health
                return true; // For example purposes
            }

            return false;
        }
    }

    /// <summary>
    /// Example action: Rest to restore health.
    /// </summary>
    public class RestAction : GoapAction
    {
        private float _restTime;
        private float _elapsedTime;
        private float _healthRestoreAmount;

        public RestAction(float restTime = 3f, float healthRestoreAmount = 50f) 
            : base("Rest", cost: 2f)
        {
            _restTime = restTime;
            _healthRestoreAmount = healthRestoreAmount;

            // Preconditions: Must be at safe location
            AddPrecondition("atSafeLocation", true);

            // Effects: Health is restored
            AddEffect("isHealthy", true);
        }

        public override void OnEnter(GameObject agent)
        {
            _elapsedTime = 0f;
            Debug.Log("Started resting");
        }

        public override bool Execute(GameObject agent)
        {
            _elapsedTime += Time.deltaTime;
            
            if (_elapsedTime >= _restTime)
            {
                Debug.Log($"Finished resting, restored {_healthRestoreAmount} health");
                return true;
            }

            return false;
        }
    }
}
