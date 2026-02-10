using UnityEngine;

namespace Shizounu.Library.Update
{
    /// <summary>
    /// Delegate for update callbacks invoked by the UpdateSystem.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since the last update in seconds.</param>
    /// <param name="context">Context information about the update execution.</param>
    public delegate void UpdateEvent(float deltaTime, UpdateContext context);

    /// <summary>
    /// Specifies which thread an update callback should execute on.
    /// </summary>
    public enum UpdateThreading
    {
        /// <summary>Execute on Unity's main thread (safe for all Unity API calls).</summary>
        MainThread,
        /// <summary>Execute on a dedicated background thread (no Unity API calls allowed).</summary>
        Background,
        /// <summary>Execute using Unity's Job System for parallel processing.</summary>
        JobSystem
    }

    /// <summary>
    /// Context information passed to update callbacks, including timing and thread information.
    /// </summary>
    public class UpdateContext
    {
        /// <summary>Time elapsed since the last update in seconds.</summary>
        public float DeltaTime { get; private set; }

        /// <summary>The managed thread ID where this update is executing.</summary>
        public int ThreadId { get; private set; }

        /// <summary>True if executing on Unity's main thread, false if on a background thread.</summary>
        public bool IsMainThread { get; private set; }

        /// <summary>
        /// Creates a new UpdateContext.
        /// </summary>
        public UpdateContext(float deltaTime, int threadId, bool isMainThread)
        {
            DeltaTime = deltaTime;
            ThreadId = threadId;
            IsMainThread = isMainThread;
        }
    }
}