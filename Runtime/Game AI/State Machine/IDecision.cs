using Shizounu.Library.GameAI;

namespace Shizounu.Library.GameAI.StateMachine {
	public interface IDecision {
		bool Decide(StateMachineContext context);
	}
}
