using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using Shizounu.Library.Dialogue.Data;
using Shizounu.Library.Editor.DialogueEditor.Windows;
using Shizounu.Library.Editor.DialogueEditor.Utilities;

namespace Shizounu.Library.Editor.DialogueEditor.Elements
{
    [System.Serializable]
    public class SentenceNode : BaseNode
    {
        public string Text;
        public Speaker Speaker;
        public override void Initialize(Vector2 position, DialogueGraphView graphView)
        {
            base.Initialize(position, graphView);
            SlideName = "Sentence";
            Text = "";
        }
        protected override void MakeOutput()
        {
            foreach (var item in BranchPorts)
                outputContainer.Add(item.port);
        }

        protected override void MakeExtension()
        {
            Button addPrioPort = ElementUtility.CreateButton("Add Priority", () => CreatePriorityPort(0));
            extensionContainer.Add(addPrioPort);

            //Extension container
            VisualElement customDataContainer = new();
            customDataContainer.AddToClassList("ds-node__custom-data-container");
            Foldout textFoldout = ElementUtility.CreateFoldout("Sentence Text");
            ObjectField speakerField = ElementUtility.CreateSOField<Speaker>("Speaker", Speaker, ctx => Speaker = (Speaker)ctx.newValue);
            TextField textTextField = ElementUtility.CreateTextArea(Text, null, ctx => Text = ctx.newValue);

            textTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__quote-text-field"
            );

            textFoldout.Add(speakerField);
            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);
        }

        public override DialogueElement GetElement()
        {
            return new Sentence()
            {
                ID = this.UID,
                Text = this.Text,
                Speaker = this.Speaker,
                NodePosition = this.GetPosition()
            };
        }

        public override void LoadData(DialogueElement element)
        {
            Text = ((Sentence)element).Text;
            Speaker = ((Sentence)element).Speaker;
        }
    }
}
