using PixelCrushers.DialogueSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lavender.DialogueLib
{
    public abstract class ConversationMaker : ConversationPatcher
    {
        private bool ConversationCreationDisabled = false;
        public string ActorName;

        /// <summary>
        /// Create our ConversationMaker instance.  Once created, please register with Lavender.AddConversationMaker().
        /// </summary>
        /// <param name="conversationName">Internal name of the conversation</param>
        public ConversationMaker(string conversationName, string actorName)
            : base(conversationName)
        {
            ActorName = actorName;
        }

        /// <summary>
        /// Check if this conversation maker has no conflicts - ie the conversation is purely synthetic and made explicitly for this ConversationMaker/Patcher.
        /// In the event there is a conflict (such as 2 mods trying to create the same conversation), this can be used by implementing classes to change how they patch out the conversation.
        /// </summary>
        /// <returns>true if this conversation is exclusively owned by this instance, false if it is shared with other instances</returns>
        public bool HasNoConflicts()
        {
            return !ConversationCreationDisabled;
        }

        /// <summary>
        /// In the event that you are creating a conversation for a new actor/NPC, a new entry will need to be created for them in the DialogueDatabase
        /// The default implementation creates a very basic definition; you can override this function if you want to extend the definition
        /// </summary>
        /// <param name="db">DialogueDatabase the actor will be registered to</param>
        /// <param name="pcTemplate">PixelCrusher's helper Template object for object creation</param>
        /// <returns>An Actor instance ready to be added to the database</returns>
        protected virtual Actor CreateActor(DialogueDatabase db, Template pcTemplate)
        {
            return pcTemplate.CreateActor(pcTemplate.GetNextActorID(db), ActorName, false);
        }

        /// <summary>
        /// Executed whenever the conversation is detected to already exist (at any point during registration and setup)
        /// This could be for any number of reasons:
        ///  A vanilla conversation with this ID may already exist
        ///  Another mod may be making a conversation with the same name
        ///  An error in your mod may be trying to create this conversation multiple times.  Only create and register the ConversationMaker once - at Plugin Start!
        /// </summary>
        protected void OnConversationAlreadyExists() { }

        internal void InformIsDuplicateMaker(bool bInformManager = true)
        {
            if (!ConversationCreationDisabled)
            {
                ConversationCreationDisabled = true;
                if (bInformManager)
                {
                    ConversationPatchesManager.Instance.FlagMakerAsOnlyPatcher(this);
                }
                LavenderLog.DialogueVerbose(ConversationName, $"Conversation with name {ConversationName} already exists; disabling conversation maker.");
                OnConversationAlreadyExists();
            }
        }

        private int GetConversantID(DialogueDatabase db, Template pcTemplate)
        {
            Actor actor = db.GetActor(ActorName);
            // Sanity check to make sure if we do get an Actor, that it has a valid id
            if (actor != null && actor.id != 0)
            {
                return actor.id;
            }

            actor = CreateActor(db, pcTemplate);

            if (actor != null)
            {
                db.actors.Add(actor);
                return actor.id;
            }
            else
            {
                LavenderLog.Error($"ConversationMaker failed to create Actor for name {ActorName}. Check implementation of CreateActor in class {GetType().FullName}");
            }

            return 0;
        }

        /// <summary>
        /// Replace ConversationPatcher's TryPatchDialogue to implement our logic to create the conversation framework
        /// Once the Conversation framework is created, run ConversationPatcher's TryPatchDialogue to handle the rest of the setup
        /// </summary>
        internal override void TryPatchDialogue()
        {
            if (ConversationCreationDisabled)
            {
                // If we aren't allowed to create the conversation then don't even try - immediately route up to the patcher.
                base.TryPatchDialogue();
                return;
            }

            if (RanPatch || SaveController.Loading)
            {
                return;
            }
            
            LavenderLog.DialogueVerbose(ConversationName, $"Making dialogue conversation {ConversationName}");


            DialogueDatabase db = DialogueController.instance.dialogueSystem.databaseManager.masterDatabase;

            if (db != null)
            {
                // Test to make sure the conversation doesn't already exist, and we simply haven't found out yet
                Conversation testConv = db.GetConversation(ConversationName);

                if (testConv != null)
                {
                    // Nope, conversation already exists.  Abort creation and fall back to patcher-only behavior
                    InformIsDuplicateMaker(true);
                    base.TryPatchDialogue();
                    return;
                }


                Template pcTemplate = Template.FromDefault();

                // Ask the child class what actor we should use.  Probably making a new one
                int ConversantID = GetConversantID(db, pcTemplate);

                // We use the Template class from PixelCrushers to create the Conversation object
                // Because it ensures that the Conversation has all necessary fields created correctly
                // We don't use the Template for creating DialogueEntry's because it imposes a scaling O(n) cost each time it looks up the next ID
                // It's also nice to use initialization syntax
                Conversation conv = pcTemplate.CreateConversation(pcTemplate.GetNextConversationID(db), ConversationName);

                conv.ConversantID = ConversantID;
                conv.ActorID = 1; // Player

                DialogueEntry start = new DialogueEntry()
                {
                    fields = [], 
                    id = 0,                                 // ID 0 is correct for the start entry
                    conversationID = conv.id,       
                    ActorID = 1,
                    ConversantID = ConversantID,
                    Title = "START",
                    Sequence = "None()",                    // All START nodes are passthrough sequences
                    falseConditionAction = "Block"
                };

                conv.dialogueEntries.Add(start);

                db.AddConversation(conv);

                LavenderLog.DialogueVerbose(ConversationName, $"Dialogue conversation {ConversationName} successfully created and assigned ID {conv.id}.  Conversant is {ConversantID}.");

                base.TryPatchDialogue();
            }
            else
            {
                LavenderLog.Error($"Attempted to make conversation \"{ConversationName}\", but the DialogueDB does not appear to exist.  " +
                        "Are you doing something weird to call TryPatchDialogue directly?  Stopit!");
            }
        }
    }
}
