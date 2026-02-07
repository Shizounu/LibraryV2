using UnityEngine;
using UnityEngine.UIElements;
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
            extensionContainer.Add(ElementUtility.CreateSOField<Dialogue.Data.DialogueBlackboard>("Blackboard", Blackboard, ctx => Blackboard = (Dialogue.Data.DialogueBlackboard)ctx.newValue));
            extensionContainer.Add(ElementUtility.CreateTextField(FactKey, "Fact Key", ctx => FactKey = ctx.newValue));
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
    }
}

