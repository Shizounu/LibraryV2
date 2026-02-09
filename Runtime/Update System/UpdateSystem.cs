using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

using Shizounu.Library.Utility;

namespace Shizounu.Library.UpdateSystem
{
    /// <summary>
    /// Multithreaded update system managing callbacks with different update intervals and threading modes.
    /// Automatically integrates with Unity's PlayerLoop without requiring a MonoBehaviour.
    /// </summary>
    public class UpdateSystem : Singleton<UpdateSystem>
    {
        #region Constants
        private const float BACKGROUND_TARGET_FRAME_TIME = 1f / 60f;
        private const int SHUTDOWN_TIMEOUT_MS = 1000;
        #endregion

        #region Fields

        private readonly ConcurrentDictionary<float, UpdateChannel> _mainThreadChannels;
        private readonly ConcurrentDictionary<float, UpdateChannel> _backgroundChannels;
        private readonly ConcurrentDictionary<float, UpdateChannel> _jobSystemChannels;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _backgroundUpdateTask;
        private bool _isRunning = false;
        private bool _isInjectedIntoPlayerLoop = false;
        private bool _isPaused = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the UpdateSystem and injects it into Unity's PlayerLoop.
        /// </summary>
        public UpdateSystem() : base()
        {
            _mainThreadChannels = new ConcurrentDictionary<float, UpdateChannel>();
            _backgroundChannels = new ConcurrentDictionary<float, UpdateChannel>();
            _jobSystemChannels = new ConcurrentDictionary<float, UpdateChannel>();

            InjectIntoPlayerLoop();
        }

        #endregion

        #region PlayerLoop Injection

        /// <summary>
        /// Injects the update system into Unity's PlayerLoop for automatic updates.
        /// </summary>
        private void InjectIntoPlayerLoop()
        {
            if (_isInjectedIntoPlayerLoop)
                return;

            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            // Inject into Update cycle
            if (!IsSystemAlreadyInjected(ref playerLoop, typeof(Tracker_UpdateSystemUpdate)))
            {
                playerLoop = InsertSystem<Update>(
                    playerLoop,
                    typeof(Tracker_UpdateSystemUpdate),
                    UpdateSystemUpdate,
                    0);
            }

            // Inject into FixedUpdate cycle
            if (!IsSystemAlreadyInjected(ref playerLoop, typeof(Tracker_UpdateSystemFixedUpdate)))
            {
                playerLoop = InsertSystem<FixedUpdate>(
                    playerLoop,
                    typeof(Tracker_UpdateSystemFixedUpdate),
                    UpdateSystemFixedUpdate,
                    0);
            }

            PlayerLoop.SetPlayerLoop(playerLoop);
            _isInjectedIntoPlayerLoop = true;

            StartSystem();
            Application.quitting += OnApplicationQuit;
        }

        /// <summary>
        /// Removes the update system from Unity's PlayerLoop.
        /// </summary>
        private void RemoveFromPlayerLoop()
        {
            if (!_isInjectedIntoPlayerLoop)
                return;

            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            playerLoop = RemoveSystem<Update>(playerLoop, typeof(Tracker_UpdateSystemUpdate));
            playerLoop = RemoveSystem<FixedUpdate>(playerLoop, typeof(Tracker_UpdateSystemFixedUpdate));

            PlayerLoop.SetPlayerLoop(playerLoop);
            _isInjectedIntoPlayerLoop = false;

            Application.quitting -= OnApplicationQuit;
        }

        #endregion

        #region Public API
        /// <summary>
        /// Register a callback to be invoked at the specified interval.
        /// </summary>
        /// <param name="updateEvent">The callback to invoke.</param>
        /// <param name="updateInterval">How often to invoke the callback (in seconds).</param>
        /// <param name="threading">Which thread to execute the callback on.</param>
        public void RegisterCallback(
            UpdateEvent updateEvent,
            float updateInterval,
            UpdateThreading threading = UpdateThreading.MainThread)
        {
            if (updateEvent == null)
                throw new ArgumentNullException(nameof(updateEvent));

            var channelDict = GetChannelDictionary(threading);
            var channel = channelDict.GetOrAdd(
                updateInterval,
                _ => new UpdateChannel(updateInterval, threading));
            channel.Subscribe(updateEvent);
        }

        /// <summary>
        /// Unregister a callback.
        /// </summary>
        public void UnregisterCallback(
            UpdateEvent updateEvent,
            float updateInterval,
            UpdateThreading threading = UpdateThreading.MainThread)
        {
            if (updateEvent == null)
                return;

            var channelDict = GetChannelDictionary(threading);
            if (channelDict.TryGetValue(updateInterval, out var channel))
            {
                channel.Unsubscribe(updateEvent);
            }
        }

        /// <summary>
        /// Start the background update thread.
        /// </summary>
        public void StartSystem()
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            _backgroundUpdateTask = Task.Run(() => BackgroundUpdateLoop(_cancellationTokenSource.Token));
        }

        /// <summary>
        /// Stop the background update thread gracefully.
        /// </summary>
        public void StopSystem()
        {
            if (!_isRunning)
                return;

            _isRunning = false;
            _cancellationTokenSource?.Cancel();
            _backgroundUpdateTask?.Wait(SHUTDOWN_TIMEOUT_MS);
        }

        /// <summary>
        /// Pause all update callbacks across all threads.
        /// </summary>
        public void PauseSystem()
        {
            _isPaused = true;
        }

        /// <summary>
        /// Resume all update callbacks across all threads.
        /// </summary>
        public void ResumeSystem()
        {
            _isPaused = false;
        }

        /// <summary>
        /// Get the pause state of the update system.
        /// </summary>
        public bool IsPaused => _isPaused;

        #endregion

        #region Internal Updates

        /// <summary>
        /// Process main thread updates (called automatically via PlayerLoop).
        /// </summary>
        private void ProcessMainThreadUpdates(float deltaTime)
        {
            if (_isPaused)
                return;

            ProcessChannelUpdates(_mainThreadChannels.Values, deltaTime, isMainThread: true);
        }

        /// <summary>
        /// Process job system updates (called automatically via PlayerLoop).
        /// </summary>
        private void ProcessJobSystemUpdates(float deltaTime)
        {
            if (_isPaused)
                return;

            ProcessChannelUpdates(_jobSystemChannels.Values, deltaTime, isMainThread: true);
        }

        /// <summary>
        /// Main loop for background thread updates.
        /// </summary>
        private void BackgroundUpdateLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var startTime = DateTime.UtcNow;

                if (!_isPaused)
                {
                    ProcessChannelUpdates(
                        _backgroundChannels.Values,
                        BACKGROUND_TARGET_FRAME_TIME,
                        isMainThread: false);
                }

                var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;
                var sleepTime = BACKGROUND_TARGET_FRAME_TIME - elapsed;

                if (sleepTime > 0)
                {
                    Thread.Sleep((int)(sleepTime * 1000));
                }
            }
        }

        /// <summary>
        /// Process all channels in the given collection.
        /// </summary>
        private void ProcessChannelUpdates(
            IEnumerable<UpdateChannel> channels,
            float deltaTime,
            bool isMainThread)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;

            foreach (var channel in channels)
            {
                channel.ProcessPendingSubscriptions();
                channel.TimeIncrement(deltaTime);

                if (!channel.ShouldUpdate())
                    continue;

                float actualDelta = channel.GetAndResetDeltaTime();
                var context = new UpdateContext(actualDelta, threadId, isMainThread);
                channel.Invoke(actualDelta, context);
            }
        }

        #endregion

        #region Helpers
        /// <summary>
        /// Get the appropriate channel dictionary for the given threading mode.
        /// </summary>
        private ConcurrentDictionary<float, UpdateChannel> GetChannelDictionary(UpdateThreading threading)
        {
            return threading switch
            {
                UpdateThreading.MainThread => _mainThreadChannels,
                UpdateThreading.Background => _backgroundChannels,
                UpdateThreading.JobSystem => _jobSystemChannels,
                _ => _mainThreadChannels
            };
        }

        private void OnApplicationQuit()
        {
            StopSystem();
            RemoveFromPlayerLoop();
        }

        ~UpdateSystem()
        {
            StopSystem();
        }
        #endregion

        #region PlayerLoop Integration
        private struct Tracker_UpdateSystemUpdate { }
        private struct Tracker_UpdateSystemFixedUpdate { }

        private static void UpdateSystemUpdate()
        {
            Instance.ProcessMainThreadUpdates(Time.deltaTime);
        }

        private static void UpdateSystemFixedUpdate()
        {
            Instance.ProcessJobSystemUpdates(Time.fixedDeltaTime);
        }

        /// <summary>
        /// Inserts a system into the appropriate position in the PlayerLoop.
        /// </summary>
        private static PlayerLoopSystem InsertSystem<T>(PlayerLoopSystem playerLoop,Type systemType, PlayerLoopSystem.UpdateFunction updateFunction, int index)
        {
            if (playerLoop.type == typeof(T))
            {
                var newSystem = new PlayerLoopSystem
                {
                    type = systemType,
                    updateDelegate = updateFunction
                };

                var newSubSystemList = new List<PlayerLoopSystem>();
                if (playerLoop.subSystemList != null)
                {
                    newSubSystemList.AddRange(playerLoop.subSystemList);
                }

                index = Mathf.Clamp(index, 0, newSubSystemList.Count);
                newSubSystemList.Insert(index, newSystem);

                playerLoop.subSystemList = newSubSystemList.ToArray();
                return playerLoop;
            }

            if (playerLoop.subSystemList != null)
            {
                for (int i = 0; i < playerLoop.subSystemList.Length; i++)
                {
                    playerLoop.subSystemList[i] = InsertSystem<T>(
                        playerLoop.subSystemList[i],
                        systemType,
                        updateFunction,
                        index);
                }
            }

            return playerLoop;
        }

        /// <summary>
        /// Removes a system from the PlayerLoop.
        /// </summary>
        private static PlayerLoopSystem RemoveSystem<T>(PlayerLoopSystem playerLoop, Type systemType)
        {
            if (playerLoop.subSystemList == null)
                return playerLoop;

            var newSubSystemList = new List<PlayerLoopSystem>();

            foreach (var subsystem in playerLoop.subSystemList)
            {
                if (subsystem.type != systemType)
                {
                    newSubSystemList.Add(RemoveSystem<T>(subsystem, systemType));
                }
            }

            playerLoop.subSystemList = newSubSystemList.ToArray();
            return playerLoop;
        }

        /// <summary>
        /// Checks if a system is already injected into the PlayerLoop.
        /// </summary>
        private static bool IsSystemAlreadyInjected(ref PlayerLoopSystem playerLoop, Type systemType)
        {
            if (playerLoop.subSystemList == null)
                return false;

            for (int i = 0; i < playerLoop.subSystemList.Length; i++)
            {
                if (playerLoop.subSystemList[i].type == systemType)
                    return true;

                if (IsSystemAlreadyInjected(ref playerLoop.subSystemList[i], systemType))
                    return true;
            }

            return false;
        }

        #endregion
    }
}