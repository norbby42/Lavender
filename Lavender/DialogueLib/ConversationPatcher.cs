using HarmonyLib;
using PixelCrushers.DialogueSystem;
using System;
using System.Collections.Generic;

namespace Lavender.DialogueLib
{
    /**
     * Allows patching a single conversation
     * Make a child class and implement the PatchDialogue() function using other functions defined in this class to populate the conversation.
     **/
    public abstract class ConversationPatcher
    {
        private PixelCrushers.DialogueSystem.Conversation _Conversation;
        private string _ConversationName;
        private bool RanPatch = false;
        
        public Conversation Conversation
        {
            get { return _Conversation; }
        }
        public string ConversationName
        {
            get { return _ConversationName; }
        }

        #region Dialogue writing tools
        // Convenience commonly used escape character - it causes the NPC speaker to pause for a second or two before continuing
        public static string Pause = "\\,";
        // Convenience commonly used escape character - simply renders a double-quote
        public static string Quote = "\"";
        // Convenience commonly used Lua function wrapper - wraps {@param text} inside a Lua execution context.
        // Suitable for player-facing text; the dialogue library identifies the execution context and processes/displays the contents instead (ie {@param text}) 
        public static string Lua(string text) { return $"[lua({text})]"; }
        #endregion

        // Construct the ConversationPatcher
        // You must pass in the name of the conversation that this instance will patch.
        public ConversationPatcher(string conversationName)
        {
            _ConversationName = conversationName;

            try
            {
                // The DialogueDatabase may not exist at this moment.  So we catch any exceptions and, for the time being, ignore them.
                DialogueDatabase db = DialogueController.instance.dialogueSystem.databaseManager.masterDatabase;
                _Conversation = db.GetConversation(conversationName);
            }
            catch (Exception)
            {
                _Conversation = null!;
            }


            LavenderLog.DialogueVerbose(conversationName, $"  Created {conversationName} conversation patcher.");
        }

        #region Implementable functions

        // This is where your conversation patching logic lives
        // Implement it in your child class
        protected abstract void PatchDialogue();

        // Implementation stub: Called just before opening the fullscreen dialogue UI
        // If you want to register Lua functions that are ONLY available while this conversation is active, then do so within this function
        public virtual void OnConversationStarted(InteractableTalk interactableTalk) { }

        // Implementation stub: Called when the fullscreen dialogue UI has closed
        // If you have conversation-specific Lua functions, make sure to unregister them here
        public virtual void OnConversationEnded(InteractableTalk interactableTalk) { }

        #endregion

        #region Dialogue Patching

        // Create a new DialogueEntry representing the player saying {@param message}.
        // The entry will do nothing until you {@method Link} to/from it.
        // Once created, you can freely modify any parameters on the entry.
        //  Commonly altered params are: conditionsString, userScript
        protected DialogueEntry PlayerSays(string message)
        {
            DialogueEntry entry = new DialogueEntry()
            {
                fields = [], // Required because there's no default value for the fields list, and it is referenced as though it's initialized by other property's setters
                id = 0, // ID will be fixed up in AddDialogueEntry
                conversationID = Conversation.id,
                ActorID = 1, // Hardcoded player ID
                ConversantID = Conversation.ConversantID,
                DialogueText = message,
                falseConditionAction = "Block"
            };

            AddDialogueEntry(entry);

            return entry;
        }

        // Create a new DialogueEntry representing the NPC saying {@param message}.
        // The entry will do nothing until you {@method Link} to/from it.
        // Once created, you can freely modify any parameters on the entry.
        //  Commonly altered params are: conditionsString, userScript
        protected DialogueEntry NPCSays(string message)
        {
            DialogueEntry entry = new DialogueEntry()
            {
                fields = [], // Required because there's no default value for the fields list, and it is referenced as though it's initialized by other property's setters
                id = 0, // ID will be fixed up in AddDialogueEntry
                conversationID = Conversation.id,
                ActorID = Conversation.ConversantID,
                ConversantID = 1, // Hardcoded player ID
                DialogueText = message,
                falseConditionAction = "Block"
            };

            AddDialogueEntry(entry);

            return entry;
        }

        // If you want to add a DialogueEntry directly without using the PlayerSays or NPCSays utility methods, this is how.
        // Do note that you will need to handle DialogueEntry instantiating/initialization yourself.
        protected void AddDialogueEntry(DialogueEntry entry)
        {
            if (Conversation.dialogueEntries.Contains(entry))
            {
                LavenderLog.Error($"Dialogue entry \"{entry.DialogueText}\" already exists in conversation {Conversation.Title}");
                return;
            }

            if (entry.id == 0)
            {
                entry.id = ConversationPatchesManager.Instance.GetNextDialogueEntryID();
            }

            Conversation.dialogueEntries.Add(entry);

            LavenderLog.DialogueVerbose(Conversation.Title, $" Added new dialogue entry \"{entry.DialogueText}\" to conversation {Conversation.Title} with ID {entry.id}");
        }

        // Create a link from {@param source} to {@param dest}.  Links are navigation paths that determine what responses are available.
        // Use {@param priority} to specify the PixelCrushers sorting priority of the link.  In most cases, this is ConditionPriority.Normal.
        // Use {@param Ordering} to give more specific hints about where exactly you want the dest entry to appear in the list of responses.
        protected void Link(DialogueEntry source, DialogueEntry dest, LinkOrdering? ordering = null, ConditionPriority priority = ConditionPriority.Normal)
        {
            if (!Conversation.dialogueEntries.Contains(source) || !Conversation.dialogueEntries.Contains(dest))
            {
                LavenderLog.Error($"Attempted to link dialog entries on conversation {Conversation.Title}, but the provided entries are not both in the same conversation.  Source = {source.DialogueText}, Dest = {dest.DialogueText}");
            }

            if (source.id == 0)
            {
                LavenderLog.Error($" Source entry {source.DialogueText} is not in a Conversation.");
                return;
            }

            if (dest.id == 0)
            {
                LavenderLog.Error($" Target entry {dest.DialogueText} is not in a Conversation.");
                return;
            }

            foreach (Link l in source.outgoingLinks)
            {
                if (l.destinationConversationID == Conversation.id && l.originConversationID == Conversation.id)
                {
                    if (l.destinationDialogueID == dest.id && l.originDialogueID == source.id)
                    {
                        LavenderLog.Error($"Link already exists in conversation {Conversation.Title} from \"{source.DialogueText}\" to \"{dest.DialogueText}\"");
                        return;
                    }
                    else if (l.originDialogueID == source.id && l.destinationDialogueID == dest.id)
                    {
                        LavenderLog.Error($"Link already exists (reverse direction) in conversation {Conversation.Title} from \"{source.DialogueText}\" to \"{dest.DialogueText}\"");
                        return;
                    }
                }
            }

            Link link = new Link(Conversation.id, source.id, Conversation.id, dest.id);
            link.priority = priority;

            if (ordering == null)
            {
                ordering = LinkOrdering.DefaultOrdering();
            }
            ordering.AttachToDialogueEntry(Conversation, source, dest, link);

            LavenderLog.DialogueVerbose(Conversation.Title, $"Created link from \"{source.DialogueText}\" ({source.id}) to \"{dest.DialogueText}\" ({dest.id})");
        }

        // Convenience function to find DialogueEntry's that are responses to the provided entry
        protected IEnumerable<DialogueEntry> GetResponsesTo(DialogueEntry dialogueEntry)
        {
            return ConversationUtils.GetResponsesTo(Conversation, dialogueEntry);
        }

        // Convenience function to find only STATIC DialogueEntry's that are responses to the provided entry
        // A STATIC DialogueEntry does not have any condition set and contains no Lua - ie it is static unchanging text and is always available
        protected IEnumerable<DialogueEntry> GetStaticResponsesTo(DialogueEntry dialogueEntry)
        {
            return ConversationUtils.GetStaticResponsesTo(Conversation, dialogueEntry);
        }

        // Convenience function to find where best to put player responses
        protected IEnumerable<DialogueEntry> AdvanceToRespondable(DialogueEntry entry)
        {
            return ConversationUtils.AdvanceToRespondable(Conversation, entry);
        }

        #endregion

        #region Internal glue
        // Wrapper for PatchDialogue.  Do not override this function, as it contains safeguards to prevent double-patching and performs necessary state initialization
        // You don't ever need to call this function - Lavender handles that for you.
        internal void TryPatchDialogue()
        {
            if (RanPatch || SaveController.Loading)
            {
                return;
            }

            LavenderLog.DialogueVerbose(ConversationName, $"Patching dialogue conversation {ConversationName}");

            // Even though we have Conversation theoretically cached as a class variable, we cannot trust that it is valid
            // Scene transitions cause the Dialogue DB to be fully reloaded from disc, and thus create new conversation instances.
            try
            {
                DialogueDatabase db = DialogueController.instance.dialogueSystem.databaseManager.masterDatabase;
                if (db != null)
                {
                    _Conversation = db.GetConversation(ConversationName);
                    if (_Conversation != null)
                    {
                        PatchDialogue();

                        // Sorts links by priority in case we do something with a non-Normal priority
                        LinkUtility.SortOutgoingLinks(db, Conversation);

                        RanPatch = true;
                    }
                    else
                    {
                        LavenderLog.Error($"Failed to locate conversation \"{ConversationName}\".  Check spelling.");
                    }
                }
                else
                {
                    LavenderLog.Error($"Attempted to patch conversation \"{ConversationName}\", but the DialogueDB does not appear to exist.  " +
                        "Are you doing something weird to call TryPatchDialogue directly?  Stopit!");
                }
            }
            catch (Exception ex)
            {
                LavenderLog.Error($"Failed to patch conversation \"{ConversationName}\":");
                LavenderLog.Error(ex.ToString());
            }
        }

        // Package level so ConversationPatchesManager can reset the blocker flag when the scene changes
        internal void ConversationDBReloading()
        {
            RanPatch = false;
        }
        #endregion
    }
}
