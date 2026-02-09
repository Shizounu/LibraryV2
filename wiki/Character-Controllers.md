# Character Controllers - Quick Start

The **Character Controllers** module provides ready-to-use movement and control systems for player characters.

## Features

- Built-in player controller
- Input handling
- Ground detection
- Jump mechanics
- Animation integration
- Works with Rigidbody physics

## Basic Setup

### Add To Player GameObject

```csharp
gameObject.AddComponent<CharacterController>();
```

### Configure in Inspector

```
Character Controller
├── Speed: 5.0
├── Jump Force: 5.0
├── Ground Check
│   ├── Layers: Ground
│   └─ Radius: 0.1
├── Animation
│   ├── Animator: [assigned]
│   └── Speed Param: \"Speed\"
```

## Simple Example

```csharp
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 velocity;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float horizontal = Input.GetAxis(\"Horizontal\");
        float vertical = Input.GetAxis(\"Vertical\");
        
        Vector3 input = new Vector3(horizontal, 0, vertical);
        controller.Move(input);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            controller.Jump();
        }
    }
}
```

## Integration with Buffs

```csharp
public class CharacterWithBuffs : MonoBehaviour
{
    private CharacterController controller;
    private BuffDebuffSystem buffs;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        buffs = GetComponent<BuffDebuffSystem>();
    }

    private void Update()
    {
        // Get speed modifier from buffs
        float speedMult = buffs.GetModifierMultiplier(
            CommonModifiers.MOVEMENT_SPEED_MULTIPLIER);
        
        controller.speed *= speedMult;
        
        // Movement
        float horizontal = Input.GetAxis(\"Horizontal\");
        float vertical = Input.GetAxis(\"Vertical\");
        controller.Move(new Vector3(horizontal, 0, vertical));
    }
}
```

## API Reference

```csharp
// Movement
controller.Move(Vector3 direction);

// Jumping
controller.Jump();
bool isGrounded = controller.IsGrounded;

// Configuration
controller.speed = 5f;
controller.jumpForce = 5f;
controller.acceleration = 10f;
```

## See Also

- **[Examples](Examples-and-Tutorials)** - Complete working examples
- **[Best Practices](Best-Practices)** - Integration tips
