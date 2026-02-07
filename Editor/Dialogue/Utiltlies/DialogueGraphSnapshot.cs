using System.Collections.Generic;
using UnityEngine;

using Shizounu.Library.Dialogue.Data;

namespace Shizounu.Library.Editor.DialogueEditor.Utilities
{
    /// <summary>
    /// Serializable snapshot of a dialogue graph for undo/redo.
    /// </summary>
    public class DialogueGraphSnapshot : ScriptableObject
    {
        [SerializeReference] public List<PriorityIDTuple> EntryElements = new List<PriorityIDTuple>();
        [SerializeReference] public List<DialogueElement> Elements = new List<DialogueElement>();
        public List<GroupData> groupDatas = new List<GroupData>();

        public void Clear()
        {
            EntryElements.Clear();
            Elements.Clear();
            groupDatas.Clear();
        }
    }
}
