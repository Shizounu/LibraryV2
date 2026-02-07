using UnityEngine;

namespace Shizounu.Library.GameAI.StateMachine.Examples {
	public class LogAndCacheAction : IAction {
		private readonly string message;
		private readonly string cacheKey;

		public LogAndCacheAction(string message, string cacheKey = null) {
			this.message = message;
			this.cacheKey = cacheKey ?? "last_log";
		}

		public void Execute(StateMachineContext context) {
			Debug.Log(message);
			context.Blackboard.SetValue(cacheKey, message);
		}
	}

	public partial class CommonActions {
		public static IAction Log(string message) => 
			new LogAndCacheAction(message);
	}
}
