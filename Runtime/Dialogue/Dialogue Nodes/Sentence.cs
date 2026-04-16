using UnityEngine;

namespace Shizounu.Library.Dialogue.Data
{
    public class Sentence : DialogueElement
    {
        public Speaker Speaker;
        public string Text;
        public bool UseLocalization;
        public string LocalizationKey;
        

        public override bool CanEnter()
        {
            return true; 
        }

        public override void OnEnter(DialogueContext context)
        {
            context.ShowSentence(this);
        }

        public string GetDisplayText()
        {
            return UseLocalization ? LocalizationKey : Text;
        }
    }
}
