using UnityEngine;
using Shizounu.Library.Tweening;

namespace Shizounu.Library.Examples
{
    /// <summary>
    /// Comprehensive examples demonstrating the LINQ-style tweening library.
    /// Shows all major features including basic tweens, easing, callbacks, composition, and more.
    /// </summary>
    public class TweeningExamples : MonoBehaviour
    {
        #region Example 1: Simple Float Tween

        /// <summary>
        /// Example: Tween a health bar value from 100 to 50 over 3 seconds.
        /// </summary>
        public void Example_SimpleTweenFloat()
        {
            float health = 100f;
            
            var tween = new TweenBuilder(3f)
                .Easing(EasingType.QuintOut)
                .OnUpdate(progress =>
                {
                    health = Mathf.Lerp(100f, 50f, progress);
                    Debug.Log($"Health: {health}");
                })
                .OnComplete(() => Debug.Log("Health reduction complete!"))
                .Build();
        }

        #endregion

        #region Example 2: Transform Tweening

        /// <summary>
        /// Example: Move a game object from its current position to a target position.
        /// </summary>
        public void Example_MoveTransform(Transform target, Vector3 destination)
        {
            target.TweenPosition(2f, destination)
                .Easing(EasingType.CubicInOut)
                .OnStart(() => Debug.Log("Movement started"))
                .OnComplete(() => Debug.Log("Movement complete"))
                .Build();
        }

        /// <summary>
        /// Example: Fade in and scale up a game object simultaneously.
        /// </summary>
        public void Example_FadeInAndScale(CanvasGroup canvasGroup, Transform target)
        {
            // Fade in
            canvasGroup.TweenAlpha(1f, 1f)
                .Easing(EasingType.SineOut)
                .Build();

            // Scale up (runs simultaneously)
            target.TweenScale(1f, 1.2f)
                .Easing(EasingType.CubicOut)
                .Build();
        }

        #endregion

        #region Example 3: Easing Functions

        /// <summary>
        /// Example: Compare different easing functions.
        /// </summary>
        public void Example_EasingFunctions(RectTransform uiElement)
        {
            var easing1 = uiElement.TweenAnchoredPosition(1f, new Vector2(100, 0))
                .Easing(EasingType.Linear)
                .Build();

            var easing2 = uiElement.TweenAnchoredPosition(1f, new Vector2(200, 0))
                .Easing(EasingType.SineInOut)
                .Build();

            var easing3 = uiElement.TweenAnchoredPosition(1f, new Vector2(300, 0))
                .Easing(EasingType.ElasticOut)
                .Build();

            var easing4 = uiElement.TweenAnchoredPosition(1f, new Vector2(400, 0))
                .Easing(EasingType.BounceOut)
                .Build();
        }

        #endregion

        #region Example 4: Sequential Tweens (Sequences)

        /// <summary>
        /// Example: Play tweens one after another in a sequence.
        /// </summary>
        public void Example_SequentialTweens(Transform target)
        {
            var sequence = TweenBuilder.Sequence()
                .Add(target.TweenPosition(1f, new Vector3(5, 0, 0)))      // Move right
                .AddDelay(0.5f)                                           // Wait 0.5 seconds
                .Add(target.TweenPosition(1f, new Vector3(5, 5, 0)))      // Move up
                .AddDelay(0.5f)                                           // Wait 0.5 seconds
                .Add(target.TweenPosition(1f, new Vector3(0, 5, 0)))      // Move left
                .AddDelay(0.5f)                                           // Wait 0.5 seconds
                .Add(target.TweenPosition(1f, new Vector3(0, 0, 0)))      // Move down (back to start)
                .AddCallback(() => Debug.Log("Sequence complete!"))       // Final callback
                .Play();
        }

        #endregion

        #region Example 5: Parallel Tweens (Groups)

        /// <summary>
        /// Example: Run multiple tweens at the same time.
        /// </summary>
        public void Example_ParallelTweens(Transform target)
        {
            var group = TweenBuilder.Parallel()
                .Add(target.TweenPosition(2f, new Vector3(10, 10, 0)))
                .Add(target.TweenRotation(2f, Quaternion.Euler(0, 0, 180)))
                .Add(target.TweenScale(2f, 2f))
                .OnComplete(() => Debug.Log("All tweens completed!"))
                .Play();
        }

        #endregion

        #region Example 6: Looping Tweens

        /// <summary>
        /// Example: Create a looping animation.
        /// </summary>
        public void Example_LoopingTween(Transform target)
        {
            // Bounce up and down infinitely
            target.TweenPosition(1f, new Vector3(0, 5, 0))
                .LoopInfinite()                     // Loop forever
                .Easing(EasingType.SineInOut)
                .Build();

            // Rotate infinitely with ping-pong
            target.TweenRotation(2f, Quaternion.Euler(0, 360, 0))
                .PingPong()                         // Go forward then backward
                .Build();

            // Scale pulsing effect (loop 4 times)
            target.TweenScale(0.5f, 1.2f)
                .Loop(LoopMode.PingPong, 4)        // Ping-pong 4 times
                .Easing(EasingType.SineInOut)
                .Build();
        }

        #endregion

        #region Example 7: Custom Easing Functions

        /// <summary>
        /// Example: Use custom easing functions.
        /// </summary>
        public void Example_CustomEasing(Transform target)
        {
            // Define a custom easing function
            EasingFunctions.EasingFunction customEase = (t) =>
            {
                // Custom easing: ease-in-sine + bounce hybrid
                return Mathf.Sin(t * Mathf.PI * 0.5f) + (t * t * 0.3f);
            };

            target.TweenPosition(2f, new Vector3(10, 0, 0))
                .Easing(customEase)
                .Build();
        }

        #endregion

        #region Example 8: Chaining with Then

        /// <summary>
        /// Example: Chain tweens using the fluent Then() method.
        /// </summary>
        public void Example_ChainingWithThen(Transform target)
        {
            target.TweenPosition(1f, new Vector3(5, 0, 0))
                .OnComplete(() =>
                {
                    Debug.Log("First tween done, starting second...");
                })
                .Build()
                .Then(target.TweenScale(1f, 1.5f).Build());
        }

        #endregion

        #region Example 9: UI Animation

        /// <summary>
        /// Example: Create a dialog opening animation.
        /// </summary>
        public void Example_UIAnimation(RectTransform dialogPanel, CanvasGroup canvasGroup)
        {
            // Initial state
            canvasGroup.alpha = 0;
            dialogPanel.localScale = Vector3.zero;

            var dialog = TweenBuilder.Sequence()
                .Add(new TweenBuilder(0.3f)
                    .OnUpdate(p => canvasGroup.alpha = p)
                    .Build())
                .Add(new TweenBuilder(0.5f)
                    .OnUpdate(p => dialogPanel.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, p))
                    .Easing(EasingType.CubicOut)
                    .Build())
                .AddCallback(() => Debug.Log("Dialog is now visible"))
                .Play();
        }

        #endregion

        #region Example 10: Color Tweening

        /// <summary>
        /// Example: Tween material colors.
        /// </summary>
        public void Example_ColorTween(Renderer renderer)
        {
            renderer.TweenColor(2f, Color.red)
                .Easing(EasingType.SineInOut)
                .Build();

            // Tween using the extension method directly
            var tween = TweenExtensions.TweenColor(3f, Color.green, Color.blue, color =>
            {
                renderer.material.color = color;
            })
            .Easing(EasingType.QuintOut)
            .Build();
        }

        #endregion

        #region Example 11: Speed Control and Delay

        /// <summary>
        /// Example: Use Speed and Delay for advanced control.
        /// </summary>
        public void Example_SpeedAndDelay(Transform target)
        {
            var tween = target.TweenPosition(2f, new Vector3(10, 0, 0))
                .Delay(1f)                  // Wait 1 second before starting
                .Speed(2f)                  // Play at 2x speed (finishes in 1 second)
                .Easing(EasingType.CubicInOut)
                .Build();
        }

        #endregion

        #region Example 12: Global Tween Management

        /// <summary>
        /// Example: Control all tweens globally.
        /// </summary>
        public void Example_GlobalTweenControl()
        {
            // Check how many tweens are active
            int activeTweens = Tweener.ActiveTweenCount;
            Debug.Log($"Active tweens: {activeTweens}");

            // Pause all tweens
            Tweener.PauseAll();

            // Resume all tweens
            Tweener.ResumeAll();

            // Kill all tweens
            Tweener.KillAll();
        }

        #endregion

        #region Example 13: Individual Tween Control

        /// <summary>
        /// Example: Control individual tweens.
        /// </summary>
        public void Example_IndividualTweenControl(Transform target)
        {
            var tween = target.TweenPosition(5f, new Vector3(10, 0, 0))
                .Easing(EasingType.CubicInOut)
                .Build();

            // Pause the tween
            tween.Pause();

            // Resume the tween
            tween.Resume();

            // Kill (stop and remove) the tween
            tween.Kill();

            // Instantly complete the tween
            tween.Complete();
        }

        #endregion

        #region Example 14: Custom Value Tweening

        /// <summary>
        /// Example: Tween custom numeric values.
        /// </summary>
        public void Example_CustomValueTween()
        {
            float score = 0f;
            const float targetScore = 1000f;

            TweenExtensions.TweenFloat(3f, 0f, targetScore, newScore =>
            {
                score = newScore;
                Debug.Log($"Current score: {score:F0}");
            })
            .Easing(EasingType.QuartOut)
            .Build();

            // Using vector tweening
            Vector3 objectPosition = Vector3.zero;
            TweenExtensions.TweenVector3(2f, Vector3.zero, new Vector3(10, 10, 10), newPos =>
            {
                objectPosition = newPos;
            })
            .Easing(EasingType.CubicInOut)
            .Build();
        }

        #endregion
    }
}
