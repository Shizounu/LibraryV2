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

        public override void OnEnter(DialogueContext context)
        {
            if (Blackboard == null)
            {
                Debug.LogWarning($"Information node '{ID}' is missing a blackboard reference.");
                context.CompleteCurrentStep();
                return;
            }

            if (string.IsNullOrWhiteSpace(FactKey))
            {
                Debug.LogWarning($"Information node '{ID}' is missing a fact key.");
                context.CompleteCurrentStep();
                return;
            }

            if (!Blackboard.HasKey(FactKey))
            {
                Debug.LogWarning($"Key ({FactKey}) is not present in Blackboard {Blackboard.name}");
                context.CompleteCurrentStep();
                return;
            }

            if (Value == null)
            {
                Debug.LogWarning($"Information node '{ID}' is missing a value reference.");
                context.CompleteCurrentStep();
                return;
            }

            if (!Blackboard.TryGetValue(FactKey, out int currentValue))
            {
                Debug.LogWarning($"Key ({FactKey}) in Blackboard {Blackboard.name} is not an int value.");
                context.CompleteCurrentStep();
                return;
            }

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
            context.CompleteCurrentStep();
        }
    }

    public enum ComparisonOperator
    {
        Addition,
        Subtraction,
        Set
    }
}
