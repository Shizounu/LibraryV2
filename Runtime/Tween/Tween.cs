using System;
using UnityEngine;

namespace Shizounu.Library.Tweening
{
    /// <summary>
    /// Represents a single tween animation that can be played, paused, and controlled.
    /// This is the core class that tracks animation progress and applies interpolation.
    /// </summary>
    public class Tween
    {
        #region Event Declarations

        /// <summary>Called when the tween starts.</summary>
        public event Action OnStart;

        /// <summary>Called every frame while the tween is playing.</summary>
        public event Action<float> OnUpdate;

        /// <summary>Called when the tween completes.</summary>
        public event Action OnComplete;

        /// <summary>Called when the tween loops (if looping enabled).</summary>
        public event Action OnLoop;

        /// <summary>Called when the tween is paused.</summary>
        public event Action OnPause;

        /// <summary>Called when the tween is resumed.</summary>
        public event Action OnResume;

        /// <summary>Called when the tween is killed/cancelled.</summary>
        public event Action OnKill;

        #endregion

        #region Properties

        /// <summary>Unique identifier for this tween.</summary>
        public int Id { get; private set; }

        /// <summary>Total duration of the tween in seconds.</summary>
        public float Duration { get; private set; }

        /// <summary>Delay before the tween starts in seconds.</summary>
        public float Delay { get; private set; }

        /// <summary>Current elapsed time of the tween (including delay).</summary>
        public float ElapsedTime { get; private set; }

        /// <summary>Progress of the tween from 0 to 1 (excluding delay).</summary>
        public float Progress { get; private set; }

        /// <summary>Is the tween currently playing?</summary>
        public bool IsPlaying { get; private set; }

        /// <summary>Is the tween paused?</summary>
        public bool IsPaused { get; private set; }

        /// <summary>Is the tween completed?</summary>
        public bool IsComplete { get; private set; }

        /// <summary>Is the tween killed?</summary>
        public bool IsKilled { get; private set; }

        /// <summary>Number of times the tween has looped.</summary>
        public int LoopCount { get; private set; }

        /// <summary>Loop mode for this tween.</summary>
        public LoopMode LoopMode { get; set; }

        /// <summary>Maximum number of loops (-1 for infinite).</summary>
        public int MaxLoops { get; set; }

        #endregion

        #region Private Fields

        private EasingFunctions.EasingFunction _easingFunction = EasingFunctions.Linear;
        private float _speed = 1f;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new Tween with a unique ID.
        /// </summary>
        public Tween(int id, float duration)
        {
            Id = id;
            Duration = duration;
            Delay = 0f;
            ElapsedTime = 0f;
            Progress = 0f;
            IsPlaying = false;
            IsPaused = false;
            IsComplete = false;
            IsKilled = false;
            LoopCount = 0;
            LoopMode = LoopMode.Once;
            MaxLoops = 1;
        }

        #endregion

        #region Configuration Methods

        /// <summary>
        /// Sets the easing function for this tween.
        /// </summary>
        public Tween SetEasing(EasingType easingType)
        {
            _easingFunction = EasingFunctions.GetEasingFunction(easingType);
            return this;
        }

        /// <summary>
        /// Sets a custom easing function for this tween.
        /// </summary>
        public Tween SetEasing(EasingFunctions.EasingFunction easingFunction)
        {
            _easingFunction = easingFunction ?? EasingFunctions.Linear;
            return this;
        }

        /// <summary>
        /// Sets the delay before the tween starts.
        /// </summary>
        public Tween SetDelay(float delay)
        {
            Delay = Mathf.Max(0f, delay);
            return this;
        }

        /// <summary>
        /// Sets the animation speed multiplier.
        /// </summary>
        public Tween SetSpeed(float speed)
        {
            _speed = Mathf.Max(0.001f, speed);
            return this;
        }

        /// <summary>
        /// Sets the loop mode and maximum loops.
        /// </summary>
        public Tween SetLoop(LoopMode loopMode, int maxLoops = -1)
        {
            LoopMode = loopMode;
            MaxLoops = maxLoops;
            return this;
        }

        #endregion

        #region Callback Methods

        /// <summary>
        /// Registers a callback to be invoked when the tween starts.
        /// </summary>
        public Tween OnStartCallback(Action callback)
        {
            if (callback != null) OnStart += callback;
            return this;
        }

        /// <summary>
        /// Registers a callback to be invoked every frame the tween updates.
        /// </summary>
        public Tween OnUpdateCallback(Action<float> callback)
        {
            if (callback != null) OnUpdate += callback;
            return this;
        }

        /// <summary>
        /// Registers a callback to be invoked when the tween completes.
        /// </summary>
        public Tween OnCompleteCallback(Action callback)
        {
            if (callback != null) OnComplete += callback;
            return this;
        }

        /// <summary>
        /// Registers a callback to be invoked when the tween loops.
        /// </summary>
        public Tween OnLoopCallback(Action callback)
        {
            if (callback != null) OnLoop += callback;
            return this;
        }

        #endregion

        #region Playback Control

        /// <summary>
        /// Starts playing this tween.
        /// </summary>
        public void Play()
        {
            if (IsKilled) return;
            IsPlaying = true;
            IsPaused = false;
        }

        /// <summary>
        /// Pauses this tween.
        /// </summary>
        public void Pause()
        {
            if (!IsPlaying || IsPaused || IsKilled) return;
            IsPaused = true;
            IsPlaying = false;
            OnPause?.Invoke();
        }

        /// <summary>
        /// Resumes this tween from pause.
        /// </summary>
        public void Resume()
        {
            if (!IsPaused || IsKilled) return;
            IsPaused = false;
            IsPlaying = true;
            OnResume?.Invoke();
        }

        /// <summary>
        /// Stops and removes this tween.
        /// </summary>
        public void Kill()
        {
            if (IsKilled) return;
            IsKilled = true;
            IsPlaying = false;
            IsPaused = false;
            OnKill?.Invoke();

            // Clear all callbacks
            OnStart = null;
            OnUpdate = null;
            OnComplete = null;
            OnLoop = null;
            OnPause = null;
            OnResume = null;
            OnKill = null;
        }

        #endregion

        #region Update Logic

        /// <summary>
        /// Updates the tween by the given delta time. Returns true if the tween is still active.
        /// </summary>
        internal bool Update(float deltaTime)
        {
            if (IsKilled || !IsPlaying)
                return !IsKilled;

            // Apply speed modifier
            float scaledDeltaTime = deltaTime * _speed;
            ElapsedTime += scaledDeltaTime;

            // Check if still in delay phase
            if (ElapsedTime < Delay)
            {
                return true;
            }

            // Calculate animation time (excluding delay)
            float animationTime = ElapsedTime - Delay;

            // Handle looping
            if (animationTime >= Duration)
            {
                if (MaxLoops == -1 || LoopCount < MaxLoops - 1)
                {
                    // Can loop
                    switch (LoopMode)
                    {
                        case LoopMode.Loop:
                            animationTime = animationTime % Duration;
                            break;
                        case LoopMode.PingPong:
                            float totalTime = animationTime;
                            float cycleTime = Duration * 2f;
                            totalTime = totalTime % cycleTime;
                            animationTime = totalTime <= Duration ? totalTime : cycleTime - totalTime;
                            break;
                    }

                    LoopCount++;
                    OnLoop?.Invoke();
                }
                else
                {
                    // Complete
                    animationTime = Duration;
                    IsPlaying = false;
                    IsComplete = true;
                }
            }

            // Calculate progress (0-1)
            Progress = Mathf.Clamp01(animationTime / Duration);

            // Invoke start event on first update
            if (Progress > 0f && ElapsedTime - scaledDeltaTime <= Delay)
            {
                OnStart?.Invoke();
            }

            // Apply easing and invoke update
            float easedProgress = _easingFunction(Progress);
            OnUpdate?.Invoke(easedProgress);

            // Check completion
            if (IsComplete)
            {
                OnComplete?.Invoke();
                return false;
            }

            return true;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Instantly completes this tween and invokes all completion callbacks.
        /// </summary>
        public void Complete()
        {
            if (IsKilled) return;
            ElapsedTime = Delay + Duration;
            Progress = 1f;
            IsPlaying = false;
            IsComplete = true;
            OnStart?.Invoke();
            OnUpdate?.Invoke(1f);
            OnComplete?.Invoke();
        }

        /// <summary>
        /// Gets a simple string representation of this tween's state.
        /// </summary>
        public override string ToString()
        {
            return $"Tween#{Id} [Duration: {Duration}s, Progress: {Progress:P}, State: {(IsKilled ? "Killed" : IsComplete ? "Complete" : IsPaused ? "Paused" : IsPlaying ? "Playing" : "Stopped")}]";
        }

        #endregion
    }

    /// <summary>
    /// Defines how a tween loops.
    /// </summary>
    public enum LoopMode
    {
        /// <summary>Tween plays once and stops.</summary>
        Once,
        /// <summary>Tween repeats from the start.</summary>
        Loop,
        /// <summary>Tween goes forward then backward.</summary>
        PingPong,
    }
}
