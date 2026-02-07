using UnityEngine.UIElements;
using UnityEngine;

using Shizounu.Library.Dialogue.Data;
using Shizounu.Library.ScriptableArchitecture;
using Shizounu.Library.Editor.DialogueEditor.Windows;
using Shizounu.Library.Editor.DialogueEditor.Utilities;

namespace Shizounu.Library.Editor.DialogueEditor.Elements
{
    public class EventTriggerNode : BaseNode
    {
        public ScriptableEvent scriptableEvent;

        public override void Initialize(Vector2 position, DialogueGraphView graphView)
        {
            base.Initialize(position, graphView);
            SlideName = "Event Trigger";
        }
        protected override void MakeOutput()
        {
            foreach (var item in BranchPorts) {
                outputContainer.Add(item.port);
            }
        }

        protected override void MakeExtension()
        {
            Button addPrioPort = ElementUtility.CreateButton("Add Priority", () => CreatePriorityPort(0));
            extensionContainer.Add(ElementUtility.CreateSOField<ScriptableEvent>("Event", scriptableEvent, ctx => scriptableEvent = (ScriptableEvent)ctx.newValue));
        }

        public override DialogueElement GetElement()
        {
            return new EventTrigger() 
            { 
                ID = this.UID, 
                scriptableEvent = this.scriptableEvent, 
                NodePosition = this.GetPosition() 
            };
        }

        public override void LoadData(DialogueElement element)
        {
            scriptableEvent = ((EventTrigger)element).scriptableEvent;
        }
    }
}
