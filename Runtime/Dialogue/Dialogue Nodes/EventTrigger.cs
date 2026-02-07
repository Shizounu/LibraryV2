using UnityEngine;
using Shizounu.Library.ScriptableArchitecture;

namespace Shizounu.Library.Dialogue.Data
{
    public class EventTrigger : DialogueElement
    {
        public ScriptableEvent scriptableEvent;         
        public override bool CanEnter() {
            return true; 
        }

        public override void OnEnter(DialogueManager manager)
        {
            scriptableEvent.Invoke();
        }
    }
}
