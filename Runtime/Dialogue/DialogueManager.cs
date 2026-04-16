using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using Shizounu.Library.Dialogue.Data;
using Shizounu.Library.Utility;

namespace Shizounu.Library.Dialogue
{
    public abstract class DialogueManager : SingletonBehaviour<DialogueManager>
    {
        [Header("Dialogue")]
        [SerializeField] private DialogueElement _currentElement;
        [SerializeField] private DialogueExecutionState _executionState;
        [SerializeField, Min(0f)] private float _nodeCompletionTimeoutSeconds = 10f;
        [SerializeField] private bool _useNodeCompletionTimeout = true;

        private Coroutine _activeDialogueCoroutine;

        public static DialogueContext ActiveContext { get; private set; }
        public static DialogueData ActiveDialogue => ActiveContext?.ActiveDialogue;
        public static DialogueElement ActiveElement => ActiveContext?.ActiveElement;

        public static event Action<DialogueData> DialogueStarted;
        public static event Action DialogueEnded;
        public static event Action DialogueCancelled;
        public static event Action<DialogueElement> ElementEntered;
        public static event Action<DialogueElement> ElementExited;
        public static event Action<DialogueExecutionState> ExecutionStateChanged;

        public DialogueExecutionState ExecutionState => _executionState;
        
        public bool NodeHasCompleted 
        { 
            get => _executionState == DialogueExecutionState.ReadyToAdvance;
            set
            {
                if (value)
                {
                    CompleteCurrentStep();
                    return;
                }

                BeginNodeExecution();
            }
        }
        
        public bool CanContinue 
        { 
            get => _executionState == DialogueExecutionState.WaitingForContinue;
            set
            {
                if (value)
                {
                    BeginContinuePrompt();
                    return;
                }

                if (_executionState == DialogueExecutionState.WaitingForContinue)
                    SetExecutionState(DialogueExecutionState.Idle);
            }
        }

        public void ContinueDialogue()
        {
            if (!CanContinue)
                return;

            CompleteCurrentStep();
        }

        public abstract void EnableDialogueControl();
        public abstract void DisableDialogueControl();

        private void EnsureContext()
        {
            if (ActiveContext != null && ActiveContext.Manager == this)
                return;

            ActiveContext = new DialogueContext(this);
        }

        public void DoDialogue(DialogueData dialogue)
        {
            if (dialogue == null)
            {
                Debug.LogWarning("Cannot start dialogue because DialogueData is null.");
                return;
            }

            if (_activeDialogueCoroutine != null)
                StopDialogue();

            EnsureContext();
            ActiveContext.SetActiveDialogue(dialogue);
            EnableDialogueControl();
            DialogueStarted?.Invoke(dialogue);
            _activeDialogueCoroutine = StartCoroutine(DialogueLoop(dialogue));
        }

        public void StopDialogue()
        {
            if (ActiveContext == null || ActiveContext.Manager != this)
                return;

            DialogueElement activeElement = ActiveContext.ActiveElement;
            if (activeElement != null)
            {
                activeElement.OnCancelled(ActiveContext);
                ElementExited?.Invoke(activeElement);
            }

            if (_activeDialogueCoroutine != null)
            {
                StopCoroutine(_activeDialogueCoroutine);
                _activeDialogueCoroutine = null;
            }

            SetExecutionState(DialogueExecutionState.Cancelled);
            FinalizeDialogue(cancelled: true);
        }

        private IEnumerator DialogueLoop(DialogueData dialogue) 
        {
            EnsureContext();
            DialogueElement element = dialogue.GetStartingElement();
            while (element != null) 
            {
                BeginNodeExecution();
                _currentElement = element;
                ActiveContext.SetActiveElement(element);
                ElementEntered?.Invoke(element);
                element.OnEnter(ActiveContext);

                yield return WaitForNodeCompletion(element);

                if (ExecutionState == DialogueExecutionState.Cancelled)
                    yield break;

                element.OnExit(ActiveContext);
                ElementExited?.Invoke(element);
                ActiveContext.SetActiveElement(null);
                element = element.GetNextElement(dialogue);
            }
            //Closing Dialogue
            BeginContinuePrompt();
            while (ExecutionState == DialogueExecutionState.WaitingForContinue)
                yield return new WaitForEndOfFrame();

            if (ExecutionState == DialogueExecutionState.Cancelled)
                yield break;

            FinalizeDialogue(cancelled: false);
        }

        public void BeginNodeExecution()
        {
            SetExecutionState(DialogueExecutionState.RunningNode);
        }

        public void BeginContinuePrompt()
        {
            SetExecutionState(DialogueExecutionState.WaitingForContinue);
        }

        public void CompleteCurrentStep()
        {
            SetExecutionState(DialogueExecutionState.ReadyToAdvance);
        }

        protected void SetExecutionState(DialogueExecutionState nextState)
        {
            if (_executionState == nextState)
                return;

            _executionState = nextState;
            ExecutionStateChanged?.Invoke(_executionState);
        }

        private IEnumerator WaitForNodeCompletion(DialogueElement element)
        {
            float elapsed = 0f;

            while (ExecutionState == DialogueExecutionState.RunningNode)
            {
                if (_useNodeCompletionTimeout)
                {
                    elapsed += Time.unscaledDeltaTime;
                    if (elapsed >= _nodeCompletionTimeoutSeconds)
                    {
                        Debug.LogWarning($"Dialogue node '{element?.ID}' timed out after {_nodeCompletionTimeoutSeconds:0.##}s. Forcing completion.");
                        CompleteCurrentStep();
                        yield break;
                    }
                }

                yield return null;
            }
        }

        private void FinalizeDialogue(bool cancelled)
        {
            if (_activeDialogueCoroutine != null)
            {
                StopCoroutine(_activeDialogueCoroutine);
                _activeDialogueCoroutine = null;
            }

            DisableDialogueControl();

            _currentElement = null;
            ActiveContext?.Clear();

            if (cancelled)
                DialogueCancelled?.Invoke();

            SetExecutionState(DialogueExecutionState.Idle);
            DialogueEnded?.Invoke();
        }


        public abstract void ShowSentence(Sentence sentence);

        public abstract void ShowChoice(Choice choice);

        public abstract IEnumerator WriteText(string text, Speaker speaker);
    }
}
