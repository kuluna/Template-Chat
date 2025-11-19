using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

#nullable enable

/*
Specs









*/

public class ChatParser
{
    public string RawText { get; private set; } = string.Empty;
    public List<IChatCommand> Commands { get; } = new();
    public int CommandIndex { get; private set; } = 0;

    private string[] lines = new string[0];

    public void Parse(string rawText)
    {
        Commands.Clear();
        CommandIndex = 0;
        RawText = rawText;

        lines = RawText.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            if (line.StartsWith("#")) continue; // Skip comments

            var command = ParseLine(line, i);
            Commands.Add(command);
        }
    }

    private IChatCommand ParseLine(string line, int index)
    {
        var args = line.Split(',');
        return args[0] switch
        {
            "@text" => new TextChatCommand(index, args),
            "@image" => new ImageChatCommand(index, args),
            "@choice" => new ChoiceChatCommand(index, args),
            "@if" => new IfChatCommand(index, args),
            "@label" => new LabelChatCommand(index, args),
            "@wait" => new WaitChatCommand(index, args),
            _ => new UnknownChatCommand(index, args),
        };
    }

    public List<IChatCommand> NextCommands()
    {
        var next = new List<IChatCommand>();
        var BREAK_COMMANDS = new CommandType[]
        {
            CommandType.Text,
            CommandType.If,
            CommandType.Wait
        };

        while (CommandIndex < Commands.Count)
        {
            var command = Commands[CommandIndex];
            next.Add(command);
            CommandIndex += 1;

            if (BREAK_COMMANDS.Contains(command.Type))
            {
                break;
            }
        }

        return next;
    }
}
