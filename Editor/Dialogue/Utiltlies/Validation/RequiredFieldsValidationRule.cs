using System.Collections.Generic;
using Shizounu.Library.Editor.DialogueEditor.Elements;
using Shizounu.Library.Editor.DialogueEditor.Windows;
using Shizounu.Library.Dialogue.Data;

namespace Shizounu.Library.Editor.DialogueEditor.Utilities
{
    public sealed class RequiredFieldsValidationRule : IDialogueValidationRule
    {
        public int Order => 500;

        public void Validate(DialogueGraphView graphView, List<ValidationIssue> issues)
        {
            foreach (BaseNode node in graphView.NodeCache.Values)
            {
                if (node is SentenceNode sentence)
                {
                    if (string.IsNullOrWhiteSpace(sentence.Text) && !sentence.UseLocalization)
                    {
                        issues.Add(new ValidationIssue
                        {
                            Severity = ValidationSeverity.Info,
                            Message = "Sentence node has empty text.",
                            Node = node
                        });
                    }

                    if (sentence.UseLocalization && string.IsNullOrWhiteSpace(sentence.LocalizationKey))
                    {
                        issues.Add(new ValidationIssue
                        {
                            Severity = ValidationSeverity.Warning,
                            Message = "Localization is enabled but key is empty.",
                            Node = node
                        });
                    }
                }
                else if (node is ChoiceNode choice)
                {
                    if (choice.Options == null || choice.Options.Count == 0)
                    {
                        issues.Add(new ValidationIssue
                        {
                            Severity = ValidationSeverity.Error,
                            Message = "Choice node has no options.",
                            Node = node
                        });
                    }

                    if (string.IsNullOrWhiteSpace(choice.Prompt))
                    {
                        issues.Add(new ValidationIssue
                        {
                            Severity = ValidationSeverity.Info,
                            Message = "Choice node has empty prompt text.",
                            Node = node
                        });
                    }

                    foreach (ChoiceOption option in choice.Options ?? new System.Collections.Generic.List<ChoiceOption>())
                    {
                        if (option != null && string.IsNullOrWhiteSpace(option.Text))
                        {
                            issues.Add(new ValidationIssue
                            {
                                Severity = ValidationSeverity.Warning,
                                Message = $"Choice option at priority {option.Priority} has empty text.",
                                Node = node
                            });
                        }
                    }
                }
                else if (node is EventTriggerNode eventTrigger && eventTrigger.scriptableEvent == null)
                {
                    issues.Add(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Warning,
                        Message = "Event Trigger has no ScriptableEvent assigned.",
                        Node = node
                    });
                }
            }
        }
    }
}
