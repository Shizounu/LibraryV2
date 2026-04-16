using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using Shizounu.Library.Dialogue.Data;
using Shizounu.Library.Editor.DialogueEditor.Utilities;
using Shizounu.Library.Editor.DialogueEditor.Windows;

namespace Shizounu.Library.Editor.DialogueEditor.Elements
{
    public class ChoiceNode : BaseNode
    {
        public Speaker Speaker;
        public string Prompt;
        public List<ChoiceOption> Options = new();

        private VisualElement optionsContainer;

        public override void Initialize(Vector2 position, DialogueGraphView graphView)
        {
            base.Initialize(position, graphView);
            SlideName = "Choice";
            Prompt = string.Empty;
            EnsureOptionDefaults();
        }

        public override void Draw()
        {
            RebuildPortsFromOptions();
            base.Draw();
        }

        protected override void MakeOutput()
        {
            foreach (PriorityPort item in BranchPorts)
                outputContainer.Add(item.port);
        }

        protected override void MakeExtension()
        {
            extensionContainer.Add(ElementUtility.CreateButton("Add Choice", AddChoiceOption));

            VisualElement customDataContainer = new VisualElement();
            customDataContainer.AddToClassList("ds-node__custom-data-container");

            Foldout contentFoldout = ElementUtility.CreateFoldout("Choice Content");
            contentFoldout.Add(ElementUtility.CreateSOField<Speaker>("Speaker", Speaker, ctx => Speaker = (Speaker)ctx.newValue));
            contentFoldout.Add(ElementUtility.CreateTextArea(Prompt, "Prompt", ctx => Prompt = ctx.newValue));

            optionsContainer = new VisualElement();
            contentFoldout.Add(optionsContainer);

            customDataContainer.Add(contentFoldout);
            extensionContainer.Add(customDataContainer);

            RefreshOptionsUI();
        }

        public override DialogueElement GetElement()
        {
            return new Choice
            {
                ID = UID,
                Speaker = Speaker,
                Prompt = Prompt,
                Options = Options.Select(option => new ChoiceOption(option.Priority, option.Text)).ToList(),
                NodePosition = GetPosition()
            };
        }

        public override void LoadData(DialogueElement element)
        {
            Choice choice = (Choice)element;
            Speaker = choice.Speaker;
            Prompt = choice.Prompt;
            Options = choice.Options?.Select(option => new ChoiceOption(option.Priority, option.Text)).ToList() ?? new List<ChoiceOption>();
            EnsureOptionDefaults();
        }

        public override string GetSearchText()
        {
            string speakerName = Speaker != null ? Speaker.name : string.Empty;
            string optionText = string.Join(" ", Options.Where(option => option != null).Select(option => option.Text));
            return $"{SlideName} {Prompt} {speakerName} {optionText}";
        }

        protected override void OnPriorityPortRemoved(PriorityPort priorityPort)
        {
            if (priorityPort?.Metadata is ChoiceOption option)
                Options.Remove(option);

            EnsureOptionDefaults();
            RefreshOptionsUI();
        }

        protected override void OnPriorityPortPriorityChanged(PriorityPort priorityPort)
        {
            if (priorityPort?.Metadata is ChoiceOption option)
                option.Priority = priorityPort.priority;

            RefreshOptionsUI();
        }

        private void AddChoiceOption()
        {
            ChoiceOption option = new ChoiceOption(GetNextPriority(), $"Option {Options.Count + 1}");
            Options.Add(option);

            PriorityPort port = CreatePriorityPort(option.Priority);
            port.Metadata = option;

            RefreshOptionsUI();
            graphView.RecordUndo("Add Choice Option");
        }

        private void RemoveChoiceOption(ChoiceOption option)
        {
            PriorityPort port = BranchPorts.FirstOrDefault(current => ReferenceEquals(current.Metadata, option));
            if (port != null)
            {
                RemovePriorityPort(port);
            }
            else
            {
                Options.Remove(option);
                EnsureOptionDefaults();
                RefreshOptionsUI();
            }

            graphView.RecordUndo("Remove Choice Option");
        }

        private void RefreshOptionsUI()
        {
            if (optionsContainer == null)
                return;

            optionsContainer.Clear();

            foreach (ChoiceOption option in Options)
            {
                if (option == null)
                    continue;

                VisualElement row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;

                IntegerField priorityField = ElementUtility.CreateIntField(option.Priority, null, evt =>
                {
                    option.Priority = evt.newValue;
                    PriorityPort port = BranchPorts.FirstOrDefault(current => ReferenceEquals(current.Metadata, option));
                    if (port != null)
                    {
                        port.priority = evt.newValue;
                        port.priorityField?.SetValueWithoutNotify(evt.newValue);
                    }
                });
                priorityField.style.width = 70;

                TextField textField = ElementUtility.CreateTextField(option.Text, null, evt => option.Text = evt.newValue);
                textField.style.flexGrow = 1;

                Button removeButton = ElementUtility.CreateButton("Remove", () => RemoveChoiceOption(option));

                row.Add(priorityField);
                row.Add(textField);
                row.Add(removeButton);
                optionsContainer.Add(row);
            }
        }

        private void EnsureOptionDefaults()
        {
            if (Options == null)
                Options = new List<ChoiceOption>();

            if (Options.Count == 0)
                Options.Add(new ChoiceOption(0, "Option 1"));
        }

        private void RebuildPortsFromOptions()
        {
            EnsureOptionDefaults();

            foreach (PriorityPort branchPort in BranchPorts.ToList())
                RemovePortSilently(branchPort);

            foreach (ChoiceOption option in Options)
            {
                PriorityPort port = CreatePriorityPort(option.Priority);
                port.Metadata = option;
            }
        }

        private void RemovePortSilently(PriorityPort priorityPort)
        {
            if (priorityPort?.port == null)
                return;

            priorityPort.port.RemoveFromHierarchy();
            BranchPorts.Remove(priorityPort);
        }

        private int GetNextPriority()
        {
            return Options.Count == 0 ? 0 : Options.Max(option => option?.Priority ?? 0) + 1;
        }
    }
}