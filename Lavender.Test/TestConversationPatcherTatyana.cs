using JetBrains.Annotations;
using Lavender.DialogueLib;
using System;
using System.Collections.Generic;
using System.Text;
using PixelCrushers.DialogueSystem;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Lavender.Test
{
    public class TestConversationPatcherTatyana : ConversationPatcher
    {
        private int LastRandom = 0;
        public TestConversationPatcherTatyana() 
            : base("Tenement/Outside/Tatyana Gopnikova")
        {
        }

        protected override void PatchDialogue()
        {
            // In this test, we will add an additional section to Tatyana's dialogue - a small "test menu" with an assortment of test data

            // Tatyana has a "first time" greeting that has the condition: Actor["Tenement_Tatyana_Gopnikova"].TalkedFirstTime == false
            // And her "repeat" greetings have conditions of the form: Actor["Tenement_Tatyana_Gopnikova"].Start == 0/1/2/3
            // BUT, the dialogue system will string-replace "Tenement_Tatyana_Gopnikova" with the dynamic runtime ID of the actor
            // So instead we filter by the presence of "Start"
            // NB - Tatyana *also* has a Sequence node that serves as a router node between her Start and the player response nodes.  Which is why AdvanceToRespondable is important.
            IEnumerable<DialogueEntry> standardIntros = GetResponsesTo(Conversation.GetFirstDialogueEntry())
                .Where(de => de.conditionsString.Contains("Start")) // Start our search from any initial response nodes that run off the "Start" criteria, which is how greetings are cycled between interactions
                .SelectMany(de => AdvanceToRespondable(de)) // Then progress down the dialog tree to find where we should actually put our player responses
                .Distinct(); // De-duplicate identical results

            if (!standardIntros.Any())
            {
                BepinexPlugin.Log.LogError("Did not find any node to respond to at the start of Tatyana's conversation");
            }

            DialogueEntry playerInitiateTestFirst = PlayerSays("Open dialogue patching test menu");
            DialogueEntry lateTest = PlayerSays("This message is very late in the options.  But not last.  [End Conversation]");
            foreach (var de in standardIntros)
            {
                Link(de, playerInitiateTestFirst, LinkOrdering.First());

                Link(de, lateTest);
            }

            DialogueEntry testMenu = NPCSays("Beep boop testing");
            testMenu.userScript = "RollRandomNumber(1, 10)"; // Generate the random number before the menu is loaded
            Link(playerInitiateTestFirst, testMenu);

            
            #region Individual tests

            // Test for non-Lua markup
            DialogueEntry markupTest = PlayerSays("Markup test");
            Link(testMenu, markupTest);
            DialogueEntry markupResponse = NPCSays($"{Quote}O{Pause}K{Pause}{Quote}.{Pause}{Pause}{Pause}Dramatic pauses done.");
            Link(markupTest, markupResponse);
            Link(markupResponse, testMenu);

            // Lua function registration and calls
            DialogueEntry tellMeARandomNumber = PlayerSays($"I'm thinking of a random number {Lua("GetLastRandomNumber()")}.  What number am I thinking of?");
            // We can't directly generate our test random number in the dialogue entry because it will be generated multiple times.
            Link(testMenu, tellMeARandomNumber);
            DialogueEntry npcRandomNumber = NPCSays($"You were thinking of... {Lua("GetLastRandomNumber()")}");
            Link(tellMeARandomNumber, npcRandomNumber);
            Link(npcRandomNumber, testMenu);

            // userConditions
            DialogueEntry chanceToAppear = PlayerSays("Show me some responses that use userConditions to be visible (or not).");
            Link(testMenu, chanceToAppear);
            DialogueEntry chanceToAppearMenu = NPCSays("Okay.  Change zones to reroll the dialog entries.");
            Link(chanceToAppear, chanceToAppearMenu);

            for (int i = 0; i < 4; ++i)
            {
                DialogueEntry chanceEntry = PlayerSays($"Response {i} with a 50% chance to appear");
                // NOTE: We CANNOT use Lua RandomNumber() because the condition is evaluated twice:
                // First, when the response list is generated to show in the UI
                // Second, when the player actually selects a response
                // If the condition returns true during response generation, then false during response selection, it will fail to navigate and can freeze the dialogue system
                chanceEntry.conditionsString = $"{UnityEngine.Random.Range(0, 2)} == 1";
                BepinexPlugin.Log.LogInfo($"Chance-based userConditions response #{i} setup with condition: {chanceEntry.conditionsString}");
                Link(chanceToAppearMenu, chanceEntry);
                Link(chanceEntry, testMenu);
            }

            Link(chanceToAppearMenu, playerInitiateTestFirst);

            #endregion

            // Leave conversation from test menu.  Otherwise we soft lock player in the conversation
            DialogueEntry bye = PlayerSays("Goodbye");
            Link(testMenu, bye);
        }

        public override void OnConversationStarted(InteractableTalk interactableTalk)
        {
            BepinexPlugin.Log.LogInfo($"Registering conversation Lua functions from {typeof(TestConversationPatcherTatyana).ToString()}");
            PixelCrushers.DialogueSystem.Lua.RegisterFunction("RollRandomNumber", this, typeof(TestConversationPatcherTatyana).GetMethod("LuaRandomNumber"));
            PixelCrushers.DialogueSystem.Lua.RegisterFunction("GetLastRandomNumber", this, typeof(TestConversationPatcherTatyana).GetMethod("LuaLastRandom"));
        }

        public override void OnConversationEnded(InteractableTalk interactableTalk)
        {
            BepinexPlugin.Log.LogInfo($"Unregistering conversation Lua functions from {typeof(TestConversationPatcherTatyana).ToString()}");
            PixelCrushers.DialogueSystem.Lua.UnregisterFunction("RollRandomNumber");
            PixelCrushers.DialogueSystem.Lua.UnregisterFunction("GetLastRandomNumber");
        }

        // NB - Lua doesn't really acknowledge the difference between integer and floating point most of the time, and unless you go to lengths it'll just assume that everything is a floating point
        // This is why Lua functions that take an int param generate this error:
        // ArgumentException: Object of type 'System.Single' cannot be converted to type 'System.Int32'.
        public int LuaRandomNumber(float min, float max)
        {
            LastRandom = UnityEngine.Random.Range((int)min, (int)max);
            BepinexPlugin.Log.LogInfo($"LuaRandomNumber is selecting {LastRandom}");
            return LastRandom;
        }

        public int LuaLastRandom()
        {
            return LastRandom;
        }
    }
}
