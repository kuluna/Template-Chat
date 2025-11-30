# Unity Chat System - Copilot Instructions

## Project Overview
Unityで動作するテキストベースのチャットシステムテンプレート。カスタムコマンドパーサーを使用してテキストファイルからチャット会話を生成します。Unity 6、C# with nullable reference types enabled を使用。

## Architecture

### Core Components
- **ChatParser**: テキストファイルをパースして `IChatCommand` オブジェクトに変換
- **ChatEventPresenter**: コマンド実行のオーケストレーター。`IChatEventListener` インターフェースを通じてUIと通信
- **ChatController**: MonoBehaviourでPresenterとUIをつなぐメインコントローラー。partial classでCommand実装とUI実装を分離
- **ChatNode**: 個々のチャットメッセージのUIプレハブ

### Command System
コマンド形式: `@command, arg1, arg2, ...`

実装済みコマンド:
- `@text, <message>` - テキスト表示 (例: `@text, Hello, world!`)
- `@image, <imageName>` - 画像表示
- `@wait, <seconds>` - 待機 (最大5秒)
- `@choice, <option1>, <option2>, ...` - 選択肢 (最大3つ)
- `@if, <varName>, <expectedValue>, <labelToJump>` - 条件分岐
- `@label, <labelName>` - ジャンプ先ラベル

すべてのコマンドクラスは `Assets/Chat/Scripts/ChatCommand.cs` に定義。各コマンドは `Check()` メソッドでバリデーションを実装し、無効な引数は `ChatCommandException` をスロー。

## Code Conventions

### Partial Classes
`ChatController` は partial class で実装:
- `ChatController.cs` - MonoBehaviourメイン実装、Unity lifecycle、UIリファレンス
- `ChatController+Command.cs` - `IChatEventListener` 実装、コマンド実行ロジック

ファイル名に `+` を使用してpartial classであることを明示。

### Nullable Reference Types
`#nullable enable` ディレクティブを全ファイルで使用。Unity SerializeFieldは `= null!` で初期化を明示:
```csharp
[SerializeField] private ChatNode chatNodePrefab = null!;
[SerializeField] private Sprite? defaultIcon; // nullable
```

### Async Patterns
Unity の `Awaitable` を使用（`Task` ではない）:
```csharp
public async Awaitable ShowText(TextChatCommand command)
{
    // 実装
    await Task.CompletedTask; // 非同期処理を行わなかった場合のみ Task.CompletedTask で終了
}
```

並列実行は `AwaitAll()` ヘルパーで複数の `Awaitable` を待機。

## Development Workflow

### Testing
単体テストは `Assets/Tests/` に配置:
```csharp
[Test]
public void Parseable()
{
    var text = "@text, Hello, world!";
    var command = new TextChatCommand(0, text.Split(','));
    Assert.AreEqual(CommandType.Text, command.Type);
}
```

Unity Editor の Window > General > Test Runner から実行。

### Adding New Commands
1. `Assets/Chat/Scripts/ChatCommand.cs` に新しいコマンドクラスを追加
2. `CommandType` enum に新しい型を追加
3. `ChatParser.ParseLine()` にパースロジックを追加
4. `ChatEventPresenter.ExecuteCommand()` に実行ロジックを追加
5. 必要に応じて `ChatController+Command.cs` にUI処理を実装
6. `Assets/Tests/ChatCommandTest.cs` にテストを追加

### Sample Data
- `Assets/Chat/Samples/SampleText.txt` - チャットシナリオサンプル
- `Assets/Chat/Samples/SamplePictures.asset` - ScriptableObject形式の画像アセット

#### Scenario File Example (`.txt`)
```text
# コメント行はハッシュで開始
@text, こんにちは！これはチャットのテストです。
@wait, 1.5

# 画像を表示
@image, smile_icon
@text, 画像を表示しました。

# 選択肢の表示
@choice, はい, いいえ

# 条件分岐とラベルジャンプ
@if, user_selection, はい, LabelYes
@text, 「いいえ」が選ばれました。
@label, LabelYes
@text, 処理を終了します。
```

## Key Files
- `Assets/Chat/Scripts/ChatController.cs` - メインコントローラー
- `Assets/Chat/Scripts/ChatCommand.cs` - 全コマンド定義 (312行)
- `Assets/Chat/Scripts/ChatParser.cs` - パーサー実装
- `Assets/Chat/Scripts/ChatEventPresenter.cs` - コマンド実行エンジン
- `Assets/Chat/Chat.unity` - メインシーン

## Important Notes
- コメント行は `#` で開始（シナリオテキスト内）
- 連続する `@text` コマンドは自動的に次へ進む（デフォルト0.5秒待機）
- `CanMoveToNext` で再入呼び出しを防止（`AsyncLocal<int>` で深さ追跡）
- UI要素は TextMeshPro を使用
