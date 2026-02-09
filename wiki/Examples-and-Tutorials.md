# Examples & Tutorials

This page links to complete, working code examples for all systems in Shizounu Library.

## Featured Examples

### Complete Game Setup

A full example showing all systems working together:

1. **Player Prefab Setup**
   ```csharp
   // All components configured and working together
   // See: Assets/Library/Examples/Player.prefab
   
   - HealthComponent (100 HP)
   - BuffDebuffSystem (active and updating)
   - DamageDealer (base 20 damage)
   - CharacterController (movement)
   ```

2. **Enemy Systems**
   ```csharp
   // Enemy with AI and health
   // See: Assets/Library/Examples/Enemy.prefab
   
   - HealthComponent
   - BuffDebuffSystem  
   - DamageDealer
   - AI behavior
   - Damage feedback
   ```

3. **UI Setup**
   ```csharp
   // Complete UI integration
   // See: Assets/Library/Examples/HealthUI.prefab
   
   - HealthUIConnector
   - All UI elements
   - Scriptable variables assigned
   ```

## Health & Damage Examples

### Example 1: Basic Combat

```csharp
using UnityEngine;
using Shizounu.Library.BuffDebuff;
using Shizounu.Library.HealthAndDamage;

public class BasicCombat : MonoBehaviour
{
    public GameObject enemy;
    
    private HealthComponent myHealth;
    private DamageDealer damageDealer;

    private void Start()
    {
        myHealth = GetComponent<HealthComponent>();
        damageDealer = GetComponent<DamageDealer>();
        
        myHealth.SetMaxHealth(100f);
        damageDealer.SetBaseDamage(20f);
        
        myHealth.OnDeath += OnDeath;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Attack enemy
            damageDealer.DealDamageToTarget(enemy, DamageType.Physical);
        }
        
        if (Input.GetKeyDown(KeyCode.H))
        {
            // Heal self
            myHealth.Heal(25f, \"potion\");
        }
    }

    private void OnDeath(string source)
    {
        Debug.Log(\"Character died!\");
        Destroy(gameObject);
    }
}
```

### Example 2: Enemy with Effects

```csharp
using UnityEngine;
using Shizounu.Library.BuffDebuff;
using Shizounu.Library.HealthAndDamage;

public class ToughEnemy : MonoBehaviour
{
    private HealthComponent health;
    private BuffDebuffSystem buffs;

    private void Start()
    {
        health = GetComponent<HealthComponent>();
        buffs = GetComponent<BuffDebuffSystem>();

        health.SetMaxHealth(150f);
        
        // This enemy has permanent armor
        buffs.AddEffect(new ArmorEffect(\"tough_hide\", -1f, 30f));
        
        health.OnDeath += OnDeath;
    }

    private void OnDeath(string source)
    {
        Debug.Log($\"Enemy defeated by {source}\");
        Destroy(gameObject);
    }

    public void TakeCriticalHit(float damage)
    {
        // Critical hits ignore armor!
        health.TakeDamage(damage, \"critical\", DamageType.True);
    }
}
```

### Example 3: Healing Spell

```csharp
using UnityEngine;
using Shizounu.Library.HealthAndDamage;

public class HealingSpell : MonoBehaviour
{
    public float healAmount = 40f;

    public void CastHealOnTarget(GameObject target)
    {
        HealthComponent targetHealth = target.GetComponent<HealthComponent>();
        
        if (targetHealth != null && targetHealth.IsAlive)
        {
            float actualHealing = targetHealth.Heal(healAmount, \"healing_spell\");
            Debug.Log($\"Healed {actualHealing} HP\");
        }
    }
}
```

## Buff/Debuff Examples

### Example 1: Damage Buff

```csharp
using UnityEngine;
using Shizounu.Library.BuffDebuff;

public class DamageBuffEffect : BuffDebuffEffect
{
    public DamageBuffEffect(float duration, float bonusPercent)
        : base(\"damage_buff\", duration, maxStackCount: 3)
    {
    }

    public override IStatModifier[] GetModifiers()
    {
        // 30% bonus per stack
        float totalBonus = 0.3f * StackCount;
        
        return new[]
        {
            new SimpleStatModifier(CommonModifiers.DAMAGE_MULTIPLIER, totalBonus)
        };
    }

    public override void OnApply()
    {
        base.OnApply();
        Debug.Log(\"Damage buff applied!\");
    }
}

// Usage
BuffDebuffSystem buffs = GetComponent<BuffDebuffSystem>();
var buff = new DamageBuffEffect(duration: 5f, bonusPercent: 0.3f);
buffs.AddEffect(buff);
```

### Example 2: Stun Effect

```csharp
using UnityEngine;
using Shizounu.Library.BuffDebuff;

public class StunEffect : BuffDebuffEffect
{
    private Renderer rendererComponent;

    public StunEffect(float duration, Renderer renderer)
        : base(\"stun\", duration, maxStackCount: 1)
    {
        rendererComponent = renderer;
    }

    public override IStatModifier[] GetModifiers()
    {
        return new[]
        {
            // Movement disabled
            new SimpleStatModifier(CommonModifiers.MOVEMENT_SPEED_MULTIPLIER, -1f),
            // Can't act
            new SimpleStatModifier(\"disabled\", 1f)
        };
    }

    public override void OnApply()
    {
        base.OnApply();
        rendererComponent.material.color = Color.blue; // Visual feedback
    }

    public override void OnRemove()
    {
        base.OnRemove();
        rendererComponent.material.color = Color.white; // Restore
    }
}
```

### Example 3: Querying Modifiers

```csharp
using UnityEngine;
using Shizounu.Library.BuffDebuff;

public class MovementWithBuffs : MonoBehaviour
{
    public float baseSpeed = 5f;
    private BuffDebuffSystem buffSystem;

    private void Start()
    {
        buffSystem = GetComponent<BuffDebuffSystem>();
    }

    private void Update()
    {
        float effectiveSpeed = GetEffectiveSpeed();
        
        Vector3 input = new Vector3(
            Input.GetAxis(\"Horizontal\"),
            0,
            Input.GetAxis(\"Vertical\")
        );
        
        transform.position += input * effectiveSpeed * Time.deltaTime;
    }

    private float GetEffectiveSpeed()
    {
        // Check if stunned
        if (buffSystem.GetEffect(\"stun\")?.IsActive ?? false)
            return 0f;
        
        float speed = baseSpeed;
        
        // Apply buff modifiers
        float multiplier = buffSystem.GetModifierMultiplier(
            CommonModifiers.MOVEMENT_SPEED_MULTIPLIER);
        speed *= multiplier;
        
        // Apply flat modifiers
        float flatBonus = buffSystem.GetModifierSum(
            CommonModifiers.MOVEMENT_SPEED_FLAT);
        speed += flatBonus;
        
        return Mathf.Max(0, speed);
    }
}
```

## Scriptable Architecture Examples

### Example 1: Health UI Integration

```csharp
using UnityEngine;
using UnityEngine.UI;
using Shizounu.Library.ScriptableArchitecture;
using Shizounu.Library.HealthAndDamage;

public class HealthUIIntegration : MonoBehaviour
{
    public ScriptableFloat currentHealthVar;
    public ScriptableFloat maxHealthVar;
    public Text healthText;

    private void Start()
    {
        // Listen to changes
        currentHealthVar.OnRuntimeValueChanged += UpdateHealthDisplay;
    }

    private void UpdateHealthDisplay(float currentHealth)
    {
        float maxHealth = maxHealthVar.RuntimeValue;
        healthText.text = $\"{currentHealth:F0}/{maxHealth:F0}\";
    }

    private void OnDestroy()
    {
        currentHealthVar.OnRuntimeValueChanged -= UpdateHealthDisplay;
    }
}
```

### Example 2: Game State Manager

```csharp
using UnityEngine;
using Shizounu.Library.ScriptableArchitecture;

public class GameStateManager : MonoBehaviour
{
    public ScriptableInt score;
    public ScriptableBool isGameOver;
    public ScriptableString gameState; // \"playing\", \"paused\", \"ended\"

    public void AddScore(int points)
    {
        score.RuntimeValue += points;
        // UI updates automatically!
    }

    public void GameOver()
    {
        isGameOver.RuntimeValue = true;
        gameState.RuntimeValue = \"ended\";
        // All listening systems respond instantly
    }

    public void PauseGame()
    {
        gameState.RuntimeValue = \"paused\";
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        gameState.RuntimeValue = \"playing\";
        Time.timeScale = 1f;
    }
}
```

## Character Controller Examples

### Example 1: Basic Player Controller

```csharp
using UnityEngine;
using Shizounu.Library.BuffDebuff;

public class SimplePlayerController : MonoBehaviour
{
    public float baseSpeed = 5f;
    private BuffDebuffSystem buffSystem;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        buffSystem = GetComponent<BuffDebuffSystem>();
    }

    private void Update()
    {
        Vector3 input = new Vector3(
            Input.GetAxis(\"Horizontal\"),
            0,
            Input.GetAxis(\"Vertical\")
        );
        
        // Query buffs for speed modifiers
        float speedMult = buffSystem.GetModifierMultiplier(
            CommonModifiers.MOVEMENT_SPEED_MULTIPLIER);
        
        float effectiveSpeed = baseSpeed * speedMult;
        
        // Apply movement
        rb.velocity = new Vector3(
            input.x * effectiveSpeed,
            rb.velocity.y,
            input.z * effectiveSpeed
        );
    }
}
```

## Pattern Examples

### Pattern: Damage Calculation

```csharp
public float CalculateDamage(GameObject attacker, GameObject target, float baseDamage, DamageType type)
{
    // Get attacker and target buff systems
    var attackerBuffs = attacker.GetComponent<BuffDebuffSystem>();
    var targetHealth = target.GetComponent<HealthComponent>();

    // Apply attacker's damage buffs
    float attackerMultiplier = attackerBuffs?.GetModifierMultiplier(
        CommonModifiers.DAMAGE_MULTIPLIER) ?? 1f;
    float effectiveDamage = baseDamage * attackerMultiplier;

    // Apply target's resistances
    float resistance = 0f;
    if (type == DamageType.Physical)
    {
        resistance = targetHealth.GetComponent<BuffDebuffSystem>()?
            .GetModifierSum(HealthModifiers.ARMOR) ?? 0f;
    }

    effectiveDamage = Mathf.Max(1f, effectiveDamage - resistance);

    return effectiveDamage;
}
```

### Pattern: Multi-Listener UI

```csharp
public class ScoreDisplay : MonoBehaviour
{
    public ScriptableInt playerScore;
    public ScriptableInt enemyScore;
    
    public Text playerScoreText;
    public Text enemyScoreText;

    private void Start()
    {
        playerScore.OnRuntimeValueChanged += UpdatePlayerScore;
        enemyScore.OnRuntimeValueChanged += UpdateEnemyScore;
    }

    private void UpdatePlayerScore(int score)
    {
        playerScoreText.text = $\"Player: {score}\";
    }

    private void UpdateEnemyScore(int score)
    {
        enemyScoreText.text = $\"Enemy: {score}\";
    }
}
```

## Complete Working Example

See the included example scenes in:
```
Assets/Scenes/
├── HealthAndDamageExample.scene
├── BuffDebuffExample.scene
└── UIIntegrationExample.scene
```

Each includes:
- Complete working setup
- Annotated code
- Keyboard controls for testing
- Debug output

## Test Your Knowledge

Try these challenges:

1. **Create a poison effect** that deals damage over 5 seconds
2. **Set up a buff** that increases armor by 25 for 10 seconds
3. **Create a UI** that tracks enemy health from a Scriptable Variable
4. **Implement lifesteal** that heals 20% of damage dealt
5. **Build an A effect** that stuns enemies for 3 seconds

Then check the example scenes to see how we solved them!

## See Also

- **[Getting Started](Getting-Started)** - Your first steps
- **[Health & Damage](Health-Damage-System)** - Full API reference
- **[Buff/Debuff](Buff-Debuff-System)** - Effect system details
- **[Best Practices](Best-Practices)** - Code organization tips
