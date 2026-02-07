namespace Shizounu.Library.GameAI.StateMachine.Examples {
	/// <summary>
	/// Example decision that checks if a value is cached on the blackboard.
	/// </summary>
	public struct HasKeyDecision : IDecision {
		private readonly string key;

		public HasKeyDecision(string key) {
			this.key = key;
		}

		public bool Decide(StateMachineContext context) {
			return context.Blackboard.HasKey(key);
		}
	}

	public partial class CommonDecisions {
		public static IDecision HasKey(string key) => 
			new HasKeyDecision(key);
	}
}
