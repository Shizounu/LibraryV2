using System;
using System.Collections.Generic;

namespace Shizounu.Library.GameAI.StateMachine {
	/// <summary>
	/// Factory for creating and managing reusable state definitions.
	/// </summary>
	public static class StateFactory {
		private static readonly Dictionary<string, StateDefinition> registeredStates = 
			new Dictionary<string, StateDefinition>();

		public static void RegisterState(StateDefinition state) {
			if (state == null) throw new ArgumentNullException(nameof(state));
			registeredStates[state.Name] = state;
		}

		public static StateDefinition GetState(string name) {
			return registeredStates.TryGetValue(name, out var state) ? state : null;
		}


		public static bool HasState(string name) {
			return registeredStates.ContainsKey(name);
		}

		public static bool UnregisterState(string name) {
			return registeredStates.Remove(name);
		}

		public static void ClearAll() {
			registeredStates.Clear();
		}
	}
}
