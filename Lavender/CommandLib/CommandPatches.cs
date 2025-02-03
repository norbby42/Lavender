using System;
using HarmonyLib;
using Lavender.CommandLib;

internal static class CommandManagerPatches
{
  [HarmonyPatch(typeof(DeveloperConsole), nameof(DeveloperConsole.HandleInput))]
  [HarmonyPrefix]
  internal static bool HandleInput_Prefix(DeveloperConsole __instance, string input)
  {
    //This ensures that the Dev console will find us rather than needing to search
    //The hierarchy. Any instance of Dev console that runs HandleInput will call in
    //to the command manager, yay!
    CommandManager.LastInConsole = __instance;

    //Block bad input
    if (input == "")
      return false;

    string[] args = input.SplitOutsideQuotes(' ');
    string cmdName = args[0] ?? string.Empty;

    return CommandManager.RunCommand(cmdName, args);
  }

}
