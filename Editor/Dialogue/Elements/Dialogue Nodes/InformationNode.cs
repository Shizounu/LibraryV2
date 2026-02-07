using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Linq;
using Shizounu.Library.Dialogue.Data;
using Shizounu.Library.ScriptableArchitecture;
using Shizounu.Library.Editor.DialogueEditor.Windows;
using Shizounu.Library.Editor.DialogueEditor.Utilities;

namespace Shizounu.Library.Editor.DialogueEditor.Elements
{
    public class InformationNode : BaseNode
    {
        public Dialogue.Data.DialogueBlackboard Blackboard;
        public string FactKey;
        public ComparisonOperator ConditionOperator;
        public IntReference Value;

        public override void Initialize(Vector2 position, DialogueGraphView graphView)
        {
            base.Initialize(position, graphView);
            SlideName = "Information";
        }
        protected override void MakeOutput()
        {
            foreach (var item in BranchPorts)
            {
                outputContainer.Add(item.port);
            }
        }

        protected override void MakeExtension()
        {
            Button addPrioPort = ElementUtility.CreateButton("Add Priority", () => CreatePriorityPort(0));
            extensionContainer.Add(addPrioPort);
            VisualElement factKeyContainer = new VisualElement();
            extensionContainer.Add(factKeyContainer);

            void RefreshFactKeyUI()
            {
                factKeyContainer.Clear();

                List<string> keys = Blackboard != null
                    ? Blackboard.GetAllKeys().OrderBy(key => key).ToList()
                    : new List<string>();

                if (keys.Count > 0)
                {
                    if (string.IsNullOrWhiteSpace(FactKey) || !keys.Contains(FactKey))
                        FactKey = keys[0];

                    PopupField<string> keyDropdown = new PopupField<string>("Fact Key", keys, FactKey);
                    keyDropdown.RegisterValueChangedCallback(evt => FactKey = evt.newValue);
                    factKeyContainer.Add(keyDropdown);
                }

                TextField factKeyField = ElementUtility.CreateTextField(FactKey, "Fact Key (Manual)", ctx => FactKey = ctx.newValue);
                factKeyContainer.Add(factKeyField);
            }

            ObjectField blackboardField = ElementUtility.CreateSOField<Dialogue.Data.DialogueBlackboard>(
                "Blackboard",
                Blackboard,
                ctx =>
                {
                    Blackboard = (Dialogue.Data.DialogueBlackboard)ctx.newValue;
                    RefreshFactKeyUI();
                });
            extensionContainer.Add(blackboardField);

            RefreshFactKeyUI();
            extensionContainer.Add(ElementUtility.CreateEnumField<ComparisonOperator>(ConditionOperator, "Operator", ctx => ConditionOperator = (ComparisonOperator)ctx.newValue));
            
            // Initialize Value if null
            if (Value == null)
                Value = new IntReference();
            
            extensionContainer.Add(ElementUtility.CreateIntReferenceField(Value, "Value"));
        }

        public override DialogueElement GetElement()
        {
            return new Information()
            {
                ID = UID,
                Blackboard = this.Blackboard,
                FactKey = this.FactKey,
                Operator = this.ConditionOperator,
                Value = this.Value,
                NodePosition = this.GetPosition()
            };
        }

        public override void LoadData(DialogueElement element)
        {
            Blackboard = ((Information)element).Blackboard;
            FactKey = ((Information)element).FactKey;
            ConditionOperator = ((Information)element).Operator;
            Value = ((Information)element).Value;
        }

        public override string GetSearchText()
        {
            string blackboardName = Blackboard != null ? Blackboard.name : string.Empty;
            return $"{SlideName} {blackboardName} {FactKey} {ConditionOperator}";
        }
    }
}

