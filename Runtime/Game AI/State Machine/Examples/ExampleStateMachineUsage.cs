using UnityEngine;

namespace Shizounu.Library.GameAI.StateMachine.Examples
{
    /// <summary>
	/// Example showing how to use the state machine with blackboard.
	/// </summary>
	public class ExampleStateMachineUsage : MonoBehaviour {
		private StateMachine stateMachine;

		private void Awake() {
			stateMachine = gameObject.AddComponent<StateMachine>();

			// Create states with fluent API
			var idleState = new StateDefinition("Idle")
				.AddEnterAction(CommonActions.Log("Entering Idle"))
				.AddUpdateAction(CommonActions.IncrementCounter("idleTime"))
				.AddExitAction(CommonActions.Log("Exiting Idle"));

			var activeState = new StateDefinition("Active")
				.AddEnterAction(CommonActions.Log("Entering Active"))
				.AddUpdateAction(CommonActions.Log("Active state running"))
				.AddExitAction(CommonActions.Log("Exiting Active"));

			// Add transitions using blackboard data
			idleState.AddTransition(
				CommonDecisions.CounterAbove("idleTime", 100),
				activeState
			);

			activeState.AddTransition(
				CommonDecisions.AlwaysTrue,
				idleState
			);

			// Optional: Register states for reuse
			StateFactory.RegisterState(idleState);
			StateFactory.RegisterState(activeState);

			// Initialize the state machine
			stateMachine.Initialize(idleState);

			// Access blackboard directly
			stateMachine.Blackboard.SetValue("gameStartTime", Time.time);
		}

		private void OnGUI() {
			if (GUI.Button(new Rect(10, 10, 150, 30), "Get Idle Time")) {
				int idleTime = stateMachine.Blackboard.GetValue<int>("idleTime");
				Debug.Log($"Idle time: {idleTime}");
			}

			if (GUI.Button(new Rect(10, 50, 150, 30), "Clear Blackboard")) {
				stateMachine.Blackboard.Clear();
			}
		}
	}
}