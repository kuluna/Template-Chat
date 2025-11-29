using System.Text.RegularExpressions;

#nullable enable

namespace Template.Chat
{

    ////////////////// Define abstract classes //////////////////

    public abstract class ChatCommand : IChatCommand
    {
        public int Index { get; protected set; }
        public string[] Args { get; protected set; } = new string[0];
        public abstract CommandType Type { get; }

        protected ChatCommand(int index, string[] args)
        {
            Index = index;
            Args = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                Args[i] = args[i].Trim();
            }

            Check();
        }

        public abstract void Check();
    }

    public interface IChatCommand
    {
        public int Index { get; }
        public string[] Args { get; }
        public CommandType Type { get; }
    }

    public enum CommandType
    {
        Unknown,
        Text,
        Image,
        Choice,
        If,
        Label,
        Wait,
        Goto
    }

    public class ChatCommandException : System.Exception
    {
        public ChatCommandException(ChatCommand command, string? message = null) : base(BuildMessage(command, message)) { }

        private static string BuildMessage(ChatCommand command, string? message = null)
        {
            var exceptionMessage = message != null ? $"{message}\n" : string.Empty;
            exceptionMessage += $"Invalid command at index {command.Index} of type {command.Type} with args: {string.Join(", ", command.Args)}";
            return exceptionMessage;
        }
    }

    ////////////////// Command classes //////////////////

    /// <summary>
    /// パースできない不明なコマンド
    /// </summary>
    public class UnknownChatCommand : ChatCommand
    {
        public override CommandType Type => CommandType.Unknown;
        public UnknownChatCommand(int index, string[] args) : base(index, args) { }
        public override void Check()
        {
            throw new ChatCommandException(this, "不明なコマンドです。");
        }
    }

    /// <summary>
    /// チャットテキスト表示コマンド
    /// <para>
    /// example:<br/>
    /// @text, Hello, world!
    /// </para>
    /// </summary>
    public class TextChatCommand : ChatCommand
    {
        public override CommandType Type => CommandType.Text;
        public string Text { get; }

        public TextChatCommand(int index, string[] args) : base(index, args)
        {
            Text = string.Join(", ", Args[1..]);
        }

        public override void Check()
        {
            if (Args.Length < 2)
            {
                throw new ChatCommandException(this, "必要な引数が不足しています。\nex: @text, <表示するテキスト>");
            }

            if (string.IsNullOrWhiteSpace(Args[1]))
            {
                throw new ChatCommandException(this, "表示するテキストが空です。\nex: @text, <表示するテキスト>");
            }
        }
    }

    /// <summary>
    /// 画像表示コマンド
    /// <para>
    /// example:<br/>
    /// @image, <画像名>
    /// </para>
    /// </summary>
    public class ImageChatCommand : ChatCommand
    {
        public override CommandType Type => CommandType.Image;
        public string ImageName { get; }

        public ImageChatCommand(int index, string[] args) : base(index, args)
        {
            ImageName = Args[1];
        }

        public override void Check()
        {
            if (Args.Length != 2)
            {
                throw new ChatCommandException(this, "必要な引数が不足か多いです。\nex: @image, <画像名>");
            }

            if (string.IsNullOrWhiteSpace(Args[1]))
            {
                throw new ChatCommandException(this, "画像名が空です。\nex: @image, <画像名>");
            }
        }
    }

    /// <summary>
    /// 選択肢表示コマンド。選択肢は3つまで指定可能です。
    /// <para>
    /// example:<br/>
    /// @choice, 変数名, 選択肢1, 選択肢2, ...
    /// </para>
    /// </summary>
    public class ChoiceChatCommand : ChatCommand
    {
        public override CommandType Type => CommandType.Choice;
        public string VariableName { get; }
        public string[] Choices { get; }

        public ChoiceChatCommand(int index, string[] args) : base(index, args)
        {
            VariableName = Args[1];
            Choices = Args[2..];
        }

        public override void Check()
        {
            if (Args.Length < 3 || Args.Length > 5)
            {
                throw new ChatCommandException(this, "必要な引数が不足か多いです。\nex: @choice, <変数名>, <選択肢1>, <選択肢2>, ...");
            }

            if (string.IsNullOrWhiteSpace(Args[1]))
            {
                throw new ChatCommandException(this, "変数名が空です。\nex: @choice, <変数名>, <選択肢1>, <選択肢2>, ...");
            }

            for (int i = 2; i < Args.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(Args[i]))
                {
                    throw new ChatCommandException(this, $"選択肢{i - 1}が空です。\nex: @choice, <変数名>, <選択肢1>, <選択肢2>, ...");
                }
            }
        }
    }

    /// <summary>
    /// 条件分岐コマンド。条件がtrueなら指定ラベルへジャンプします。
    /// <para>
    /// example:<br/>
    /// @if, 変数名, 期待値, ジャンプ先ラベル
    /// </para>
    /// 数値比較の場合: 20, =20, >20, &lt;20
    /// </summary>
    public class IfChatCommand : ChatCommand
    {
        public override CommandType Type => CommandType.If;
        public string VariableName { get; }
        public string ExpectedValue { get; }
        public string GotoLabel { get; }

        public EvalType ValueEvalType { get; }

        private static readonly Regex NumericComparePattern = new(@"^(>|<|=)(.+)$");

        public IfChatCommand(int index, string[] args) : base(index, args)
        {
            VariableName = Args[1];
            ExpectedValue = Args[2];
            GotoLabel = Args[3];

            ValueEvalType = ExpectedValue switch
            {
                "true" or "false" => EvalType.Boolean,
                _ when NumericComparePattern.IsMatch(ExpectedValue) => EvalType.Numeric,
                _ when double.TryParse(ExpectedValue, out _) => EvalType.Numeric,
                _ => EvalType.String
            };

            AdditionalCheck();
        }

        public override void Check()
        {
            if (Args.Length != 4)
            {
                throw new ChatCommandException(this, "必要な引数が不足か多いです。\nex: @if, 変数名, 期待値, ジャンプ先ラベル");
            }

            if (string.IsNullOrWhiteSpace(Args[1]))
            {
                throw new ChatCommandException(this, "変数名が空です。\nex: @if, 変数名, 期待値, ジャンプ先ラベル");
            }

            if (string.IsNullOrWhiteSpace(Args[2]))
            {
                throw new ChatCommandException(this, "期待値が空です。\nex: @if, 変数名, 期待値, ジャンプ先ラベル");
            }

            if (string.IsNullOrWhiteSpace(Args[3]))
            {
                throw new ChatCommandException(this, "ジャンプ先ラベルが空です。\nex: @if, 変数名, 期待値, ジャンプ先ラベル");
            }
        }

        public void AdditionalCheck()
        {
            if (ValueEvalType == EvalType.Numeric)
            {
                var match = NumericComparePattern.Match(ExpectedValue);
                if (match.Success)
                {
                    var numericPart = match.Groups[2].Value;
                    if (!double.TryParse(numericPart, out _))
                    {
                        throw new ChatCommandException(this, "数値比較の期待値が不正です。\nex: 20, =20, >20, <20");
                    }
                }
                else
                {
                    // 比較演算子がない場合、数値としてパースできるか確認
                    if (!double.TryParse(ExpectedValue, out _))
                    {
                        throw new ChatCommandException(this, "数値比較の期待値が不正です。\nex: 20, =20, >20, <20");
                    }
                }
            }
        }

        public bool Evaluate(string actualValue)
        {
            return ValueEvalType switch
            {
                EvalType.String => actualValue == ExpectedValue,
                EvalType.Boolean => bool.TryParse(actualValue, out var boolValue) &&
                                     boolValue == bool.Parse(ExpectedValue),
                EvalType.Numeric => EvaluateNumeric(actualValue),
                _ => false
            };
        }

        private bool EvaluateNumeric(string actualValue)
        {
            if (!double.TryParse(actualValue, out var actual))
            {
                return false;
            }

            // 比較演算子を抽出
            var op = ExpectedValue[0];
            var expectedNumStr = ExpectedValue;

            if (op == '>' || op == '<' || op == '=')
            {
                expectedNumStr = ExpectedValue[1..];
            }
            else
            {
                op = '='; // デフォルトは等号
            }

            if (!double.TryParse(expectedNumStr, out var expected))
            {
                return false;
            }

            return op switch
            {
                '>' => actual > expected,
                '<' => actual < expected,
                '=' => System.Math.Abs(actual - expected) < 0.0001, // 浮動小数点誤差を考慮
                _ => false
            };
        }

        public enum EvalType
        {
            String,
            Numeric,
            Boolean
        }
    }

    /// <summary>
    /// ラベル定義コマンド
    /// <para>
    /// example:<br/>
    /// @label, StartPoint
    /// </para>
    /// </summary>
    public class LabelChatCommand : ChatCommand
    {
        public override CommandType Type => CommandType.Label;
        public string LabelName { get; }

        public LabelChatCommand(int index, string[] args) : base(index, args)
        {
            LabelName = Args[1];
        }

        public override void Check()
        {
            if (Args.Length != 2)
            {
                throw new ChatCommandException(this, "必要な引数が不足か多いです。\nex: @label, <ラベル名>");
            }

            if (string.IsNullOrWhiteSpace(Args[1]))
            {
                throw new ChatCommandException(this, "ラベル名が空です。\nex: @label, <ラベル名>");
            }
        }
    }

    /// <summary>
    /// 待機コマンド。指定できるのは5秒までです。
    /// <para>
    /// example:<br/>
    /// @wait, 2.5
    /// </para>
    /// </summary>
    public class WaitChatCommand : ChatCommand
    {
        public override CommandType Type => CommandType.Wait;
        public float Seconds { get; }

        public WaitChatCommand(int index, string[] args) : base(index, args)
        {
            Seconds = float.Parse(Args[1]);
        }

        public override void Check()
        {
            if (Args.Length != 2)
            {
                throw new ChatCommandException(this, "必要な引数が不足か多いです。\nex: @wait, <秒数>");
            }

            if (!float.TryParse(Args[1], out var duration) || duration <= 0 || duration > 5)
            {
                throw new ChatCommandException(this, "5秒以下の数値で指定してください。");
            }
        }
    }

    /// <summary>
    /// ラベルジャンプコマンド
    /// <para>
    /// example:<br/>
    /// @goto, EndLabel
    /// </para>
    /// </summary>
    public class GotoChatCommand : ChatCommand
    {
        public override CommandType Type => CommandType.Goto;
        public string GotoLabel { get; }

        public GotoChatCommand(int index, string[] args) : base(index, args)
        {
            GotoLabel = Args[1];
        }

        public override void Check()
        {
            if (Args.Length != 2)
            {
                throw new ChatCommandException(this, "必要な引数が不足か多いです。\nex: @goto, <ラベル名>");
            }

            if (string.IsNullOrWhiteSpace(Args[1]))
            {
                throw new ChatCommandException(this, "ラベル名が空です。\nex: @goto, <ラベル名>");
            }
        }
    }
}
