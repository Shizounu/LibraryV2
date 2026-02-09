using UnityEngine;
using Shizounu.Library.BuffDebuff;

namespace Shizounu.Library.HealthAndDamage.Examples
{
    /// <summary>
    /// Example demonstrating the Health and Damage system integrated with BuffDebuff.
    /// Add this to a game object alongside HealthComponent and BuffDebuffSystem.
    /// </summary>
    public class HealthAndDamageExample : MonoBehaviour
    {
        private HealthComponent _healthComponent;
        private BuffDebuffSystem _buffDebuffSystem;
        private DamageDealer _damageDealer;

        private void Start()
        {
            _healthComponent = GetComponent<HealthComponent>();
            _buffDebuffSystem = GetComponent<BuffDebuffSystem>();
            _damageDealer = GetComponent<DamageDealer>();

            if (_healthComponent == null)
            {
                Debug.LogError("HealthAndDamageExample requires HealthComponent!", this);
                return;
            }

            if (_buffDebuffSystem == null)
            {
                Debug.LogError("HealthAndDamageExample requires BuffDebuffSystem!", this);
                return;
            }

            // Subscribe to health events
            _healthComponent.OnHealthChanged += OnHealthChanged;
            _healthComponent.OnDeath += OnDeath;
            _healthComponent.OnRevived += OnRevived;
            _healthComponent.OnFullyHealed += OnFullyHealed;
            _healthComponent.OnMaxHealthChanged += OnMaxHealthChanged;

            // Subscribe to buff events
            _buffDebuffSystem.OnEffectAdded += OnEffectAdded;
            _buffDebuffSystem.OnEffectRemoved += OnEffectRemoved;

            // Apply example buffs
            ApplyExampleEffects();
        }

        private void ApplyExampleEffects()
        {
            // Apply armor effect (reduces incoming damage)
            var armorEffect = new ArmorEffect("armor_buff", 15f, 30f, maxStackCount: 3);
            _buffDebuffSystem.AddEffect(armorEffect);

            // Apply health regeneration
            var regenEffect = new HealthRegenEffect("health_regen", -1f, 5f); // Infinite duration, 5 HP/sec
            _buffDebuffSystem.AddEffect(regenEffect);

            // Apply damage reduction through vulnerability (opposite of vulnerability - damage taken reduction)
            var fortifyEffect = new FortifyEffect("fortify", 8f, maxHealthBonus: 50f, armorBonus: 20f, maxStackCount: 2);
            _buffDebuffSystem.AddEffect(fortifyEffect);

            Debug.Log("Applied example effects to " + gameObject.name);
        }

        private void Update()
        {
            // Display current health and stats
            DisplayStats();

            // Example: Take damage when pressing Space
            if (Input.GetKeyDown(KeyCode.Space))
            {
                float damageAmount = 15f;
                float actualDamage = _healthComponent.TakeDamage(damageAmount, "example_attack", DamageType.Physical);
                Debug.Log($"Took {actualDamage} damage (base: {damageAmount})");
            }

            // Example: Heal when pressing H
            if (Input.GetKeyDown(KeyCode.H))
            {
                float healAmount = 20f;
                float actualHeal = _healthComponent.Heal(healAmount, "example_heal");
                Debug.Log($"Healed {actualHeal} HP");
            }

            // Example: Apply vulnerability debuff when pressing V
            if (Input.GetKeyDown(KeyCode.V))
            {
                var vulnEffect = new VulnerabilityEffect("vulnerability_curse", 5f, 0.5f); // 50% more damage taken
                _buffDebuffSystem.AddEffect(vulnEffect);
                Debug.Log("Applied vulnerability! Taking 50% more damage for 5 seconds");
            }

            // Example: Apply invulnerability when pressing I
            if (Input.GetKeyDown(KeyCode.I))
            {
                var invulnEffect = new InvulnerabilityEffect("godmode", 3f);
                _buffDebuffSystem.AddEffect(invulnEffect);
                Debug.Log("Invulnerable for 3 seconds!");
            }

            // Example: Deal damage to another entity when pressing D
            if (Input.GetKeyDown(KeyCode.D))
            {
                GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                foreach (GameObject obj in allObjects)
                {
                    if (obj != gameObject && obj.GetComponent<HealthComponent>() != null)
                    {
                        float damage = _damageDealer != null ? _damageDealer.GetEffectiveDamage() : 10f;
                        if (_damageDealer != null)
                        {
                            _damageDealer.DealDamageToTarget(obj, DamageType.Physical);
                            Debug.Log($"Dealt {damage} damage to {obj.name}");
                        }
                        break;
                    }
                }
            }
        }

        private void DisplayStats()
        {
            float effectiveMaxHealth = _healthComponent.EffectiveMaxHealth;
            float currentHealth = _healthComponent.CurrentHealth;
            float healthPercent = _healthComponent.HealthPercent;

            Debug.Log($"Health: {currentHealth:F1}/{effectiveMaxHealth:F1} ({healthPercent * 100:F1}%) - " +
                      $"Armor: {(_buffDebuffSystem?.GetModifierSum(HealthModifiers.ARMOR) ?? 0):F1} - " +
                      $"Effects: {_buffDebuffSystem?.GetActiveEffectCount()}");
        }

        private void OnHealthChanged(float oldHealth, float newHealth, float maxHealth)
        {
            Debug.Log($"Health changed: {oldHealth:F1} → {newHealth:F1} (max: {maxHealth:F1})");
        }

        private void OnDeath(string damageSource)
        {
            Debug.Log($"{gameObject.name} died! Damage source: {damageSource}");
        }

        private void OnRevived()
        {
            Debug.Log($"{gameObject.name} revived!");
        }

        private void OnFullyHealed()
        {
            Debug.Log($"{gameObject.name} is fully healed!");
        }

        private void OnMaxHealthChanged(float oldMax, float newMax)
        {
            Debug.Log($"Max health changed: {oldMax:F1} → {newMax:F1}");
        }

        private void OnEffectAdded(IBuffDebuffEffect effect)
        {
            Debug.Log($"Effect added: {effect.EffectID}");
        }

        private void OnEffectRemoved(IBuffDebuffEffect effect)
        {
            Debug.Log($"Effect removed: {effect.EffectID}");
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            if (_healthComponent != null)
            {
                _healthComponent.OnHealthChanged -= OnHealthChanged;
                _healthComponent.OnDeath -= OnDeath;
                _healthComponent.OnRevived -= OnRevived;
                _healthComponent.OnFullyHealed -= OnFullyHealed;
                _healthComponent.OnMaxHealthChanged -= OnMaxHealthChanged;
            }

            if (_buffDebuffSystem != null)
            {
                _buffDebuffSystem.OnEffectAdded -= OnEffectAdded;
                _buffDebuffSystem.OnEffectRemoved -= OnEffectRemoved;
            }
        }
    }

    /// <summary>
    /// Simple enemy AI example that uses Health and Damage systems.
    /// </summary>
    public class EnemyAIExample : MonoBehaviour
    {
        private HealthComponent _healthComponent;
        private BuffDebuffSystem _buffDebuffSystem;
        private DamageDealer _damageDealer;

        [SerializeField]
        private float _attackCooldown = 2f;

        private float _attackTimer = 0f;

        private void Start()
        {
            _healthComponent = GetComponent<HealthComponent>();
            _buffDebuffSystem = GetComponent<BuffDebuffSystem>();
            _damageDealer = GetComponent<DamageDealer>();

            _healthComponent.OnDeath += OnEnemyDeath;
        }

        private void Update()
        {
            if (_healthComponent.IsDead) return;

            _attackTimer -= Time.deltaTime;

            // Try to attack a nearby player/other entity
            if (_attackTimer <= 0)
            {
                AttackNearbyEntity();
                _attackTimer = _attackCooldown;
            }

            // Heal if health is low
            if (_healthComponent.HealthPercent < 0.3f)
            {
                _healthComponent.Heal(10f, "self_heal");
            }
        }

        private void AttackNearbyEntity()
        {
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject obj in allObjects)
            {
                if (obj != gameObject && obj.GetComponent<HealthComponent>() != null)
                {
                    if (_damageDealer != null)
                    {
                        _damageDealer.DealDamageToTarget(obj, DamageType.Physical);
                    }
                    break;
                }
            }
        }

        private void OnEnemyDeath(string damageSource)
        {
            Debug.Log($"Enemy {gameObject.name} was defeated by {damageSource}");
            // You could instantiate a death effect, drop loot, etc.
            Destroy(gameObject);
        }

        private void OnDisable()
        {
            if (_healthComponent != null)
            {
                _healthComponent.OnDeath -= OnEnemyDeath;
            }
        }
    }

    /// <summary>
    /// Example healer that can apply healing-related effects to targets.
    /// </summary>
    public class HealerExample : MonoBehaviour
    {
        private BuffDebuffSystem _buffDebuffSystem;

        [SerializeField]
        private float _healingPower = 25f;

        private void Start()
        {
            _buffDebuffSystem = GetComponent<BuffDebuffSystem>();
        }

        /// <summary>
        /// Heal a target entity.
        /// </summary>
        public void HealTarget(GameObject target)
        {
            HealthComponent targetHealth = target.GetComponent<HealthComponent>();
            if (targetHealth != null)
            {
                float effectiveHealing = _healingPower;

                // Apply healing buffs from this healer if they exist
                if (_buffDebuffSystem != null)
                {
                    float healingMultiplier = _buffDebuffSystem.GetModifierMultiplier(HealthModifiers.HEALING_DEALT_MULTIPLIER);
                    effectiveHealing *= healingMultiplier;
                }

                float actualHealing = targetHealth.Heal(effectiveHealing, "healer");
                Debug.Log($"Healed {target.name} for {actualHealing} HP");
            }
        }

        /// <summary>
        /// Apply a protective buff to a target (increases armor and health regen).
        /// </summary>
        public void BuffTarget(GameObject target)
        {
            BuffDebuffSystem targetBuff = target.GetComponent<BuffDebuffSystem>();
            if (targetBuff != null)
            {
                var protectionEffect = new FortifyEffect(
                    "healer_protection",
                    duration: 10f,
                    maxHealthBonus: 30f,
                    armorBonus: 20f
                );
                targetBuff.AddEffect(protectionEffect);
                Debug.Log($"Applied protective buff to {target.name}");
            }
        }

        /// <summary>
        /// Apply a curse/debuff to an enemy (increases damage taken).
        /// </summary>
        public void CurseEnemy(GameObject target)
        {
            BuffDebuffSystem targetBuff = target.GetComponent<BuffDebuffSystem>();
            if (targetBuff != null)
            {
                var curseEffect = new VulnerabilityEffect(
                    "healer_curse",
                    duration: 8f,
                    damageIncrease: 0.3f // 30% more damage taken
                );
                targetBuff.AddEffect(curseEffect);
                Debug.Log($"Cursed {target.name}!");
            }
        }
    }

    /// <summary>
    /// Example showing how to setup UI with Scriptable Architecture integration.
    /// This demonstrates the decoupled UI pattern where UI listens to scriptable variables
    /// instead of directly referencing the HealthComponent.
    /// </summary>
    public class ScriptableArchitectureUISetup : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Reference to the HealthComponent")]
        private HealthComponent _healthComponent;

        [Header("UI Elements")]
        [SerializeField]
        [Tooltip("Canvas with UI elements to test")]
        private Canvas _uiCanvas;

        [SerializeField]
        [Tooltip("Scriptable variables for health state")]
        private Shizounu.Library.ScriptableArchitecture.ScriptableFloat _playerCurrentHealth;

        [SerializeField]
        private Shizounu.Library.ScriptableArchitecture.ScriptableFloat _playerMaxHealth;

        [SerializeField]
        private Shizounu.Library.ScriptableArchitecture.ScriptableFloat _playerHealthPercent;

        [SerializeField]
        private Shizounu.Library.ScriptableArchitecture.ScriptableBool _playerIsDead;

        [SerializeField]
        [Tooltip("The HealthUIConnector component on the Canvas")]
        private HealthUIConnector _healthUIConnector;

        private void Start()
        {
            if (_healthComponent == null)
            {
                _healthComponent = FindFirstObjectByType<HealthComponent>();
            }

            // Setup scriptable architecture integration
            // This step is optional if you already assigned variables in inspector
            SetupScriptableVariables();
        }

        private void SetupScriptableVariables()
        {
            // Assign scriptable variables to HealthComponent
            // These will be automatically updated whenever health changes
            if (_healthComponent != null)
            {
                // Instead of direct assignment, the variables should be assigned in inspector
                // This example shows how the system works conceptually
                Debug.Log("HealthComponent is configured with Scriptable Variables:");
                Debug.Log($"  Current Health: {_playerCurrentHealth?.name ?? "Not Assigned"}");
                Debug.Log($"  Max Health: {_playerMaxHealth?.name ?? "Not Assigned"}");
                Debug.Log($"  Health Percent: {_playerHealthPercent?.name ?? "Not Assigned"}");
                Debug.Log($"  Is Dead: {_playerIsDead?.name ?? "Not Assigned"}");
            }

            // The HealthUIConnector automatically subscribes to variable changes
            // and updates the UI whenever the scriptable variables change
            if (_healthUIConnector != null)
            {
                Debug.Log("HealthUIConnector is ready to display health");
            }
        }

        private void Update()
        {
            // Interactive testing
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_healthComponent != null)
                {
                    float damage = 15f;
                    _healthComponent.TakeDamage(damage, "test_attack", DamageType.Physical);
                    Debug.Log($"Test damage applied: {damage}");
                }
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                if (_healthComponent != null)
                {
                    float healing = 20f;
                    _healthComponent.Heal(healing, "test_heal");
                    Debug.Log($"Test healing applied: {healing}");
                }
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (_healthComponent != null)
                {
                    _healthComponent.FullRestore();
                    Debug.Log("Health fully restored");
                }
            }

            // Display current health from scriptable variable
            if (_playerCurrentHealth != null)
            {
                if (Input.GetKeyDown(KeyCode.S))
                {
                    Debug.Log($"Scriptable Variable Current Health: {_playerCurrentHealth.RuntimeValue}");
                    Debug.Log($"Scriptable Variable Max Health: {_playerMaxHealth?.RuntimeValue ?? 0}");
                    Debug.Log($"Scriptable Variable Health %: {_playerHealthPercent?.RuntimeValue ?? 0}");
                    Debug.Log($"Scriptable Variable Is Dead: {_playerIsDead?.RuntimeValue ?? false}");
                }
            }
        }
    }

    /// <summary>
    /// Example demonstrating a networked health system using Scriptable Architecture.
    /// Multiple clients can subscribe to the same scriptable variables and see health updates.
    /// </summary>
    public class NetworkedHealthExample : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Scriptable float shared across network")]
        private Shizounu.Library.ScriptableArchitecture.ScriptableFloat _networkHealthVariable;

        [SerializeField]
        [Tooltip("UI connector that displays networked health")]
        private HealthUIConnector _networkHealthUIConnector;

        /// <summary>
        /// Simulates receiving health update from network.
        /// In a real networked game, this would come from a server/other player.
        /// </summary>
        public void ReceiveHealthUpdate(float currentHealth, float maxHealth, float healthPercent, bool isDead)
        {
            // Update the shared scriptable variable
            // All UI listening to this variable will update automatically
            if (_networkHealthVariable != null)
            {
                _networkHealthVariable.RuntimeValue = currentHealth;
                Debug.Log($"Network health update: {currentHealth}/{maxHealth}");
            }
        }

        /// <summary>
        /// Example of local player UI updating from scriptable variable.
        /// </summary>
        public void DisplayRemotePlayerHealth(
            Shizounu.Library.ScriptableArchitecture.ScriptableFloat currentHealth,
            Shizounu.Library.ScriptableArchitecture.ScriptableFloat maxHealth,
            Shizounu.Library.ScriptableArchitecture.ScriptableFloat healthPercent,
            Shizounu.Library.ScriptableArchitecture.ScriptableBool isDead)
        {
            // Simply connect any UI to these variables and they'll display automatically
            _networkHealthUIConnector.SetScriptableVariables(currentHealth, maxHealth, healthPercent, isDead);
            Debug.Log("Displaying remote player health");
        }
    }}