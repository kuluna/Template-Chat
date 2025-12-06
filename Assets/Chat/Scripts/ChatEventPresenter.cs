using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable

namespace Template.Chat
{
    public class ChatEventPresenter
    {
        public IChatEventListener? Listener { private get; set; }
        public bool CanMoveToNext => nextCallDepth.Value <= 0;

        private readonly ChatParser parser = new();
        private readonly AsyncLocal<int> nextCallDepth = new();
        private readonly Dictionary<string, int> labelIndexMap = new();
        private readonly Dictionary<string, string> variables = new();

        public void Setup(string rawChatText)
        {
            parser.Parse(rawChatText);
            if (parser.Commands.Count == 0)
            {
                Debug.LogWarning("No chat nodes parsed from the provided text.");
            }

            // ラベルのインデックスをマッピング（最初に見つかったラベルを優先）
            labelIndexMap.Clear();
            for (int i = 0; i < parser.Commands.Count; i++)
            {
                if (parser.Commands[i] is LabelChatCommand labelCommand)
                {
                    if (!labelIndexMap.ContainsKey(labelCommand.LabelName))
                    {
                        labelIndexMap[labelCommand.LabelName] = i;
                    }
                }
            }

            // 変数をクリア
            variables.Clear();
            nextCallDepth.Value = 0;
        }


        public async Awaitable Next()
        {
            if (Listener == null) return;
            nextCallDepth.Value += 1;

            var STOP_COMMANDS = new HashSet<CommandType>
        {
            CommandType.Choice
        };

            IChatCommand lastCommand;
            do
            {
                var commands = parser.NextCommands();
                if (commands.Count == 0)
                {
                    nextCallDepth.Value = 0;
                    await Listener.OnEndChat();
                    return;
                }

                // コマンドを全て並列で実行
                var tasks = commands.Select(ExecuteCommand).ToList();
                await AwaitAll(tasks);

                lastCommand = commands.Last();
            } while (!STOP_COMMANDS.Contains(lastCommand.Type));

            nextCallDepth.Value -= 1;
        }

        /// <summary>
        /// 複数のAwaitableを並列で実行し、全ての完了を待つ
        /// </summary>
        private static async Awaitable AwaitAll(List<Awaitable> tasks)
        {
            var completionFlags = new bool[tasks.Count];
            var anyPending = true;

            while (anyPending)
            {
                anyPending = false;
                for (int i = 0; i < tasks.Count; i++)
                {
                    if (!completionFlags[i])
                    {
                        if (tasks[i].IsCompleted)
                        {
                            completionFlags[i] = true;
                        }
                        else
                        {
                            anyPending = true;
                        }
                    }
                }

                if (anyPending)
                {
                    await Awaitable.NextFrameAsync();
                }
            }
        }

        public async Awaitable ExecuteCommand(IChatCommand command)
        {
            if (Listener == null) return;
            switch (command)
            {
                case TextChatCommand textCommand:
                    await Listener.ShowText(textCommand);
                    // 連投でもデフォルトの待ちを作る
                    await Awaitable.WaitForSecondsAsync(0.5f);
                    break;

                case ImageChatCommand imageCommand:
                    await Listener.ShowImage(imageCommand);
                    await Awaitable.WaitForSecondsAsync(0.5f);
                    break;

                case WaitChatCommand waitCommand:
                    await Awaitable.WaitForSecondsAsync(waitCommand.Seconds);
                    break;

                case ChoiceChatCommand choiceCommand:
                    Listener.ShowChoice(choiceCommand);
                    break;

                case IfChatCommand ifCommand:
                    if (variables.TryGetValue(ifCommand.VariableName, out var actualValue))
                    {
                        if (ifCommand.Evaluate(actualValue))
                        {
                            JumpToLabel(ifCommand.GotoLabel);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Variable '{ifCommand.VariableName}' not found. Condition evaluates to false.");
                    }
                    break;

                case GotoChatCommand gotoCommand:
                    JumpToLabel(gotoCommand.GotoLabel);
                    break;

                case LabelChatCommand:
                    // ラベル自体は何もしない
                    break;

                case MessageChatCommand messageCommand:
                    await Listener.ShowMessage(messageCommand);
                    break;

                default:
                    Debug.LogWarning($"Unknown command type: {command.Type} at Line {command.Index}");
                    break;
            }

            // Implement command execution logic here.
            await Task.CompletedTask;
        }

        private void JumpToLabel(string labelName)
        {
            if (labelIndexMap.TryGetValue(labelName, out int index))
            {
                parser.CommandIndex = index;
            }
            else
            {
                Debug.LogError($"Label '{labelName}' not found. Cannot jump.");
            }
        }

        public void SetVariable(string variableName, string value)
        {
            variables[variableName] = value;
        }

        public interface IChatEventListener
        {
            public Awaitable ShowImage(ImageChatCommand command);
            public Awaitable ShowText(TextChatCommand command);
            public Awaitable ShowMessage(MessageChatCommand command);
            public void ShowChoice(ChoiceChatCommand command);
            public Awaitable OnEndChat();
        }
    }
}
