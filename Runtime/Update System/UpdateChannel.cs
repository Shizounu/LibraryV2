using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Shizounu.Library.Update
{
    /// <summary>
    /// Manages callbacks for a specific update interval and threading mode.
    /// Handles subscription/unsubscription and invokes callbacks at the appropriate time.
    /// </summary>
    public class UpdateChannel
    {
        #region Properties

        /// <summary>How frequently this channel should invoke its callbacks (in seconds).</summary>
        public float UpdateInterval { get; private set; }

        /// <summary>Which thread this channel executes callbacks on.</summary>
        public UpdateThreading Threading { get; private set; }

        #endregion

        #region Fields

        private float _timeSinceLastUpdate = 0f;
        private readonly object _syncLock = new object();

        private readonly ConcurrentBag<UpdateEvent> _callbacks = new ConcurrentBag<UpdateEvent>();
        private readonly ConcurrentBag<UpdateEvent> _callbacksToAdd = new ConcurrentBag<UpdateEvent>();
        private readonly ConcurrentBag<UpdateEvent> _callbacksToRemove = new ConcurrentBag<UpdateEvent>();

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new UpdateChannel.
        /// </summary>
        /// <param name="updateInterval">How often callbacks should be invoked (in seconds).</param>
        /// <param name="threading">Which thread to execute callbacks on.</param>
        public UpdateChannel(float updateInterval, UpdateThreading threading = UpdateThreading.MainThread)
        {
            UpdateInterval = updateInterval;
            Threading = threading;
        }

        #endregion

        #region Public API
        public void Subscribe(UpdateEvent callback)
        {
            if (callback == null) return;
            _callbacksToAdd.Add(callback);
        }

        /// <summary>
        /// Unsubscribe a callback from this channel.
        /// </summary>
        public void Unsubscribe(UpdateEvent callback)
        {
            if (callback == null) return;
            _callbacksToRemove.Add(callback);
        }

        /// <summary>
        /// Increment the internal timer by the given amount.
        /// </summary>
        public void TimeIncrement(float deltaTime)
        {
            lock (_syncLock)
            {
                _timeSinceLastUpdate += deltaTime;
            }
        }

        /// <summary>
        /// Check if enough time has passed to invoke callbacks.
        /// </summary>
        public bool ShouldUpdate()
        {
            lock (_syncLock)
            {
                return _timeSinceLastUpdate >= UpdateInterval;
            }
        }

        /// <summary>
        /// Get the accumulated delta time and reset the internal timer.
        /// </summary>
        public float GetAndResetDeltaTime()
        {
            lock (_syncLock)
            {
                float delta = _timeSinceLastUpdate;
                _timeSinceLastUpdate = 0f;
                return delta;
            }
        }

        /// <summary>
        /// Process any pending subscription/unsubscription changes.
        /// </summary>
        public void ProcessPendingSubscriptions()
        {
            ProcessAddedCallbacks();
            ProcessRemovedCallbacks();
        }

        /// <summary>
        /// Invoke all registered callbacks with the given context.
        /// </summary>
        public void Invoke(float deltaTime, UpdateContext context)
        {
            foreach (var callback in _callbacks)
            {
                try
                {
                    callback?.Invoke(deltaTime, context);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in UpdateChannel callback: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        #endregion

        #region Private Helpers

        private void ProcessAddedCallbacks()
        {
            while (_callbacksToAdd.TryTake(out var callback))
            {
                _callbacks.Add(callback);
            }
        }

        private void ProcessRemovedCallbacks()
        {
            if (_callbacksToRemove.Count == 0)
                return;

            var activeCallbacks = new List<UpdateEvent>(_callbacks);

            // Clear the bag
            while (_callbacks.TryTake(out _)) { }

            // Remove requested callbacks
            while (_callbacksToRemove.TryTake(out var toRemove))
            {
                activeCallbacks.Remove(toRemove);
            }

            // Re-add remaining callbacks
            foreach (var callback in activeCallbacks)
            {
                _callbacks.Add(callback);
            }
        }

        #endregion
    }
}