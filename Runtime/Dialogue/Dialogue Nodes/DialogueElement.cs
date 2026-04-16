using System.Collections.Generic;
using UnityEngine;

namespace Shizounu.Library.Dialogue.Data
{
    [System.Serializable]
    public abstract class DialogueElement {
        public string ID;
        public List<PriorityIDTuple> Branches = new();  
        public abstract bool CanEnter();
        public abstract void OnEnter(DialogueContext context);
        public virtual void OnExit(DialogueContext context) { }
        public virtual void OnCancelled(DialogueContext context) { }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>If null is returned exit the dialogue</returns>
        public virtual DialogueElement GetNextElement(DialogueData dialogue) {
            return DialogueBranchResolver.ResolveHighestPriorityEnterable(Branches, dialogue.GetElement);
        }

#if UNITY_EDITOR
        public Rect NodePosition;
#endif
    }
}
