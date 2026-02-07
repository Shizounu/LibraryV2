namespace Shizounu.Library.GameAI.StateMachine.Examples {
	/// <summary>
	/// Example decision that always returns the same value.
	/// </summary>
	public struct ConstantDecision : IDecision {
		private readonly bool value;

		public ConstantDecision(bool value) {
			this.value = value;
		}

		public bool Decide(StateMachineContext context) {
			return value;
		}
	}

	public partial class CommonDecisions {
		public static IDecision AlwaysTrue => new ConstantDecision(true);
		public static IDecision AlwaysFalse => new ConstantDecision(false);
	}
}
