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
        [SerializeField] private bool _nodeHasCompleted;
        [SerializeField] private bool _canContinue;

        public static DialogueData ActiveDialogue { get; private set; }
        public static DialogueElement ActiveElement { get; private set; }

        public static event Action<DialogueData> DialogueStarted;
        public static event Action DialogueEnded;
        public static event Action<DialogueElement> ElementEntered;
        
        public bool NodeHasCompleted 
        { 
            get => _nodeHasCompleted; 
            set => _nodeHasCompleted = value; 
        }
        
        public bool CanContinue 
        { 
            get => _canContinue; 
            set => _canContinue = value; 
        }

        private void OnContinue()
        {
            if (!CanContinue)
                return;
            NodeHasCompleted = true;
        }

        public abstract void EnableDialogueControl();
        public abstract void DisableDialogueControl();

        public void DoDialogue(DialogueData dialogue)
        {
            ActiveDialogue = dialogue;
            DialogueStarted?.Invoke(dialogue);
            StartCoroutine(DialogueLoop(dialogue));
        }
        private IEnumerator DialogueLoop(DialogueData dialogue) 
        {
            DialogueElement element = dialogue.GetStartingElement();
            while (element != null) 
            {
                NodeHasCompleted = false;
                _currentElement = element;
                ActiveElement = element;
                ElementEntered?.Invoke(element);
                element.OnEnter(this);

                while (!NodeHasCompleted)
                    yield return new WaitForEndOfFrame();

                element = element.GetNextElement(dialogue);
            }
            //Closing Dialogue
            NodeHasCompleted = false;
            CanContinue = true; 
            while (!NodeHasCompleted)
                yield return new WaitForEndOfFrame();
            DisableDialogueControl();

            ActiveElement = null;
            ActiveDialogue = null;
            DialogueEnded?.Invoke();
        }


        public abstract void ShowSentence(Sentence sentence);

        public abstract IEnumerator WriteText(string text, Speaker speaker);
    }
}
