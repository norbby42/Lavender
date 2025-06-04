using Mono.Cecil.Cil;
using PixelCrushers.DialogueSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lavender.DialogueLib
{
    public class ConversationUtils
    {
        // Convenience function to find DialogueEntry's that are responses to the provided entry
        public static IEnumerable<DialogueEntry> GetResponsesTo(Conversation conversation, DialogueEntry dialogueEntry)
        {
            return dialogueEntry.outgoingLinks.Where((Link l) =>
            {
                return l.originConversationID == conversation.id && l.destinationConversationID == conversation.id && l.originDialogueID == dialogueEntry.id;
            }).Select((Link l) =>
            {
                return conversation.GetDialogueEntry(l.destinationDialogueID);
            });
        }

        // Convenience function to find only STATIC DialogueEntry's that are responses to the provided entry
        // A STATIC DialogueEntry does not have any condition set and contains no Lua - ie it is static unchanging text and is always available
        public static IEnumerable<DialogueEntry> GetStaticResponsesTo(Conversation conversation, DialogueEntry dialogueEntry)
        {
            return dialogueEntry.outgoingLinks.Where((Link l) =>
            {
                if (l.originConversationID == conversation.id && l.destinationConversationID == conversation.id && l.originDialogueID == dialogueEntry.id)
                {
                    DialogueEntry target = conversation.GetDialogueEntry(l.destinationDialogueID);
                    if (target.conditionsString != "" || target.currentDialogueText.Contains("[lua("))
                    {
                        return false;
                    }

                    return true;
                }

                return false;

            }).Select((Link l) =>
            {
                return conversation.GetDialogueEntry(l.destinationDialogueID);
            });
        }

        // Find the soonest DialogueEntry(s) starting at the provided entry that we can add a player response to
        // If there are somehow multiple (f.ex branching dialogue) then we will only return the ones with a matching node distance
        public static IEnumerable<DialogueEntry> AdvanceToRespondable(Conversation conversation, DialogueEntry entry)
        {
            List<DialogueEntry> visited = new List<DialogueEntry>();
            List<DialogueEntry> results = new List<DialogueEntry>();

            Stack<DialogueEntry> pendingCurrentDepth = new Stack<DialogueEntry>();
            Stack<DialogueEntry> pendingNextDepth = new Stack<DialogueEntry>();

            pendingCurrentDepth.Push(entry);

            while (pendingCurrentDepth.Any())
            {
                entry = pendingCurrentDepth.Pop();
                visited.Add(entry);

                bool advance = false;
                // If there is an NPC response to our current entry, then the dialogue system will automatically take it and there will be no chance for the player to speak
                if (DoResponsesIncludeNPC(conversation, entry))
                {
                    advance = true;
                }
                // If any response is a sequenced node, then we cannot respond in this node because execution will fast-track to the sequence
                if (GetResponsesTo(conversation, entry).Except(visited).Where((DialogueEntry de) => de.Sequence != null && de.Sequence.Length != 0).Any())
                {
                    advance = true;
                }

                if (advance && results.Count == 0)
                {
                    foreach (DialogueEntry de in GetResponsesTo(conversation, entry).Except(visited))
                    {
                        pendingNextDepth.Push(de);
                    }

                    // If we have exhausted all options at our current depth, then time to start looking at the next depth
                    if (!pendingCurrentDepth.Any())
                    {
                        pendingCurrentDepth = pendingNextDepth;
                        pendingNextDepth = new Stack<DialogueEntry>();
                    }
                }
                else
                {
                    results.Add(entry);
                }
            }
            
            return results;
        }

        // Check if any of the responses to the supplied dialogueentry are spoken by an NPC
        public static bool DoResponsesIncludeNPC(Conversation conversation, DialogueEntry entry)
        {
            foreach (var l in entry.outgoingLinks)
            {
                DialogueEntry de = conversation.GetDialogueEntry(l.destinationDialogueID);
                if (de != null && de.ActorID != 1)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
