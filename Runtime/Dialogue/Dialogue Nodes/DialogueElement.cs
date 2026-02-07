using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shizounu.Library.Dialogue.Data
{
    [System.Serializable]
    public abstract class DialogueElement {
        public string ID;
        public List<PriorityIDTuple> Branches = new();  
        public abstract bool CanEnter();
        public abstract void OnEnter(DialogueManager manager);

        /// <summary>
        /// 
        /// </summary>
        /// <returns>If null is returned exit the dialogue</returns>
        public DialogueElement GetNextElement(DialogueData dialogue) {
            List<PriorityIDTuple> copy = new(Branches);
            copy.Sort((a, b) => (a.Priority - b.Priority));

            while (copy.Count > 0) {
                List<PriorityIDTuple> curPrio = GetElementsWithPriority(copy.Max(ctx => ctx.Priority));
                foreach (var cur in curPrio)
                    if (dialogue.GetElement(cur.ID).CanEnter())
                        return dialogue.GetElement(cur.ID);
                    else
                        copy.Remove(cur);
            }


            return null;
        }

        private List<PriorityIDTuple> GetElementsWithPriority(int priority)
        {
            return Branches.Where(ctx => ctx.Priority == priority).ToList();
        }

#if UNITY_EDITOR
        public Rect NodePosition;
#endif
    }
}
