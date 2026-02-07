using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using Shizounu.Library.Dialogue.Data;
using Shizounu.Library.Editor.DialogueEditor.Utilities;

namespace Shizounu.Library.Editor.DialogueEditor.Elements
{
    public class EntryNode : BaseNode
    {
        protected override void MakeInput()
        {
            //No Entry as this is the entry into the graph
        }
        protected override void MakeOutput()
        {
            foreach (var item in BranchPorts) {
                outputContainer.Add(item.port);
            }
        }
        protected override void MakeTitle()
        {
            TextField titleLabel = ElementUtility.CreateTextField("Entry");
            titleLabel.isReadOnly = true;

            titleLabel.AddClasses(
                "ds-node__text-field",
                "ds-node__text-field__hidden",
                "ds-node__filename-text-field"
            );
            titleContainer.Add(titleLabel);

            
        }
        protected override void MakeExtension()
        {
            Button addPrioPort = ElementUtility.CreateButton("Add Priority", () => CreatePriorityPort(0));
            extensionContainer.Add(addPrioPort);
        }

        public override DialogueElement GetElement()
        {
            return null;
        }

        public override void LoadData(DialogueElement element)
        {
            throw new System.NotImplementedException();
        }
    }


}
