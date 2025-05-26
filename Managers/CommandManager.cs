using UnityEngine;
using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External;

public class CommandManager
{
    private static readonly Dictionary<string, IWebsocketCommand> _commands = new();

    public static void Register(IWebsocketCommand command)
    {
        _commands[command.CommandName] = command;
    }

    public static void Execute(string commandName, string payload, WebSocketContext context)
    {
        if (_commands.TryGetValue(commandName, out var command))
        {
            command.Execute(payload, context);
        }
        else
        {
            Debug.LogWarning($"Unknown command: {commandName}");
        }
    }
}