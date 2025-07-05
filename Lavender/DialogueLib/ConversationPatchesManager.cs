using System;
using System.Collections.Generic;

namespace Lavender.DialogueLib
{
    public class ConversationPatchesManager
    {
        private const int NewDialogueEntryStartingID = 10000;
        private static int NextDialogueEntryID = NewDialogueEntryStartingID;
        private bool ExecutingPatchers = false;
        private List<ConversationMaker> DeferredDemotedMakers = new List<ConversationMaker>();

        public static Dictionary<string, ConversationMaker> ConversationMakers = new Dictionary<string, ConversationMaker>();
        public static List<ConversationPatcher> Conversations = new List<ConversationPatcher>();
        public static Dictionary<string, List<ConversationPatcher>> ConversationToPatcher = new Dictionary<string, List<ConversationPatcher>>();

        private static ConversationPatchesManager _instance = null!;

        public static ConversationPatchesManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ConversationPatchesManager();
                }

                return _instance;
            }
        }

        private ConversationPatchesManager() 
        {
            SaveController.LoadingStarted += OnLoadingStarted;
            SaveController.LoadingDone += OnLoadingDone;
        }

        ~ConversationPatchesManager()
        {
            SaveController.LoadingStarted -= OnLoadingStarted;
            SaveController.LoadingDone -= OnLoadingDone;
        }

        /// <summary>
        /// You probably want Lavender.AddConversationPatcher().
        /// </summary>
        /// <param name="patcher"></param>
        public void AddConversationPatcher(ConversationPatcher patcher)
        {
            if (!Conversations.Contains(patcher))
            {
                // ConversationMakers get special handling to ensure they run first, and also to handle uniqueness issues
                if (patcher is ConversationMaker)
                {
                    if (ConversationMakers.TryGetValue(patcher.ConversationName, out ConversationMaker existingMaker))
                    {
                        // Sorry buddy, someone else got here first.
                        ((ConversationMaker)patcher).InformIsDuplicateMaker(/*bInformManager*/false); // We don't need to be informed, we already know
                        Conversations.Add(patcher); // Back of the line
                    }
                    else
                    {
                        ConversationMakers.Add(patcher.ConversationName, (ConversationMaker)patcher);

                        // Makers have to execute first, so they go at the start of the list
                        // Order is very important or we could attempt to patch a conversation that hasn't been created yet
                        Conversations.Insert(0, patcher);
                    }
                }
                else
                {
                    Conversations.Add(patcher);
                }

                LavenderLog.DialogueVerbose(patcher.ConversationName, $"Registered conversation patcher {patcher.GetType().Name} for conversation {patcher.ConversationName}");

                // Track which patchers are modifying which conversations
                if (ConversationToPatcher.ContainsKey(patcher.ConversationName))
                {
                    ConversationToPatcher[patcher.ConversationName].Add(patcher);
                }
                else
                {
                    ConversationToPatcher.Add(patcher.ConversationName, new List<ConversationPatcher>([patcher]));
                }

                // If we're currently in a gameplay state, then immediately run the patch.
                // Otherwise, it will be patched once the game loads a gameplay scene.
                if (Lavender.instance.LoadingDone && Lavender.instance.lastLoadedScene != 0)
                {
                    patcher.TryPatchDialogue();
                }
            }
        }

        /// <summary>
        /// You probably want Lavender.GetPatchersForConversation().
        /// </summary>
        /// <param name="conversationName"></param>
        /// <returns></returns>
        public IEnumerable<ConversationPatcher> GetPatchersForConversation(string conversationName)
        {
            if (ConversationToPatcher.TryGetValue(conversationName, out var patchers))
            {
                return patchers;
            }
            return [];
        }

        // Single access point to generate DialogueEntry IDs
        // Allows us to dynamically avoid duplicate IDs at runtime
        // Assumes that NewDialogueEntryStartingID is a high enough number that the vanilla dialogues don't reach it
        //  If we start getting collisions with vanilla dialogues, we need to increase NewDialogueEntryStartingID
        // @TODO Norbby: Refactor to make this generated once at runtime in the ConversationPatcher via Template.GetNextDialogueEntryID() and cached per object?
        public int GetNextDialogueEntryID()
        {
            // Intentional that this is a post-op increment
            // First ID used is equal to NewDialogueEntryStartingID
            return NextDialogueEntryID++;
        }

        private void OnLoadingStarted()
        {
            foreach (var patcher in Conversations)
            {
                patcher.ConversationDBReloading();
            }
        }

        private void OnLoadingDone()
        {
            LavenderLog.DialogueVerboseNoConversation($"Scene loaded, reset dialogue patcher DialogueEntry IDs back to {NewDialogueEntryStartingID} and running all registered conversation patchers.");
            NextDialogueEntryID = NewDialogueEntryStartingID;
            ExecutingPatchers = true;
            foreach (var patcher in Conversations)
            {
                try
                {
                    patcher.TryPatchDialogue();
                }
                catch (Exception e)
                {
                    LavenderLog.Error($"Exception while patching conversation \"{patcher.ConversationName}\": " + e.ToString());
                }
            }
            ExecutingPatchers = false;

            foreach (var patcher in DeferredDemotedMakers)
            {
                FlagMakerAsOnlyPatcher(patcher);
            }
            DeferredDemotedMakers.Clear();
        }

        // When a ConversationMaker discovers that it is trying to act on a conversation that already exists, it calls back to us so we can update its registration
        // If it's not creating a new conversation, then it should be treated like a normal patcher (ie no execution priority on scene change, no entry in the makers dictionary)
        internal void FlagMakerAsOnlyPatcher(ConversationMaker maker)
        {
            if (ExecutingPatchers)
            {
                // Can't change the registries while we're iterating over them.  Defer this update.
                DeferredDemotedMakers.Add(maker);
            }
            else
            {
                ConversationMakers.Remove(maker.ConversationName);
                Conversations.Remove(maker);
                Conversations.Add(maker);
            }
        }
    }
}
