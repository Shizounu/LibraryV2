using UnityEngine;
using Shizounu.Library.GameAI.StateMachine;
using Shizounu.Library.Utility;
using Shizounu.Library.Extensions;
using Shizounu.Library.ScriptableArchitecture;

namespace Shizounu.Library.Examples
{
    /// <summary>
    /// Example demonstrating EventDebugger usage with ScriptableEvents.
    /// </summary>
    public class EventDebuggerExample : MonoBehaviour
    {
        [SerializeField] private ScriptableEvent _onPlayerDamaged;
        [SerializeField] private ScriptableEvent _onEnemySpotted;
        [SerializeField] private ScriptableEvent _onLevelComplete;

        private void Start()
        {
            // Enable event debugging with stack traces
            EventDebugger.Enable(captureStackTraces: true);
            EventDebugger.SetMaxHistoryPerEvent(50);

            // Simulate events
            if (_onPlayerDamaged != null)
                _onPlayerDamaged.RegisterListener(() => Debug.Log("Player damaged!"));
            if (_onEnemySpotted != null)
                _onEnemySpotted.RegisterListener(() => Debug.Log("Enemy spotted!"));
        }

        private void Update()
        {
            // Simulate events based on input
            if (Input.GetKeyDown(KeyCode.D))
            {
                if (_onPlayerDamaged != null)
                {
                    _onPlayerDamaged.Invoke();
                    _onPlayerDamaged.RecordInvocation();
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (_onEnemySpotted != null)
                {
                    _onEnemySpotted.Invoke();
                    _onEnemySpotted.RecordInvocation();
                }
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                if (_onLevelComplete != null)
                {
                    _onLevelComplete.Invoke();
                    _onLevelComplete.RecordInvocation();
                }
            }

            // Display debug info
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Debug.Log(EventDebugger.GetSummary());
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                var csv = EventDebugger.ExportToCSV();
                Debug.Log("Event history exported:\n" + csv);
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                EventDebugger.ClearHistory();
                Debug.Log("Event history cleared");
            }
        }
    }
}
