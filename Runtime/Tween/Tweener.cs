using System;
using System.Collections.Generic;
using UnityEngine;

using Shizounu.Library.UpdateSystem;
using Shizounu.Library.Utility;

namespace Shizounu.Library.Tweening
{
    /// <summary>
    /// Global manager for all tweens. Runs asynchronously using the UpdateSystem.
    /// Handles tween lifecycle, updates, and cleanup.
    /// </summary>
    public class Tweener : Singleton<Tweener>
    {
        #region Constants

        private const float UPDATE_INTERVAL = 0f; // Update every frame
        private const UpdateThreading THREADING_MODE = UpdateThreading.MainThread; // Tweens should update on main thread for safety

        #endregion

        #region Fields

        private int _nextTweenId = 0;
        private readonly List<Tween> _activeTweens = new();
        private UpdateEvent _updateCallback;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the Tweener and registers with the UpdateSystem.
        /// </summary>
        public Tweener() : base()
        {
            RegisterWithUpdateSystem();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Registers this tweener with the UpdateSystem.
        /// </summary>
        private void RegisterWithUpdateSystem()
        {
            _updateCallback = OnUpdate;
            Shizounu.Library.UpdateSystem.UpdateSystem.Instance.RegisterCallback(_updateCallback, UPDATE_INTERVAL, THREADING_MODE);
        }

        #endregion

        #region Creating Tweens

        /// <summary>
        /// Creates a new tween with the specified duration.
        /// </summary>
        public static Tween CreateTween(float duration)
        {
            var tweener = Instance;
            int id = tweener._nextTweenId++;
            Tween tween = new Tween(id, duration);
            tweener._activeTweens.Add(tween);
            return tween;
        }

        /// <summary>
        /// Creates a new tween builder for fluent configuration.
        /// </summary>
        public static TweenBuilder Create(float duration)
        {
            return new TweenBuilder(duration);
        }

        #endregion

        #region Tween Management

        /// <summary>
        /// Gets the number of currently active tweens.
        /// </summary>
        public static int ActiveTweenCount => Instance._activeTweens.Count;

        /// <summary>
        /// Kills all active tweens.
        /// </summary>
        public static void KillAll()
        {
            var tweener = Instance;
            foreach (var tween in tweener._activeTweens)
            {
                tween.Kill();
            }
            tweener._activeTweens.Clear();
        }

        /// <summary>
        /// Pauses all active tweens.
        /// </summary>
        public static void PauseAll()
        {
            foreach (var tween in Instance._activeTweens)
            {
                if (tween.IsPlaying)
                {
                    tween.Pause();
                }
            }
        }

        /// <summary>
        /// Resumes all paused tweens.
        /// </summary>
        public static void ResumeAll()
        {
            foreach (var tween in Instance._activeTweens)
            {
                if (tween.IsPaused)
                {
                    tween.Resume();
                }
            }
        }

        /// <summary>
        /// Gets a tween by its ID.
        /// </summary>
        public static Tween GetTweenById(int id)
        {
            return Instance._activeTweens.Find(t => t.Id == id);
        }

        #endregion

        #region Update Loop

        /// <summary>
        /// Called by UpdateSystem every frame to update all active tweens.
        /// </summary>
        private void OnUpdate(float deltaTime, UpdateContext context)
        {
            // Update all active tweens and remove completed/killed ones
            for (int i = _activeTweens.Count - 1; i >= 0; i--)
            {
                if (!_activeTweens[i].Update(deltaTime))
                {
                    _activeTweens.RemoveAt(i);
                }
            }
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Cleans up the tweener and unregisters from UpdateSystem.
        /// </summary>
        public void Cleanup()
        {
            if (_updateCallback != null)
            {
                Shizounu.Library.UpdateSystem.UpdateSystem.Instance.UnregisterCallback(_updateCallback, UPDATE_INTERVAL, THREADING_MODE);
            }
            KillAll();
        }

        #endregion
    }
}
