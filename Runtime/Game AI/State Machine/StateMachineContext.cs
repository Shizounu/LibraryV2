using UnityEngine;
using Shizounu.Library.GameAI;

namespace Shizounu.Library.GameAI.StateMachine {
	/// <summary>
	/// Context passed to actions and decisions, providing access to the state machine and blackboard.
	/// </summary>
	public class StateMachineContext {
		public StateMachine StateMachine { get; }
		public Blackboard Blackboard { get; }

		public StateMachineContext(StateMachine stateMachine, Blackboard blackboard) {
			StateMachine = stateMachine;
			Blackboard = blackboard;
		}
	}
}
