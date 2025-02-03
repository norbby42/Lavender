using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Text;

namespace Lavender.CommandLib;

/// <summary>
/// The static home for all Command related functions
/// </summary>
public static class CommandManager
{
    private static readonly Dictionary<string, IConsoleCommand> CommandRegistry = [];
    internal static DeveloperConsole? LastInConsole;

    static CommandManager()
    {
        LavenderLog.Log("Command manager is alive");
    }

    /// <summary>
    /// Register an IConsoleCommand with the handler
    /// </summary>
    /// <param name="command"></param>
    /// <returns>Returns true on success</returns>
    public static bool RegisterCommand(IConsoleCommand command)
    {

        LavenderLog.Log($"Registering a command {command.Name}");

        if (CommandRegistry.TryGetValue(command.Name, out var exCmd))
        {
            return false;
        }

        CommandRegistry[command.Name] = command;
        return true;
    }

    internal static bool RunCommand(string cmdName, string[] args)
    {
        if (cmdName.ToLowerInvariant() == "ext_help")
        {
            ShowHelp();
            return false;
        }

        if (CommandRegistry.TryGetValue(cmdName, out var command))
        {
            try
            {
                command.Execute(args);
            }
            catch (Exception ex)
            {
                LavenderLog.Log($"\n\nEncountered an exception while running a command:\n  Command: {cmdName}\n  Ex: {ex}");
            }

            return false;

        }

        return true;
    }

    private static void ShowHelp()
    {
        StringBuilder sb = new("--- Lavender CommandLib Extended Console ---\n\n");

        foreach (IConsoleCommand command in CommandRegistry.Values)
        {
            sb.Append("  ");
            sb.AppendLine(command.Name);
            sb.AppendLine(command.Description);
        }

        PrintToDevConsole(sb);
    }

    internal static void PrintToDevConsole(string message)
    => LastInConsole?.Print(message);

    /// <summary>
    /// Prints a message to the dev console
    /// </summary>
    /// <param name="message"></param>
    public static void PrintToDevConsole(object message)
    => PrintToDevConsole(message.ToString());
}
