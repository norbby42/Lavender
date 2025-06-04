using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace Lavender.DialogueLib
{
    public static class DialogueInteractableTalkPatches
    {
        private static Dictionary<InteractableTalk, string> StartedDialogue = new Dictionary<InteractableTalk, string>();

        [HarmonyPatch(typeof(InteractableTalk), nameof(InteractableTalk.OnDialogueStart))]
        [HarmonyPrefix]
        static bool InteractableTalk_OnDialogueStart_Prefix(InteractableTalk __instance, string dialogue, bool tryStartConversation)
        {
            string validatedDialogue = dialogue;
            if (dialogue == null || dialogue.Length == 0)
            {
                // Dialogue is not being passed in, and must be extracted from the ConversationStarter component.
                validatedDialogue = __instance.conversationTrigger.conversation;
            }

            StartedDialogue.Remove(__instance);
            StartedDialogue.Add(__instance, validatedDialogue);

            IEnumerable<ConversationPatcher> patchers = ConversationPatchesManager.Instance.GetPatchersForConversation(validatedDialogue);
            foreach (var patcher in patchers)
            {
                patcher.OnConversationStarted(__instance);
            }

            LavenderLog.DialogueVerbose(validatedDialogue, $"Conversation starting: \"{validatedDialogue}\".  Notified {patchers.Count()} patchers..");

            return true;
        }

        [HarmonyPatch(typeof(InteractableTalk), nameof(InteractableTalk.OnDialogueEnded))]
        [HarmonyPrefix]
        static bool InteractableTalk_OnDialogueEnded_Prefix(InteractableTalk __instance)
        {
            if (StartedDialogue.TryGetValue(__instance, out var dialogue))
            {
                StartedDialogue.Remove(__instance);

                IEnumerable<ConversationPatcher> patchers = ConversationPatchesManager.Instance.GetPatchersForConversation(dialogue);
                foreach (var patcher in patchers)
                {
                    patcher.OnConversationEnded(__instance);
                }

                LavenderLog.DialogueVerbose(dialogue, $"Conversation ended: \"{dialogue}\".  Notified {patchers.Count()} patchers..");
            }
            else
            {
                // Removed because it's honestly quite spammy and is not actually indicative of an issue
                //LavenderLog.DialogueVerbose($"Unknown conversation ended. " + 
                //    "This means OnConversationEnded will not be fired for some patchers, maybe! " +
                //    "Or maybe it was already cause OnDialogueEnded is called multiple times!");
            }

            return true;
        }
    }
}
