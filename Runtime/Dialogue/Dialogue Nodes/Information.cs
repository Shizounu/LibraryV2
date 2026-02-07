using UnityEngine;
using Shizounu.Library.ScriptableArchitecture;

namespace Shizounu.Library.Dialogue.Data
{
    public class Information : DialogueElement
    {
        public DialogueBlackboard Blackboard;
        public ComparisonOperator Operator;
        public string FactKey;
        public IntReference Value;
        public override bool CanEnter()
        {
            return true;
        }

        public override void OnEnter(DialogueManager manager)
        {
            if (!Blackboard.HasKey(FactKey))
            {
                Debug.LogError($"Key ({FactKey}) is not present in Blackboard {Blackboard.name}");
                throw new System.Exception($"Key ({FactKey}) is not present in Blackboard {Blackboard.name}");
            }

            int currentValue = Blackboard.GetValue<int>(FactKey);
            switch (Operator)
            {
                case ComparisonOperator.Addition:
                    Blackboard.SetValue(FactKey, currentValue + (int)Value);
                    break;
                case ComparisonOperator.Subtraction:
                    Blackboard.SetValue(FactKey, currentValue - (int)Value);
                    break;
                case ComparisonOperator.Set:
                    Blackboard.SetValue(FactKey, (int)Value);
                    break;
                default:
                    throw new System.NotImplementedException();
            }
            manager.NodeHasCompleted = true;
        }
    }

    public enum ComparisonOperator
    {
        Addition,
        Subtraction,
        Set
    }
}
