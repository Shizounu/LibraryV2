using System;
using System.Collections.Generic;

namespace Shizounu.Library.GameAI.StateMachine {

	public class StateDefinition {
		public string Name { get; }

		private readonly List<IAction> enterActions = new();
		private readonly List<IAction> updateActions = new();
		private readonly List<IAction> exitActions = new();
		private readonly List<TransitionDefinition> transitions = new();

		public StateDefinition(string name) {
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("State name cannot be null or empty", nameof(name));
			Name = name;
		}

		public StateDefinition AddEnterAction(IAction action) {
			if (action == null) throw new ArgumentNullException(nameof(action));
			enterActions.Add(action);
			return this;
		}

		public StateDefinition AddUpdateAction(IAction action) {
			if (action == null) throw new ArgumentNullException(nameof(action));
			updateActions.Add(action);
			return this;
		}

		public StateDefinition AddExitAction(IAction action) {
			if (action == null) throw new ArgumentNullException(nameof(action));
			exitActions.Add(action);
			return this;
		}

		public StateDefinition AddTransition(IDecision decision, StateDefinition targetState, bool invertDecision = false) {
			if (decision == null) throw new ArgumentNullException(nameof(decision));
			if (targetState == null) throw new ArgumentNullException(nameof(targetState));
			transitions.Add(new TransitionDefinition(decision, targetState, invertDecision));
			return this;
		}

		public void OnEnter(StateMachineContext context) {
			for (int i = 0; i < enterActions.Count; i++) {
				enterActions[i].Execute(context);
			}
		}

		public void OnUpdate(StateMachineContext context) {
			for (int i = 0; i < updateActions.Count; i++) {
				updateActions[i].Execute(context);
			}

			for (int i = 0; i < transitions.Count; i++) {
				var transition = transitions[i];
				bool decisionSucceeded = transition.Decision.Decide(context);
				if (decisionSucceeded != transition.InvertDecision) {
					context.StateMachine.ActiveState = transition.TargetState;
					return;
				}
			}
		}

		public void OnExit(StateMachineContext context) {
			for (int i = 0; i < exitActions.Count; i++) {
				exitActions[i].Execute(context);
			}
		}
	}

	/// <summary>
	/// Defines a transition between states with a decision and optional inversion.
	/// </summary>
	public struct TransitionDefinition {
		public IDecision Decision { get; }
		public StateDefinition TargetState { get; }
		public bool InvertDecision { get; }

		public TransitionDefinition(IDecision decision, StateDefinition targetState, bool invertDecision = false) {
			Decision = decision ?? throw new ArgumentNullException(nameof(decision));
			TargetState = targetState ?? throw new ArgumentNullException(nameof(targetState));
			InvertDecision = invertDecision;
		}
	}
}
