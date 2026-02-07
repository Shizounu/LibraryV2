using Shizounu.Library.GameAI;

namespace Shizounu.Library.GameAI.StateMachine {
	public interface IAction {
		void Execute(StateMachineContext context);
	}
}
