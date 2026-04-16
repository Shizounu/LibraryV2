using System;
using System.Collections.Generic;
using System.Linq;

namespace Shizounu.Library.Dialogue.Data
{
    public static class DialogueBranchResolver
    {
        public static DialogueElement ResolveHighestPriorityEnterable(
            IEnumerable<PriorityIDTuple> branches,
            Func<string, DialogueElement> elementResolver)
        {
            if (branches == null || elementResolver == null)
                return null;

            foreach (PriorityIDTuple branch in branches.OrderByDescending(ctx => ctx.Priority))
            {
                if (branch == null || string.IsNullOrEmpty(branch.ID))
                    continue;

                DialogueElement element = elementResolver(branch.ID);
                if (element != null && element.CanEnter())
                    return element;
            }

            return null;
        }
    }
}