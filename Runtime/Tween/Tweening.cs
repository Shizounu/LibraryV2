namespace Shizounu.Library.Tweening
{
    /// <summary>
    /// Complete LINQ-style tweening library for Unity.
    /// Runs asynchronously using the UpdateSystem for efficient, non-blocking animations.
    ///
    /// QUICK START EXAMPLES:
    ///
    /// 1. Simple fade-in animation:
    ///    new TweenBuilder(1f)
    ///        .Easing(EasingType.SineOut)
    ///        .OnUpdate(progress => canvasGroup.alpha = progress)
    ///        .Build();
    ///
    /// 2. Tween a transform's position:
    ///    transform.TweenPosition(2f, targetPosition)
    ///        .Easing(EasingType.CubicInOut)
    ///        .OnComplete(() => Debug.Log("Movement complete!"));
    ///
    /// 3. Tween a custom float value:
    ///    TweenExtensions.TweenFloat(3f, 0f, 100f, value =>
    ///    {
    ///        health = value;
    ///    })
    ///    .Easing(EasingType.QuintOut);
    ///
    /// 4. Chain multiple tweens sequentially:
    ///    var sequence = TweenBuilder.Sequence()
    ///        .Add(transform.TweenPosition(1f, pos1))
    ///        .Add(transform.TweenScale(1f, 1.5f))
    ///        .AddDelay(0.5f)
    ///        .AddCallback(() => Debug.Log("Done!"))
    ///        .Play();
    ///
    /// 5. Run tweens in parallel:
    ///    var group = TweenBuilder.Parallel()
    ///        .Add(transform.TweenPosition(2f, pos))
    ///        .Add(transform.TweenScale(2f, 2f))
    ///        .Play();
    ///
    /// 6. Loop infinitely with easing:
    ///    new TweenBuilder(1f)
    ///        .LoopInfinite()
    ///        .Easing(EasingType.SineInOut)
    ///        .OnUpdate(p => transform.Rotate(Vector3.up * p * 360f))
    ///        .Build();
    ///
    /// FEATURES:
    /// - Fluent LINQ-style API with method chaining
    /// - 30+ easing functions (Linear, Sine, Cubic, Elastic, Bounce, etc.)
    /// - Async/non-blocking updates via UpdateSystem
    /// - Support for floats, vectors, colors, rotations, and custom values
    /// - Transform tweening (position, rotation, scale)
    /// - UI tweening (position, size, alpha)
    /// - Sequential and parallel tween composition
    /// - Looping (repeat, ping-pong)
    /// - Delay support
    /// - Speed control
    /// - Custom easing functions
    /// - Global tween management (pause all, resume all, kill all)
    /// </summary>
    public static class Tweening
    {
        /// <summary>
        /// Creates a new tween builder with fluent configuration API.
        /// </summary>
        public static TweenBuilder Create(float duration)
        {
            return Tweener.Create(duration);
        }

        /// <summary>
        /// Gets the current number of active tweens.
        /// </summary>
        public static int ActiveTweenCount => Tweener.ActiveTweenCount;

        /// <summary>
        /// Kills all active tweens.
        /// </summary>
        public static void KillAll()
        {
            Tweener.KillAll();
        }

        /// <summary>
        /// Pauses all active tweens.
        /// </summary>
        public static void PauseAll()
        {
            Tweener.PauseAll();
        }

        /// <summary>
        /// Resumes all paused tweens.
        /// </summary>
        public static void ResumeAll()
        {
            Tweener.ResumeAll();
        }

        /// <summary>
        /// Creates a tween sequence that plays tweens one after another.
        /// </summary>
        public static TweenSequence Sequence()
        {
            return TweenBuilder.Sequence();
        }

        /// <summary>
        /// Creates a tween group that plays tweens in parallel.
        /// </summary>
        public static TweenGroup Parallel()
        {
            return TweenBuilder.Parallel();
        }
    }
}
