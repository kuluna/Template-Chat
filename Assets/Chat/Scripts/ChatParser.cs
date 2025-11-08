using UnityEngine;

#nullable enable

/*
Specs









*/

public class ChatParser
{
    public string RawText { get; private set; } = string.Empty;

    private string[] lines = new string[0];
    private int pointer = 0;

    public void Parse(string rawText)
    {
        RawText = rawText;
    
        lines = RawText.Split('\n');
        pointer = 0;

        while (pointer < lines.Length)
        {
            
        }
    }
}
