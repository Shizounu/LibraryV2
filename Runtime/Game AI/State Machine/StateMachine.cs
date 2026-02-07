using System;
using UnityEngine;
using Shizounu.Library.GameAI;

namespace Shizounu.Library.GameAI.StateMachine {

	public class StateMachine : MonoBehaviour {
		public StateDefinition activeState;
		public Blackboard Blackboard;
		//for passing both as one object to actions and decisions
		private StateMachineContext context;

		public StateDefinition ActiveState {
			get => activeState;
			set {
				if (value == null)
					throw new ArgumentNullException(nameof(value));
				if (value == activeState)
					return;

				activeState.OnExit(context);
				activeState = value;
				activeState.OnEnter(context);
			}
		}



		private void Awake() {
			Blackboard = new SimpleBlackboard();
			context = new StateMachineContext(this, Blackboard);
		}

		/// <summary>
		/// Initialize the state machine with a starting state.
		/// </summary>
		public void Initialize(StateDefinition startState) {
			if (startState == null)
				throw new ArgumentNullException(nameof(startState));
			
			if (Blackboard	 == null) {
				Blackboard = new SimpleBlackboard();
				context = new StateMachineContext(this, Blackboard);
			}

			activeState = startState;
			activeState.OnEnter(context);
		}

		protected virtual void Update() {
			DoTick();
		}

		public void DoTick() {
			activeState?.OnUpdate(context);
		}

		private void OnDestroy() {
			Blackboard?.Clear();
			Blackboard = null;
			context = null;
		}
	}
}
