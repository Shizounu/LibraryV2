using System;

namespace Shizounu.Library.Timer
{
    /// <summary>
    /// Lightweight identifier for a scheduled timer.
    /// </summary>
    public readonly struct TimerHandle : IEquatable<TimerHandle>
    {
        internal readonly Guid Id;

        internal TimerHandle(Guid id)
        {
            Id = id;
        }

        public bool IsValid => Id != Guid.Empty;

        public bool Equals(TimerHandle other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            return obj is TimerHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(TimerHandle left, TimerHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TimerHandle left, TimerHandle right)
        {
            return !left.Equals(right);
        }
    }
}
