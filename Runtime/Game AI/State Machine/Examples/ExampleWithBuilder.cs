using UnityEngine;

namespace Shizounu.Library.GameAI.StateMachine.Examples {
	

	public class ExampleWithBuilder : MonoBehaviour {
		private void Awake() {
			var stateMachine = gameObject.AddComponent<StateMachine>();

			// Create and configure states
			var patrolState = new StateDefinition("Patrol")
				.AddEnterAction(CommonActions.Log("Starting patrol"))
				.AddUpdateAction(new PatrolAction())
				.AddExitAction(CommonActions.Log("Stopped patrolling"));

			var chaseState = new StateDefinition("Chase")
				.AddEnterAction(CommonActions.Log("Chasing target"))
				.AddUpdateAction(new ChaseAction())
				.AddExitAction(CommonActions.Log("Lost target"));

			// Set up transitions
			patrolState.AddTransition(new EnemyDetectedDecision(), chaseState);
			chaseState.AddTransition(new EnemyLostDecision(), patrolState);

			stateMachine.Initialize(patrolState);
		}

		private class PatrolAction : IAction {
			public void Execute(StateMachineContext context) {
				// Patrol logic here
				context.Blackboard.SetValue("lastPatrolTime", UnityEngine.Time.time);
			}
		}

		private class ChaseAction : IAction {
			public void Execute(StateMachineContext context) {
				// Chase logic here
				context.Blackboard.SetValue("chaseStartTime", UnityEngine.Time.time);
			}
		}

		private struct EnemyDetectedDecision : IDecision {
			public bool Decide(StateMachineContext context) {
				// Check detection logic
				return false;
			}
		}

		private struct EnemyLostDecision : IDecision {
			public bool Decide(StateMachineContext context) {
				// Check if enemy is lost
				return false;
			}
		}
	}
}
