using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Linq;

using Shizounu.Library.Dialogue.Data;
using Shizounu.Library.ScriptableArchitecture;
using Shizounu.Library.Editor.DialogueEditor.Windows;
using Shizounu.Library.Editor.DialogueEditor.Utilities;

namespace Shizounu.Library.Editor.DialogueEditor.Elements
{
    public class ConditionalNode : BaseNode
    {
        public DialogueBlackboard Blackboard;
        public string FactKey; 
        public ConditionOperator ConditionOperator;
        public IntReference Value;
        public override void Initialize(Vector2 position, DialogueGraphView graphView)
        {
            base.Initialize(position, graphView);
            SlideName = "Conditional";
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
            extensionContainer.Add(ElementUtility.CreateEnumField<ConditionOperator>(ConditionOperator, "Operator", ctx => ConditionOperator = (ConditionOperator)ctx.newValue));
            
            // Initialize Value if null
            if (Value == null)
                Value = new IntReference();
            
            extensionContainer.Add(ElementUtility.CreateIntReferenceField(Value, "Value"));
        }

        public override DialogueElement GetElement()
        {
            return new Conditional()
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
            Blackboard = ((Conditional)element).Blackboard;
            FactKey = ((Conditional)element).FactKey;
            ConditionOperator = ((Conditional)element).Operator;
            Value = ((Conditional)element).Value;
        }

        public override string GetSearchText()
        {
            string blackboardName = Blackboard != null ? Blackboard.name : string.Empty;
            return $"{SlideName} {blackboardName} {FactKey} {ConditionOperator}";
        }
    }

}
