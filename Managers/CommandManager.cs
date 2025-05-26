using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using WebSocketSharp.Net.WebSockets;

namespace Aerolt_External;

public class CommandManager
{
    private static readonly Dictionary<string, Type> _commands = new();
    public static void RegisterAllCommands()
    {
        var commandTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(IWebsocketCommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
        foreach (var type in commandTypes)
        {
            _commands[type.Name] = type;
            AeroltPlugin.Log.LogInfo($"[CommandManager] Registering command {type.Name}");
        }
    }

    public static void Execute(string commandName, string payload, WebSocketContext context)
    {
        if (_commands.TryGetValue(commandName, out var command))
        {
            var commandInstance = (IWebsocketCommand)Activator.CreateInstance(command);
            JsonConvert.PopulateObject(payload, commandInstance);
            commandInstance.Execute(context);
        }
        else
        {
            Debug.LogWarning($"Unknown command: {commandName}");
        }
    }
}