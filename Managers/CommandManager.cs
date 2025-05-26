using System.Reflection;
using UnityEngine;
using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External;

public class CommandManager
{
    private static readonly Dictionary<string, IWebsocketCommand> _commands = new();


    public static void RegisterAllCommands()
    {
        var commandTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(IWebsocketCommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
        foreach (var type in commandTypes)
        {
            var instance = (IWebsocketCommand)Activator.CreateInstance(type);
            _commands[instance.CommandName] = instance;
            AeroltPlugin.Log.LogInfo($"[CommandManager] Registering command {instance.CommandName}");
        }
    }
    
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