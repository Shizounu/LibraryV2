using System.Collections.Generic;
using UnityEngine;

using Shizounu.Library.Dialogue.Data;

namespace Shizounu.Library.Editor.DialogueEditor.Utilities
{
    /// <summary>
    /// Template asset that stores a subset of dialogue nodes and connections.
    /// </summary>
    public class DialogueTemplate : ScriptableObject
    {
        [SerializeReference] public List<DialogueElement> Elements = new List<DialogueElement>();
        public List<GroupData> groupDatas = new List<GroupData>();
        public Vector2 AnchorPosition;
    }
}
