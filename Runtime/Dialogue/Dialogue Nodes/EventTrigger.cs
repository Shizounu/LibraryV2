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

        public override void OnEnter(DialogueContext context)
        {
            if (scriptableEvent == null)
            {
                Debug.LogWarning($"EventTrigger node '{ID}' is missing a ScriptableEvent reference.");
                context.CompleteCurrentStep();
                return;
            }

            scriptableEvent.Invoke();
            context.CompleteCurrentStep();
        }
    }
}
