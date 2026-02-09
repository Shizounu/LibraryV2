using System;
using UnityEngine;

namespace Shizounu.Library.Tweening
{
    /// <summary>
    /// Fluent LINQ-style builder for creating and configuring tweens.
    /// Supports method chaining for elegant tween construction.
    /// </summary>
    public class TweenBuilder
    {
        private Tween _tween;
        private bool _autoPlay = true;

        #region Constructors

        /// <summary>
        /// Creates a new TweenBuilder with a specified duration.
        /// </summary>
        public TweenBuilder(float duration)
        {
            _tween = Tweener.CreateTween(duration);
        }

        #endregion

        #region Configuration Chain Methods

        /// <summary>
        /// Sets the easing function (fluent style).
        /// </summary>
        public TweenBuilder Easing(EasingType easingType)
        {
            _tween.SetEasing(easingType);
            return this;
        }

        /// <summary>
        /// Sets a custom easing function (fluent style).
        /// </summary>
        public TweenBuilder Easing(EasingFunctions.EasingFunction easingFunction)
        {
            _tween.SetEasing(easingFunction);
            return this;
        }

        /// <summary>
        /// Sets the delay before the tween starts (fluent style).
        /// </summary>
        public TweenBuilder Delay(float delay)
        {
            _tween.SetDelay(delay);
            return this;
        }

        /// <summary>
        /// Sets the animation speed multiplier (fluent style).
        /// </summary>
        public TweenBuilder Speed(float speed)
        {
            _tween.SetSpeed(speed);
            return this;
        }

        /// <summary>
        /// Sets the loop configuration (fluent style).
        /// </summary>
        public TweenBuilder Loop(LoopMode loopMode, int maxLoops = -1)
        {
            _tween.SetLoop(loopMode, maxLoops);
            return this;
        }

        /// <summary>
        /// Enables infinite looping (fluent style).
        /// </summary>
        public TweenBuilder LoopInfinite()
        {
            _tween.SetLoop(LoopMode.Loop, -1);
            return this;
        }

        /// <summary>
        /// Enables ping-pong looping (fluent style).
        /// </summary>
        public TweenBuilder PingPong(int loopCount = -1)
        {
            _tween.SetLoop(LoopMode.PingPong, loopCount);
            return this;
        }

        /// <summary>
        /// Disables auto-play so the tween starts manually.
        /// </summary>
        public TweenBuilder NoAutoPlay()
        {
            _autoPlay = false;
            return this;
        }

        #endregion

        #region Callback Chain Methods

        /// <summary>
        /// Registers a callback on tween start (fluent style).
        /// </summary>
        public TweenBuilder OnStart(Action callback)
        {
            _tween.OnStartCallback(callback);
            return this;
        }

        /// <summary>
        /// Registers a callback on tween update (fluent style).
        /// Progress value ranges from 0 to 1.
        /// </summary>
        public TweenBuilder OnUpdate(Action<float> callback)
        {
            _tween.OnUpdateCallback(callback);
            return this;
        }

        /// <summary>
        /// Registers a callback on tween completion (fluent style).
        /// </summary>
        public TweenBuilder OnComplete(Action callback)
        {
            _tween.OnCompleteCallback(callback);
            return this;
        }

        /// <summary>
        /// Registers a callback on tween loop (fluent style).
        /// </summary>
        public TweenBuilder OnLoop(Action callback)
        {
            _tween.OnLoopCallback(callback);
            return this;
        }

        /// <summary>
        /// Chains multiple callbacks to be called on completion.
        /// </summary>
        public TweenBuilder Then(Action callback)
        {
            return OnComplete(callback);
        }

        #endregion

        #region Build Methods

        /// <summary>
        /// Gets the underlying Tween object and starts it if auto-play is enabled.
        /// </summary>
        public Tween Build()
        {
            if (_autoPlay)
            {
                _tween.Play();
            }
            return _tween;
        }

        /// <summary>
        /// Builds and immediately plays the tween.
        /// </summary>
        public Tween Play()
        {
            _tween.Play();
            return _tween;
        }

        /// <summary>
        /// Builds the tween without playing.
        /// </summary>
        public Tween BuildStopped()
        {
            return _tween;
        }

        /// <summary>
        /// Implicit conversion to Tween.
        /// </summary>
        public static implicit operator Tween(TweenBuilder builder)
        {
            return builder.Build();
        }

        #endregion

        #region Composite Pattern - Chain Multiple Tweens

        /// <summary>
        /// Creates a sequence that plays tweens one after another.
        /// </summary>
        public static TweenSequence Sequence()
        {
            return new TweenSequence();
        }

        /// <summary>
        /// Creates a parallel group that plays tweens simultaneously.
        /// </summary>
        public static TweenGroup Parallel()
        {
            return new TweenGroup();
        }

        #endregion
    }

    /// <summary>
    /// Represents a sequence of tweens that play one after another.
    /// </summary>
    public class TweenSequence
    {
        private Tween _rootTween;
        private Tween _lastTween;

        public TweenSequence()
        {
            // Create a dummy tween that serves as the sequence container
            _rootTween = Tweener.CreateTween(0f);
            _lastTween = _rootTween;
        }

        /// <summary>
        /// Adds a tween to the sequence.
        /// </summary>
        public TweenSequence Add(Tween tween)
        {
            _lastTween.OnCompleteCallback(() => tween.Play());
            _lastTween = tween;
            return this;
        }

        /// <summary>
        /// Adds a tween builder to the sequence.
        /// </summary>
        public TweenSequence Add(TweenBuilder builder)
        {
            return Add(builder.BuildStopped());
        }

        /// <summary>
        /// Adds a delay to the sequence.
        /// </summary>
        public TweenSequence AddDelay(float delay)
        {
            return Add(Tweener.CreateTween(delay));
        }

        /// <summary>
        /// Registers a callback to be called at this point in the sequence.
        /// </summary>
        public TweenSequence AddCallback(Action callback)
        {
            var callbackTween = Tweener.CreateTween(0f);
            callbackTween.OnCompleteCallback(callback);
            return Add(callbackTween);
        }

        /// <summary>
        /// Builds and plays the sequence.
        /// </summary>
        public Tween Play()
        {
            _rootTween.Play();
            return _rootTween;
        }

        /// <summary>
        /// Builds the sequence without auto-playing.
        /// </summary>
        public Tween Build()
        {
            return _rootTween;
        }
    }

    /// <summary>
    /// Represents a group of tweens that run in parallel.
    /// </summary>
    public class TweenGroup
    {
        private Tween _groupTween;
        private System.Collections.Generic.List<Tween> _tweens = new();
        private int _completedCount = 0;
        private Action _onGroupComplete;

        public TweenGroup()
        {
            // Create a tween to represent the group
            _groupTween = Tweener.CreateTween(0f);
        }

        /// <summary>
        /// Adds a tween to the parallel group.
        /// </summary>
        public TweenGroup Add(Tween tween)
        {
            _tweens.Add(tween);

            // When this tween completes, check if all are done
            tween.OnCompleteCallback(() =>
            {
                _completedCount++;
                if (_completedCount >= _tweens.Count)
                {
                    _onGroupComplete?.Invoke();
                }
            });

            return this;
        }

        /// <summary>
        /// Adds a tween builder to the parallel group.
        /// </summary>
        public TweenGroup Add(TweenBuilder builder)
        {
            return Add(builder.BuildStopped());
        }

        /// <summary>
        /// Builds and plays all tweens in the group.
        /// </summary>
        public Tween Play()
        {
            foreach (var tween in _tweens)
            {
                tween.Play();
            }
            return _groupTween;
        }

        /// <summary>
        /// Registers a callback to be called when all tweens complete.
        /// </summary>
        public TweenGroup OnComplete(Action callback)
        {
            _onGroupComplete += callback;
            return this;
        }

        /// <summary>
        /// Builds the group without auto-playing.
        /// </summary>
        public Tween Build()
        {
            return _groupTween;
        }
    }

    /// <summary>
    /// Provides extension methods for creating tweens with fluent syntax.
    /// </summary>
    public static class TweenBuilderExtensions
    {
        /// <summary>
        /// Creates a new tween builder with the specified duration.
        /// </summary>
        public static TweenBuilder Tween(this float duration)
        {
            return new TweenBuilder(duration);
        }

        /// <summary>
        /// Creates a new tween builder with the specified duration.
        /// </summary>
        public static TweenBuilder Tween(this int duration)
        {
            return new TweenBuilder(duration);
        }
    }
}
