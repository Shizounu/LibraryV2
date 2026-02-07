namespace Shizounu.Library.GameAI.StateMachine.Examples {
	/// <summary>
	/// Example decision that checks if a counter exceeds a threshold.
	/// Uses the blackboard to retrieve the cached counter value.
	/// </summary>
	public struct CounterThresholdDecision : IDecision {
		private readonly string counterKey;
		private readonly int threshold;

		public CounterThresholdDecision(string counterKey, int threshold) {
			this.counterKey = counterKey;
			this.threshold = threshold;
		}

		public bool Decide(StateMachineContext context) {
			int count = context.Blackboard.GetValue<int>(counterKey);
			return count >= threshold;
		}
	}

	public partial class CommonDecisions {
		public static IDecision CounterAbove(string key, int threshold) => 
			new CounterThresholdDecision(key, threshold);
	}
}
