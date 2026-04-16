using System.Collections.Generic;
using Shizounu.Library.Editor.DialogueEditor.Windows;

namespace Shizounu.Library.Editor.DialogueEditor.Utilities
{
    public interface IDialogueValidationRule
    {
        int Order { get; }
        void Validate(DialogueGraphView graphView, List<ValidationIssue> issues);
    }
}
