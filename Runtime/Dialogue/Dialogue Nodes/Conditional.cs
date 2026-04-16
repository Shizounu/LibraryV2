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
            if (Blackboard == null)
            {
                Debug.LogWarning($"Conditional node '{ID}' is missing a blackboard reference.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(FactKey))
            {
                Debug.LogWarning($"Conditional node '{ID}' is missing a fact key.");
                return false;
            }

            if (!Blackboard.HasKey(FactKey))
            {
                Debug.LogWarning($"Key ({FactKey}) is not present in Blackboard {Blackboard.name}");
                return false;
            }

            if (Value == null)
            {
                Debug.LogWarning($"Conditional node '{ID}' is missing a comparison value reference.");
                return false;
            }

            if (!Blackboard.TryGetValue(FactKey, out int factValue))
            {
                Debug.LogWarning($"Key ({FactKey}) in Blackboard {Blackboard.name} is not an int value.");
                return false;
            }

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
    
        public override void OnEnter(DialogueContext context) { context.CompleteCurrentStep(); }
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
