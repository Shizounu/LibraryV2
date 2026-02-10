using System;

namespace Shizounu.Library.Timer
{
    /// <summary>
    /// Which time source a timer should use.
    /// </summary>
    public enum TimerUpdateMode
    {
        ScaledTime,
        UnscaledTime
    }

    /// <summary>
    /// Current lifecycle state of a timer.
    /// </summary>
    public enum TimerState
    {
        Running,
        Paused,
        Completed,
        Cancelled
    }

    /// <summary>
    /// Snapshot of a timer's current state.
    /// </summary>
    public readonly struct TimerInfo
    {
        public float Duration { get; }
        public float Remaining { get; }
        public TimerUpdateMode UpdateMode { get; }
        public TimerState State { get; }
        public int RepeatCountRemaining { get; }

        public TimerInfo(float duration, float remaining, TimerUpdateMode updateMode, TimerState state, int repeatCountRemaining)
        {
            Duration = duration;
            Remaining = remaining;
            UpdateMode = updateMode;
            State = state;
            RepeatCountRemaining = repeatCountRemaining;
        }
    }

    /// <summary>
    /// Snapshot of a timer with its handle and info.
    /// </summary>
    public readonly struct TimerSnapshot
    {
        public TimerHandle Handle { get; }
        public TimerInfo Info { get; }

        public TimerSnapshot(TimerHandle handle, TimerInfo info)
        {
            Handle = handle;
            Info = info;
        }
    }

    /// <summary>
    /// Snapshot of a named cooldown.
    /// </summary>
    public readonly struct CooldownSnapshot
    {
        public string Key { get; }
        public float Remaining { get; }
        public bool IsReady { get; }

        public CooldownSnapshot(string key, float remaining, bool isReady)
        {
            Key = key;
            Remaining = remaining;
            IsReady = isReady;
        }
    }
}
