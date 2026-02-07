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
        public bool UseLocalization;
        public string LocalizationKey;
        public override void Initialize(Vector2 position, DialogueGraphView graphView)
        {
            base.Initialize(position, graphView);
            SlideName = "Sentence";
            Text = "";
            LocalizationKey = string.Empty;
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

            Toggle localizationToggle = new Toggle("Use Localization")
            {
                value = UseLocalization
            };
            localizationToggle.RegisterValueChangedCallback(evt => UseLocalization = evt.newValue);

            TextField localizationKeyField = ElementUtility.CreateTextField(
                LocalizationKey,
                "Localization Key",
                ctx => LocalizationKey = ctx.newValue);

            textTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__quote-text-field"
            );

            textFoldout.Add(speakerField);
            textFoldout.Add(localizationToggle);
            textFoldout.Add(localizationKeyField);
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
                UseLocalization = this.UseLocalization,
                LocalizationKey = this.LocalizationKey,
                NodePosition = this.GetPosition()
            };
        }

        public override void LoadData(DialogueElement element)
        {
            Text = ((Sentence)element).Text;
            Speaker = ((Sentence)element).Speaker;
            UseLocalization = ((Sentence)element).UseLocalization;
            LocalizationKey = ((Sentence)element).LocalizationKey;
        }

        public override string GetSearchText()
        {
            string speakerName = Speaker != null ? Speaker.name : string.Empty;
            return $"{SlideName} {Text} {speakerName} {LocalizationKey}";
        }
    }
}
