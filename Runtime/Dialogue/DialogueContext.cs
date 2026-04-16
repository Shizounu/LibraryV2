using System.Collections;
using Shizounu.Library.Dialogue.Data;

namespace Shizounu.Library.Dialogue
{
    public sealed class DialogueContext
    {
        public DialogueContext(DialogueManager manager)
        {
            Manager = manager;
        }

        public DialogueManager Manager { get; }
        public DialogueData ActiveDialogue { get; private set; }
        public DialogueElement ActiveElement { get; private set; }
        public DialogueExecutionState ExecutionState => Manager.ExecutionState;

        internal void SetActiveDialogue(DialogueData dialogue)
        {
            ActiveDialogue = dialogue;
        }

        internal void SetActiveElement(DialogueElement element)
        {
            ActiveElement = element;
        }

        internal void Clear()
        {
            ActiveDialogue = null;
            ActiveElement = null;
        }

        public void BeginNodeExecution()
        {
            Manager.BeginNodeExecution();
        }

        public void BeginContinuePrompt()
        {
            Manager.BeginContinuePrompt();
        }

        public void CompleteCurrentStep()
        {
            Manager.CompleteCurrentStep();
        }

        public void StopDialogue()
        {
            Manager.StopDialogue();
        }

        public void ShowSentence(Sentence sentence)
        {
            Manager.ShowSentence(sentence);
        }

        public void ShowChoice(Choice choice)
        {
            Manager.ShowChoice(choice);
        }

        public IEnumerator WriteText(string text, Speaker speaker)
        {
            return Manager.WriteText(text, speaker);
        }
    }
}