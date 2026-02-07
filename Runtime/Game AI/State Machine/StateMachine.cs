using System;
using UnityEngine;
using Shizounu.Library.GameAI;

namespace Shizounu.Library.GameAI.StateMachine {

	public class StateMachine : MonoBehaviour 
	{
		private StateDefinition _activeState;
		private Blackboard _blackboard;
		private StateMachineContext _context;

		public StateDefinition ActiveState 
		{
			get => _activeState;
			set 
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));
				if (value == _activeState)
					return;

				_activeState.OnExit(_context);
				_activeState = value;
				_activeState.OnEnter(_context);
			}
		}

		public Blackboard Blackboard => _blackboard;

		private void Awake() 
		{
			_blackboard = new SimpleBlackboard();
			_context = new StateMachineContext(this, _blackboard);
		}

		/// <summary>
		/// Initialize the state machine with a starting state.
		/// </summary>
		public void Initialize(StateDefinition startState) 
		{
			if (startState == null)
				throw new ArgumentNullException(nameof(startState));
			
			if (_blackboard == null) 
			{
				_blackboard = new SimpleBlackboard();
				_context = new StateMachineContext(this, _blackboard);
			}

			_activeState = startState;
			_activeState.OnEnter(_context);
		}

		protected virtual void Update() 
		{
			DoTick();
		}

		public void DoTick() 
		{
			_activeState?.OnUpdate(_context);
		}

		private void OnDestroy() 
		{
			_blackboard?.Clear();
			_blackboard = null;
			_context = null;
		}
	}
}
