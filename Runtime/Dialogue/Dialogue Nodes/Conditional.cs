using UnityEngine;
using Shizounu.Library.ScriptableArchitecture;

namespace Shizounu.Library.Dialogue.Data
{
    public class Conditional : DialogueElement {
        public DialogueBlackboard Blackboard;
        public string FactKey;
        public ConditionOperator Operator;
        public IntReference Value;
        public override bool CanEnter()
        {
            if (!Blackboard.HasKey(FactKey))
            {
                Debug.LogError($"Key ({FactKey}) is not present in Blackboard {Blackboard.name}");
                throw new System.Exception($"Key ({FactKey}) is not present in Blackboard {Blackboard.name}");
            }

            int factValue = Blackboard.GetValue<int>(FactKey);
            return Operator switch
            {
                ConditionOperator.Greater => factValue > Value,
                ConditionOperator.GreaterOrEqual => factValue >= Value,
                ConditionOperator.Equal => factValue == Value,
                ConditionOperator.NotEqual => factValue != Value,
                ConditionOperator.LessOrEqual => factValue <= Value,
                ConditionOperator.Less => factValue < Value,
                _ => throw new System.NotImplementedException(),
            };
        }
    
        public override void OnEnter(DialogueManager manager) { manager.NodeHasCompleted = true; }
    }

    public enum ConditionOperator
    {
        Greater,
        GreaterOrEqual,
        Equal,
        NotEqual,
        LessOrEqual,
        Less
    }
}
