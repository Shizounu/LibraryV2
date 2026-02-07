using System;
using System.Collections.Generic;

namespace Shizounu.Library.GameAI.StateMachine {
	/// <summary>
	/// Factory for creating and managing reusable state definitions.
	/// </summary>
	public static class StateFactory {
		private static readonly Dictionary<string, StateDefinition> registeredStates = 
			new Dictionary<string, StateDefinition>();

		/// <summary>
		/// Register a state definition for reuse across multiple state machines.
		/// </summary>
		public static void RegisterState(StateDefinition state) {
			if (state == null) throw new ArgumentNullException(nameof(state));
			registeredStates[state.Name] = state;
		}

		/// <summary>
		/// Get a registered state by name.
		/// </summary>
		public static StateDefinition GetState(string name) {
			return registeredStates.TryGetValue(name, out var state) ? state : null;
		}

		/// <summary>
		/// Check if a state is registered.
		/// </summary>
		public static bool HasState(string name) {
			return registeredStates.ContainsKey(name);
		}

		/// <summary>
		/// Unregister a state.
		/// </summary>
		public static bool UnregisterState(string name) {
			return registeredStates.Remove(name);
		}

		/// <summary>
		/// Clear all registered states.
		/// </summary>
		public static void ClearAll() {
			registeredStates.Clear();
		}
	}
}
