using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Linq;

namespace Shizounu.Library.Dialogue.Data
{
    public class DialogueData : ScriptableObject
    {
        [SerializeReference] public List<PriorityIDTuple> EntryElements = new(); 
        [SerializeReference] public List<DialogueElement> Elements = new();

        public DialogueElement GetElement(string ID) {
            return Elements.Find(ctx => ctx.ID == ID);
        }
        public static string GetID() {
            return GUID.Generate().ToString();
        }

        public DialogueElement GetStartingElement()
        {
            return DialogueBranchResolver.ResolveHighestPriorityEnterable(EntryElements, GetElement);
        }

#if UNITY_EDITOR
        public void Clear()
        {
            EntryElements.Clear();
            Elements.Clear();
            groupDatas.Clear();
        }
        public List<GroupData> groupDatas = new List<GroupData>();
#endif
    }

    [Serializable]
    public class PriorityIDTuple {
        public PriorityIDTuple(int prio, string ID) {
            this.Priority = prio;
            this.ID = ID;
        }
        public int Priority;
        public string ID; 
    }

#if UNITY_EDITOR
    [Serializable] 
    public class GroupData {
        public string Title;
        public List<string> NodeIDs;
        public Rect position;
    }
#endif
}
