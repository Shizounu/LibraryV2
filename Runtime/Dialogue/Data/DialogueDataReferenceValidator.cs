using System.Collections.Generic;
using System.Linq;

namespace Shizounu.Library.Dialogue.Data
{
    public sealed class DialogueDataReferenceIssue
    {
        public string OwnerId;
        public string Message;
    }

    public static class DialogueDataReferenceValidator
    {
        public static List<string> Validate(DialogueData dialogue)
        {
            return ValidateDetailed(dialogue)
                .Select(issue => issue.Message)
                .Distinct()
                .ToList();
        }

        public static List<DialogueDataReferenceIssue> ValidateDetailed(DialogueData dialogue)
        {
            List<DialogueDataReferenceIssue> issues = new List<DialogueDataReferenceIssue>();
            if (dialogue == null)
            {
                issues.Add(new DialogueDataReferenceIssue
                {
                    Message = "Dialogue data is null."
                });
                return issues;
            }

            Dictionary<string, DialogueElement> elementsById = new Dictionary<string, DialogueElement>();

            foreach (DialogueElement element in dialogue.Elements)
            {
                if (element == null)
                {
                    issues.Add(new DialogueDataReferenceIssue
                    {
                        Message = "Dialogue contains a null element."
                    });
                    continue;
                }

                if (string.IsNullOrWhiteSpace(element.ID))
                {
                    issues.Add(new DialogueDataReferenceIssue
                    {
                        Message = $"Dialogue contains a {element.GetType().Name} node with an empty ID."
                    });
                    continue;
                }

                if (!elementsById.TryAdd(element.ID, element))
                {
                    issues.Add(new DialogueDataReferenceIssue
                    {
                        OwnerId = element.ID,
                        Message = $"Duplicate dialogue element ID found: {element.ID}"
                    });
                }
            }

            ValidateTargets(dialogue.EntryElements, elementsById, issues, "Entry");

            foreach (DialogueElement element in elementsById.Values)
                ValidateTargets(element.Branches, elementsById, issues, element.ID);

            return issues
                .GroupBy(issue => $"{issue.OwnerId}|{issue.Message}")
                .Select(group => group.First())
                .ToList();
        }

        private static void ValidateTargets(
            IEnumerable<PriorityIDTuple> branches,
            IReadOnlyDictionary<string, DialogueElement> elementsById,
            List<DialogueDataReferenceIssue> issues,
            string ownerId)
        {
            if (branches == null)
                return;

            foreach (PriorityIDTuple branch in branches)
            {
                if (branch == null)
                {
                    issues.Add(new DialogueDataReferenceIssue
                    {
                        OwnerId = ownerId,
                        Message = $"{ownerId} contains a null branch reference."
                    });
                    continue;
                }

                if (string.IsNullOrWhiteSpace(branch.ID))
                {
                    issues.Add(new DialogueDataReferenceIssue
                    {
                        OwnerId = ownerId,
                        Message = $"{ownerId} contains a branch with an empty target ID."
                    });
                    continue;
                }

                if (!elementsById.ContainsKey(branch.ID))
                {
                    issues.Add(new DialogueDataReferenceIssue
                    {
                        OwnerId = ownerId,
                        Message = $"{ownerId} references missing target node '{branch.ID}'."
                    });
                }
            }
        }
    }
}