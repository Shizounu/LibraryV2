using UnityEngine;

namespace Shizounu.Library.GameAI.StateMachine.Examples {
	public class IncrementCounterAction : IAction {
		private readonly string counterKey;

		public IncrementCounterAction(string counterKey = "counter") {
			this.counterKey = counterKey;
		}

		public void Execute(StateMachineContext context) {
			int count = context.Blackboard.GetValue<int>(counterKey);
			context.Blackboard.SetValue(counterKey, count + 1);
		}
	}

	public partial class CommonActions {
		public static IAction IncrementCounter(string key = "counter") => 
			new IncrementCounterAction(key);
	}
}
