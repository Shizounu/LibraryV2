using UnityEngine;
using UnityEngine.InputSystem;

namespace Shizounu.Library.Timer.Example
{
    /// <summary>
    /// Example demonstrating how to use the TimerSystem for timers and cooldowns.
    /// </summary>
    public class TimerSystemExample : MonoBehaviour
    {
        [SerializeField]
        private float _cooldownSeconds = 2f;

        [SerializeField]
        private float _loopSeconds = 1f;

        [SerializeField]
        private float _delayedSeconds = 3f;

        [SerializeField]
        private bool _useUnscaledTime = false;

        private TimerHandle _loopHandle;
        private TimerHandle _delayedHandle;

        private const string ActionCooldownKey = "example_action";

        private void OnEnable()
        {
            var mode = _useUnscaledTime ? TimerUpdateMode.UnscaledTime : TimerUpdateMode.ScaledTime;

            _loopHandle = TimerSystem.Instance.StartTimer(
                _loopSeconds,
                OnLoopTick,
                updateMode: mode,
                repeatCount: -1);

            _delayedHandle = TimerSystem.Instance.StartTimer(
                _delayedSeconds,
                OnDelayedComplete,
                updateMode: mode);
        }

        private void OnDisable()
        {
            TimerSystem.Instance.CancelTimer(_loopHandle);
            TimerSystem.Instance.CancelTimer(_delayedHandle);
        }

        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                TryUseAction();
            }

            if(Keyboard.current.rKey.wasPressedThisFrame)
            {
                RestartLoop();
            }
        }

        private void TryUseAction()
        {
            if (!TimerSystem.Instance.IsCooldownReady(ActionCooldownKey))
            {
                if (TimerSystem.Instance.TryGetCooldownRemaining(ActionCooldownKey, out float remaining))
                {
                    Debug.Log($"Cooldown not ready. Remaining: {remaining:F2}s");
                }
                return;
            }

            Debug.Log("Action triggered! Starting cooldown.");
            TimerSystem.Instance.StartCooldown(ActionCooldownKey, _cooldownSeconds, OnCooldownReady);
        }

        private void RestartLoop()
        {
            TimerSystem.Instance.RestartTimer(_loopHandle, _loopSeconds);
            Debug.Log("Loop timer restarted.");
        }

        private void OnLoopTick()
        {
            Debug.Log("Loop tick.");
        }

        private void OnDelayedComplete()
        {
            Debug.Log("Delayed timer completed.");
        }

        private void OnCooldownReady()
        {
            Debug.Log("Cooldown ready.");
        }
    }
}
