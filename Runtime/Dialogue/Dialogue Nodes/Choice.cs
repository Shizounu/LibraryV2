using System;
using System.Collections.Generic;
using System.Linq;

namespace Shizounu.Library.Dialogue.Data
{
    [Serializable]
    public sealed class ChoiceOption
    {
        public ChoiceOption() { }

        public ChoiceOption(int priority, string text)
        {
            Priority = priority;
            Text = text;
        }

        public int Priority;
        public string Text;
    }

    public class Choice : DialogueElement
    {
        public Speaker Speaker;
        public string Prompt;
        public List<ChoiceOption> Options = new();

        [NonSerialized] private int? _selectedPriority;

        public override bool CanEnter()
        {
            return true;
        }

        public override void OnEnter(DialogueContext context)
        {
            _selectedPriority = null;
            context.ShowChoice(this);
        }

        public void SelectOption(int priority)
        {
            _selectedPriority = priority;
        }

        public ChoiceOption GetOption(int priority)
        {
            return Options?.FirstOrDefault(option => option != null && option.Priority == priority);
        }

        public override DialogueElement GetNextElement(DialogueData dialogue)
        {
            if (!_selectedPriority.HasValue || dialogue == null)
                return null;

            PriorityIDTuple branch = Branches?.FirstOrDefault(tuple => tuple != null && tuple.Priority == _selectedPriority.Value);
            if (branch == null || string.IsNullOrWhiteSpace(branch.ID))
                return null;

            DialogueElement element = dialogue.GetElement(branch.ID);
            return element != null && element.CanEnter() ? element : null;
        }
    }
}