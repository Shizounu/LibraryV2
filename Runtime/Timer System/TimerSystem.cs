using System;
using System.Collections.Generic;
using UnityEngine;
using Shizounu.Library.Update;
using Shizounu.Library.Utility;


namespace Shizounu.Library.Timer
{
    /// <summary>
    /// Centralized timer and cooldown manager driven by the UpdateSystem.
    /// </summary>
    public class TimerSystem : Singleton<TimerSystem>
    {
        private sealed class TimerData
        {
            public Guid Id;
            public float Duration;
            public float Remaining;
            public TimerUpdateMode UpdateMode;
            public TimerState State;
            public int RepeatCount;
            public Action OnComplete;
            public Action<float> OnTick;
        }

        private readonly object _syncLock = new object();
        private readonly Dictionary<Guid, TimerData> _timers = new Dictionary<Guid, TimerData>();
        private readonly Dictionary<string, TimerHandle> _cooldowns = new Dictionary<string, TimerHandle>();
        private bool _isPaused;

        public bool IsPaused => _isPaused;

        public TimerSystem() : base()
        {
            
            UpdateSystem.Instance.RegisterCallback(OnUpdate, 0f, UpdateThreading.MainThread);
        }

        /// <summary>
        /// Create and start a timer.
        /// </summary>
        /// <param name="duration">Duration in seconds.</param>
        /// <param name="onComplete">Callback invoked when the timer completes.</param>
        /// <param name="updateMode">Scaled or unscaled time.</param>
        /// <param name="repeatCount">Number of repeats. Use -1 for infinite repeats.</param>
        /// <param name="startPaused">Start in a paused state.</param>
        /// <param name="onTick">Optional callback invoked with remaining time.</param>
        public TimerHandle StartTimer(
            float duration,
            Action onComplete = null,
            TimerUpdateMode updateMode = TimerUpdateMode.ScaledTime,
            int repeatCount = 0,
            bool startPaused = false,
            Action<float> onTick = null)
        {
            return StartTimerInternal(duration, onComplete, updateMode, repeatCount, startPaused, onTick);
        }

        /// <summary>
        /// Start or reuse a named cooldown. If already active, returns the existing handle.
        /// </summary>
        public TimerHandle StartCooldown(
            string key,
            float duration,
            Action onReady = null,
            TimerUpdateMode updateMode = TimerUpdateMode.ScaledTime)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Cooldown key cannot be null or empty.", nameof(key));

            lock (_syncLock)
            {
                if (_cooldowns.TryGetValue(key, out var existing) && IsTimerActiveLocked(existing))
                {
                    return existing;
                }
            }

            TimerHandle handle = StartTimerInternal(duration, onReady, updateMode, 0, false, null);
            if (!handle.IsValid)
                return default;

            lock (_syncLock)
            {
                _cooldowns[key] = handle;
            }

            return handle;
        }

        public bool CancelTimer(TimerHandle handle)
        {
            if (!handle.IsValid)
                return false;

            lock (_syncLock)
            {
                if (!_timers.TryGetValue(handle.Id, out var timer))
                    return false;

                timer.State = TimerState.Cancelled;
                _timers.Remove(handle.Id);
                RemoveCooldownByIdLocked(handle.Id);
                return true;
            }
        }

        public bool PauseTimer(TimerHandle handle)
        {
            return SetTimerState(handle, TimerState.Paused);
        }

        public bool ResumeTimer(TimerHandle handle)
        {
            return SetTimerState(handle, TimerState.Running);
        }

        public bool RestartTimer(TimerHandle handle, float? newDuration = null)
        {
            if (!handle.IsValid)
                return false;

            lock (_syncLock)
            {
                if (!_timers.TryGetValue(handle.Id, out var timer))
                    return false;

                float duration = newDuration ?? timer.Duration;
                if (duration <= 0f)
                {
                    timer.State = TimerState.Completed;
                    _timers.Remove(handle.Id);
                    RemoveCooldownByIdLocked(handle.Id);
                    return false;
                }

                timer.Duration = duration;
                timer.Remaining = duration;
                timer.State = TimerState.Running;
                return true;
            }
        }

        public bool TryGetTimerInfo(TimerHandle handle, out TimerInfo info)
        {
            if (!handle.IsValid)
            {
                info = default;
                return false;
            }

            lock (_syncLock)
            {
                if (_timers.TryGetValue(handle.Id, out var timer))
                {
                    info = new TimerInfo(
                        timer.Duration,
                        timer.Remaining,
                        timer.UpdateMode,
                        timer.State,
                        timer.RepeatCount);
                    return true;
                }
            }

            info = default;
            return false;
        }

        public bool IsTimerActive(TimerHandle handle)
        {
            if (!handle.IsValid)
                return false;

            lock (_syncLock)
            {
                return IsTimerActiveLocked(handle);
            }
        }

        public bool IsCooldownReady(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return true;

            lock (_syncLock)
            {
                if (!_cooldowns.TryGetValue(key, out var handle))
                    return true;

                if (IsTimerActiveLocked(handle))
                    return false;

                _cooldowns.Remove(key);
                return true;
            }
        }

        public bool TryGetCooldownRemaining(string key, out float remaining)
        {
            remaining = 0f;
            if (string.IsNullOrWhiteSpace(key))
                return false;

            lock (_syncLock)
            {
                if (!_cooldowns.TryGetValue(key, out var handle))
                    return false;

                if (_timers.TryGetValue(handle.Id, out var timer))
                {
                    remaining = Mathf.Max(0f, timer.Remaining);
                    return true;
                }

                _cooldowns.Remove(key);
                return false;
            }
        }

        public bool CancelCooldown(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            lock (_syncLock)
            {
                if (!_cooldowns.TryGetValue(key, out var handle))
                    return false;

                _cooldowns.Remove(key);
                if (_timers.TryGetValue(handle.Id, out var timer))
                {
                    timer.State = TimerState.Cancelled;
                    _timers.Remove(handle.Id);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get a snapshot of all known timers.
        /// </summary>
        public List<TimerSnapshot> GetTimersSnapshot()
        {
            var snapshot = new List<TimerSnapshot>();

            lock (_syncLock)
            {
                foreach (var pair in _timers)
                {
                    var timer = pair.Value;
                    var handle = new TimerHandle(timer.Id);
                    var info = new TimerInfo(
                        timer.Duration,
                        timer.Remaining,
                        timer.UpdateMode,
                        timer.State,
                        timer.RepeatCount);
                    snapshot.Add(new TimerSnapshot(handle, info));
                }
            }

            return snapshot;
        }

        /// <summary>
        /// Get a snapshot of all named cooldowns.
        /// </summary>
        public List<CooldownSnapshot> GetCooldownSnapshots()
        {
            var snapshot = new List<CooldownSnapshot>();

            lock (_syncLock)
            {
                foreach (var pair in _cooldowns)
                {
                    float remaining = 0f;
                    bool isReady = true;

                    if (_timers.TryGetValue(pair.Value.Id, out var timer))
                    {
                        remaining = Mathf.Max(0f, timer.Remaining);
                        isReady = timer.State == TimerState.Completed || timer.State == TimerState.Cancelled;
                    }

                    snapshot.Add(new CooldownSnapshot(pair.Key, remaining, isReady));
                }
            }

            return snapshot;
        }

        public void PauseAll()
        {
            _isPaused = true;
        }

        public void ResumeAll()
        {
            _isPaused = false;
        }

        private TimerHandle StartTimerInternal(
            float duration,
            Action onComplete,
            TimerUpdateMode updateMode,
            int repeatCount,
            bool startPaused,
            Action<float> onTick)
        {
            if (duration <= 0f)
            {
                onComplete?.Invoke();
                return default;
            }

            if (repeatCount < -1)
                repeatCount = -1;

            var id = Guid.NewGuid();
            var timer = new TimerData
            {
                Id = id,
                Duration = duration,
                Remaining = duration,
                UpdateMode = updateMode,
                State = startPaused ? TimerState.Paused : TimerState.Running,
                RepeatCount = repeatCount,
                OnComplete = onComplete,
                OnTick = onTick
            };

            lock (_syncLock)
            {
                _timers[id] = timer;
            }

            return new TimerHandle(id);
        }

        private bool SetTimerState(TimerHandle handle, TimerState state)
        {
            if (!handle.IsValid)
                return false;

            lock (_syncLock)
            {
                if (!_timers.TryGetValue(handle.Id, out var timer))
                    return false;

                if (timer.State == TimerState.Completed || timer.State == TimerState.Cancelled)
                    return false;

                timer.State = state;
                return true;
            }
        }

        private bool IsTimerActiveLocked(TimerHandle handle)
        {
            return _timers.TryGetValue(handle.Id, out var timer) &&
                   timer.State != TimerState.Completed &&
                   timer.State != TimerState.Cancelled;
        }

        private void RemoveCooldownByIdLocked(Guid id)
        {
            if (_cooldowns.Count == 0)
                return;

            List<string> toRemove = null;
            foreach (var pair in _cooldowns)
            {
                if (pair.Value.Id == id)
                {
                    toRemove ??= new List<string>();
                    toRemove.Add(pair.Key);
                }
            }

            if (toRemove == null)
                return;

            foreach (var key in toRemove)
            {
                _cooldowns.Remove(key);
            }
        }

        private void OnUpdate(float deltaTime, UpdateContext context)
        {
            if (_isPaused)
                return;

            float scaledDelta = Time.deltaTime;
            float unscaledDelta = Time.unscaledDeltaTime;

            List<Action> callbacks = null;
            List<Guid> completedIds = null;

            lock (_syncLock)
            {
                foreach (var pair in _timers)
                {
                    var timer = pair.Value;
                    if (timer.State != TimerState.Running)
                        continue;

                    float dt = timer.UpdateMode == TimerUpdateMode.ScaledTime ? scaledDelta : unscaledDelta;
                    if (dt <= 0f)
                        continue;

                    timer.Remaining -= dt;

                    if (timer.OnTick != null)
                    {
                        float remainingForTick = Mathf.Max(0f, timer.Remaining);
                        callbacks ??= new List<Action>();
                        callbacks.Add(() => timer.OnTick(remainingForTick));
                    }

                    if (timer.Remaining > 0f)
                        continue;

                    int completionCount = 0;
                    while (timer.Remaining <= 0f)
                    {
                        completionCount++;

                        if (timer.RepeatCount == 0)
                        {
                            timer.State = TimerState.Completed;
                            break;
                        }

                        if (timer.RepeatCount > 0)
                            timer.RepeatCount--;

                        timer.Remaining += timer.Duration;

                        if (timer.RepeatCount == 0 && timer.Remaining <= 0f)
                        {
                            timer.State = TimerState.Completed;
                            break;
                        }
                    }

                    if (timer.OnComplete != null && completionCount > 0)
                    {
                        callbacks ??= new List<Action>();
                        for (int i = 0; i < completionCount; i++)
                        {
                            callbacks.Add(timer.OnComplete);
                        }
                    }

                    if (timer.State == TimerState.Completed)
                    {
                        completedIds ??= new List<Guid>();
                        completedIds.Add(timer.Id);
                    }
                }

                if (completedIds != null)
                {
                    foreach (var id in completedIds)
                    {
                        _timers.Remove(id);
                        RemoveCooldownByIdLocked(id);
                    }
                }
            }

            if (callbacks == null)
                return;

            foreach (var callback in callbacks)
            {
                try
                {
                    callback?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in TimerSystem callback: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
    }
}
