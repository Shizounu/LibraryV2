using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Shizounu.Library.Dialogue.Data;
using Shizounu.Library.Utility;

namespace Shizounu.Library.Dialogue
{
    public abstract class DialogueManager : SingletonBehaviour<DialogueManager>
    {
        [Header("Dialogue")]
        [SerializeField] private DialogueElement currentElement;
        [SerializeField] public bool NodeHasCompleted;
        [SerializeField] public bool CanContinue;

        private void OnContinue()
        {
            if (!CanContinue)
                return;
            NodeHasCompleted = true;
        }

        public abstract void EnableDialogueControl();
        public abstract void DisableDialogueControl();

        public void DoDialogue(DialogueData dialogue) => StartCoroutine(DialogueLoop(dialogue));
        private IEnumerator DialogueLoop(DialogueData dialogue) {
            DialogueElement element = dialogue.GetStartingElement();
            while (element != null) {
                NodeHasCompleted = false;
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
        }


        public abstract void ShowSentence(Sentence sentence);

        public abstract IEnumerator WriteText(string text, Speaker speaker);
    }
}
