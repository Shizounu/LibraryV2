using UnityEngine.UIElements;
using UnityEngine;

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
            extensionContainer.Add(ElementUtility.CreateSOField<Dialogue.Data.DialogueBlackboard>("Blackboard", Blackboard, ctx => Blackboard = (Dialogue.Data.DialogueBlackboard)ctx.newValue));
            extensionContainer.Add(ElementUtility.CreateTextField(FactKey, "Fact Key", ctx => FactKey = ctx.newValue));
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
    }

}
