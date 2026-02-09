# LINQ-Style Tweening Library for Unity

A comprehensive, asynchronous tweening library for Unity C# that runs non-blocking using the UpdateSystem.

## Overview

This tweening library provides:
- **Fluent LINQ-style API** with method chaining for elegant tween construction
- **30+ easing functions** (Linear, Sine, Cubic, Elastic, Bounce, etc.)
- **Asynchronous execution** via UpdateSystem (non-blocking animations)
- **Transform tweening** (position, rotation, scale)
- **UI tweening** (anchored position, size, alpha)
- **Color tweening** for renderers and UI elements
- **Sequence composition** (tweens played one after another)
- **Parallel composition** (tweens played simultaneously)
- **Looping support** (repeat, ping-pong, infinite)
- **Global tween management** (pause/resume/kill all)
- **Per-tween control** (pause, resume, kill individual tweens)

## Quick Start

### Basic Tween

```csharp
// Fade in a canvas group over 1 second
canvasGroup.TweenAlpha(1f, 1f)
    .Easing(EasingType.SineOut)
    .Build();
```

### Transform Animation

```csharp
// Move transform to position over 2 seconds with easing
transform.TweenPosition(2f, targetPosition)
    .Easing(EasingType.CubicInOut)
    .OnComplete(() => Debug.Log("Movement done!"))
    .Build();
```

### Chaining Callbacks

```csharp
transform.TweenScale(1f, 2f)
    .Easing(EasingType.QuintOut)
    .OnStart(() => Debug.Log("Scale started"))
    .OnUpdate(progress => Debug.Log($"Progress: {progress:P}"))
    .OnComplete(() => Debug.Log("Scale complete"))
    .Build();
```

## Core Classes

### `Tweening` (Static Facade)
Main entry point for the library with global management methods.

```csharp
// Create a tween
var tween = Tweening.Create(2f);

// Global controls
Tweening.PauseAll();
Tweening.ResumeAll();
Tweening.KillAll();
int activeTweens = Tweening.ActiveTweenCount;
```

### `TweenBuilder`
Fluent builder for configuring tweens.

```csharp
new TweenBuilder(3f)
    .Easing(EasingType.SineOut)
    .Delay(0.5f)
    .Speed(1.5f)
    .Loop(LoopMode.PingPong, 3)
    .OnUpdate(progress => { /* ... */ })
    .Build();
```

### `Tween`
Represents an individual tween animation.

```csharp
var tween = new TweenBuilder(2f).Build();
tween.Play();
tween.Pause();
tween.Resume();
tween.Kill();
tween.Complete(); // Instantly complete
```

### `TweenSequence`
Compose tweens to play sequentially.

```csharp
TweenBuilder.Sequence()
    .Add(transform.TweenPosition(1f, pos1))
    .Add(transform.TweenScale(1f, 1.5f))
    .AddDelay(0.5f)
    .AddCallback(() => Debug.Log("Done!"))
    .Play();
```

### `TweenGroup`
Compose tweens to play in parallel.

```csharp
TweenBuilder.Parallel()
    .Add(transform.TweenPosition(2f, pos))
    .Add(transform.TweenRotation(2f, rot))
    .Add(canvasGroup.TweenAlpha(2f, 0))
    .OnComplete(() => Debug.Log("All done!"))
    .Play();
```

## Configuration Methods

### Easing
```csharp
.Easing(EasingType.CubicInOut)           // Predefined easing
.Easing(t => t * t)                      // Custom easing function
```

### Delay & Speed
```csharp
.Delay(1f)       // Wait 1 second before starting
.Speed(2f)       // Play at 2x speed
```

### Looping
```csharp
.Loop(LoopMode.Once)               // No loop
.Loop(LoopMode.Loop, 3)            // Repeat 3 times
.Loop(LoopMode.PingPong)           // Forward and backward
.LoopInfinite()                    // Loop forever
.PingPong(5)                       // Ping-pong 5 times
```

### Callbacks
```csharp
.OnStart(() => { })                // Called when tween starts
.OnUpdate(progress => { })         // Called every frame (progress 0-1)
.OnComplete(() => { })             // Called when tween finishes
.OnLoop(() => { })                 // Called when loop repeats
.Then(() => { })                   // Shorthand for OnComplete
```

## Extension Methods

### Transform Tweening
```csharp
transform.TweenPosition(duration, targetPos, localPosition: false)
transform.TweenRotation(duration, targetRot, localRotation: false)
transform.TweenScale(duration, targetScale)
transform.TweenScale(duration, uniformScale) // Uniform scaling
```

### RectTransform Tweening
```csharp
rectTransform.TweenAnchoredPosition(duration, targetPos)
rectTransform.TweenSizeDelta(duration, targetSize)
```

### CanvasGroup Tweening
```csharp
canvasGroup.TweenAlpha(duration, targetAlpha)
```

### Renderer Tweening
```csharp
renderer.TweenColor(duration, targetColor)
renderer.TweenColor(duration, targetColor, propertyName: "_PropertyName")
```

### Value Tweening
```csharp
TweenExtensions.TweenFloat(duration, from, to, onUpdate)
TweenExtensions.TweenVector2(duration, from, to, onUpdate)
TweenExtensions.TweenVector3(duration, from, to, onUpdate)
TweenExtensions.TweenColor(duration, from, to, onUpdate)
TweenExtensions.TweenQuaternion(duration, from, to, onUpdate)
```

### Utility Methods
```csharp
TweenExtensions.Wait(duration, onComplete)        // Delay tween
TweenExtensions.DoAction(duration, action)        // Action tween
```

## Easing Functions

### Linear
- `Linear`

### Quadratic
- `QuadIn`, `QuadOut`, `QuadInOut`

### Cubic
- `CubicIn`, `CubicOut`, `CubicInOut`

### Quartic
- `QuartIn`, `QuartOut`, `QuartInOut`

### Quintic
- `QuintIn`, `QuintOut`, `QuintInOut`

### Sine
- `SineIn`, `SineOut`, `SineInOut`

### Exponential
- `ExpoIn`, `ExpoOut`, `ExpoInOut`

### Circular
- `CircIn`, `CircOut`, `CircInOut`

### Elastic
- `ElasticIn`, `ElasticOut`, `ElasticInOut`

### Back
- `BackIn`, `BackOut`, `BackInOut`

### Bounce
- `BounceIn`, `BounceOut`, `BounceInOut`

## Advanced Examples

### Complex Sequence
```csharp
var animation = TweenBuilder.Sequence()
    .AddCallback(() => animator.SetTrigger("Jump"))
    .AddDelay(0.5f)
    .Add(transform.TweenPosition(0.3f, jumpPeak))
    .Add(transform.TweenPosition(0.3f, landingPos))
    .AddCallback(() => animator.SetTrigger("Land"))
    .Play();
```

### Parallel Animation with Effects
```csharp
var effect = TweenBuilder.Parallel()
    .Add(transform.TweenScale(0.5f, 0))
    .Add(canvasGroup.TweenAlpha(0.5f, 0))
    .Add(new TweenBuilder(0.5f)
        .OnUpdate(p => transform.Rotate(Vector3.forward * p * 360f))
        .Build())
    .OnComplete(() => Destroy(gameObject))
    .Play();
```

### Looping Movement Pattern
```csharp
TweenBuilder.Sequence()
    .Add(transform.TweenPosition(1f, pos1))
    .Add(transform.TweenPosition(1f, pos2))
    .Add(transform.TweenPosition(1f, pos3))
    .Loop(LoopMode.Loop, -1) // Repeat infinitely
    .Play();
```

### UI Dialog Animation
```csharp
TweenBuilder.Sequence()
    .Add(new TweenBuilder(0.2f)
        .OnUpdate(p => dialogPanel.alpha = p)
        .Easing(EasingType.SineOut)
        .Build())
    .Add(new TweenBuilder(0.3f)
        .OnUpdate(p => dialogPanel.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, p))
        .Easing(EasingType.CubicOut)
        .Build())
    .AddDelay(0.5f)
    .AddCallback(() => OnDialogShown())
    .Play();
```

## Performance Considerations

- Tweens run on the main thread asynchronously via UpdateSystem
- No MonoBehaviour required - tweens work independently
- Tweens are garbage collected when completed
- Use `.Kill()` to explicitly stop tweens
- Use `Tweening.KillAll()` when changing scenes or unloading

## Architecture

The library consists of:
- **EasingFunctions.cs**: 30+ easing function implementations
- **Tween.cs**: Core tween object managing state and progress
- **TweenBuilder.cs**: Fluent builder API for composing tweens
- **Tweener.cs**: Singleton manager handling all active tweens
- **TweenExtensions.cs**: Extension methods for common use cases
- **Tweening.cs**: Static facade for global management

The tweener integrates with the UpdateSystem for asynchronous execution without blocking the main thread.

## Thread Safety

- All tweens must be created and used on the main thread
- The UpdateSystem handles thread safety internally
- Do not modify tweens from background threads

## License

Part of the Shizounu Library system.
