using Lavender.DialogueLib;
using PixelCrushers.DialogueSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lavender.Test
{
    public class TestConversationPatcherTatyana2 : ConversationPatcher
    {
        public TestConversationPatcherTatyana2()
            : base("Tenement/Outside/Tatyana Gopnikova")
        {
        }

        protected override void PatchDialogue()
        {
            // In this test, we are going to edit Tatyana's greetings (what she says when the conversation opens)
            // This will help to test interactions when 2 different patchers modify the same conversation, as well as modification of existing nodes.

            // Tatyana has a "first time" greeting that has the condition: Actor["Tenement_Tatyana_Gopnikova"].TalkedFirstTime == false
            // And her "repeat" greetings have conditions of the form: Actor["Tenement_Tatyana_Gopnikova"].Start == 0/1/2/3

            IEnumerable<DialogueEntry> intros = GetResponsesTo(Conversation.GetFirstDialogueEntry());

            foreach (DialogueEntry de in intros)
            {
                if (de.conditionsString.Contains("TalkedFirstTime"))
                {
                    de.DialogueText = $"WARNING! Your version of Obenseuer has Lavender Test Suite installed.{Pause}{Pause}{Pause} Please remove Lavender.Test.dll from the Plugins folder!";
                }
                else if (de.conditionsString.Contains("Start"))
                {
                    int substrStart = de.conditionsString.IndexOf("==");
                    string substr = de.conditionsString.Substring(substrStart + 2);

                    BepinexPlugin.Log.LogInfo($"Parsing Start idx from Tatyana greeting.  ConditionsString: \"{de.conditionsString}\".  idx substring: {substr}");

                    try
                    {
                        int startIdx = int.Parse(substr);

                        string originalText = de.DialogueText;
                        de.DialogueText = $"Lavender Test Suite greeting #{startIdx}.{Pause}{Pause}{Pause} Please remove Lavender.Test.dll from the Plugins folder!";
                        BepinexPlugin.Log.LogInfo($"Reworded greeting on dialogue node {de.id} from \"{originalText}\" to \"{de.DialogueText}\"");
                    }
                    catch (Exception)
                    {
                        BepinexPlugin.Log.LogError($"Failed to parse start id from Tatyana conversation greeting: \"{de.DialogueText}\" with conditionsString: \"{de.conditionsString}\"");
                    }

                }
                else
                {
                    BepinexPlugin.Log.LogError($"Unrecognized Tatyana greeting: \"{de.DialogueText}\" with conditionsString: \"{de.conditionsString}\"");
                }
            }
        }
    }
}
