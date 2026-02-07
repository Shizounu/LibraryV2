using UnityEngine;

namespace Shizounu.Library.GameAI.StateMachine.Commons {
	public class CacheValueAction<T> : IAction {
		private readonly string key;
		private readonly T value;

		public CacheValueAction(string key, T value) {
			this.key = key;
			this.value = value;
		}

		public void Execute(StateMachineContext context) {
			context.Blackboard.SetValue(key, value);
		}
	}

	public partial class CommonActions {
		public static IAction CacheValue<T>(string key, T value) => 
			new CacheValueAction<T>(key, value);
	}
}
