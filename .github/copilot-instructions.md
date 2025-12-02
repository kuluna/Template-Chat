# Unity Chat System - Copilot Instructions

## Project Overview
Unity 6 (6000.2.x) で動作するテキストベースのチャットシステムテンプレート。テキストファイルからチャット会話を生成するカスタムコマンドパーサーを採用。UPM パッケージとして配布可能（`jp.kuluna.lib.chattemplate`）。

## Architecture

### Data Flow
```
TextAsset (.txt) → ChatParser → IChatCommand[] → ChatEventPresenter → IChatEventListener → ChatController (UI)
```

### Core Components
| コンポーネント | 責務 | ファイル |
|---|---|---|
| **ChatParser** | テキスト→コマンド変換、行インデックス管理 | `Assets/Chat/Scripts/ChatParser.cs` |
| **ChatEventPresenter** | コマンド実行、変数管理、ラベルジャンプ制御 | `Assets/Chat/Scripts/ChatEventPresenter.cs` |
| **ChatController** | UI操作、MonoBehaviour lifecycle | `Assets/Chat/Scripts/ChatController.cs` + `ChatController+Command.cs` |
| **Pictures** | 画像アセット管理 (ScriptableObject) | `Assets/Chat/Scripts/Pictures.cs` |

### Command System
コマンド形式: `@command, arg1, arg2, ...`

| コマンド | 構文 | 説明 |
|---|---|---|
| `@text` | `@text, <message>` | テキスト表示（カンマ含むメッセージOK） |
| `@image` | `@image, <imageName>` | Pictures アセットから画像表示 |
| `@wait` | `@wait, <seconds>` | 待機（最大5秒） |
| `@choice` | `@choice, <varName>, <opt1>, <opt2>, ...` | 選択肢（最大3つ、結果は変数に保存） |
| `@if` | `@if, <varName>, <expected>, <label>` | 条件分岐（文字列/数値/bool比較対応） |
| `@goto` | `@goto, <label>` | 無条件ジャンプ |
| `@label` | `@label, <labelName>` | ジャンプ先定義 |

**@if 比較演算**: `=20`, `>20`, `<20` で数値比較、`true`/`false` でbool比較

## Code Conventions

### Partial Class Pattern
`ChatController` は `+` 記法で分離:
- `ChatController.cs` - MonoBehaviour、SerializeField、lifecycle
- `ChatController+Command.cs` - `IChatEventListener` 実装

### Nullable Reference Types
全ファイルで `#nullable enable`:
```csharp
[SerializeField] private ChatNode chatNodePrefab = null!;  // 必須
[SerializeField] private Sprite? defaultIcon;              // nullable
```

### Async Pattern
Unity の `Awaitable` を使用（`Task` ではない）:
```csharp
public async Awaitable ShowText(TextChatCommand command)
{
    await Awaitable.EndOfFrameAsync();  // フレーム待機
    await Awaitable.WaitForSecondsAsync(0.5f);  // 秒待機
}
```

### Command Validation
各コマンドは `Check()` でバリデーション、失敗時は `ChatCommandException`:
```csharp
public override void Check()
{
    if (Args.Length < 2)
        throw new ChatCommandException(this, "必要な引数が不足しています。");
}
```

## Development Workflow

### Adding New Commands
1. `ChatCommand.cs`: `CommandType` enum に追加 → コマンドクラス作成（`ChatCommand` 継承）
2. `ChatParser.ParseLine()`: switch に新コマンドを追加
3. `ChatEventPresenter.ExecuteCommand()`: 実行ロジック追加
4. `ChatController+Command.cs`: UI処理（必要に応じて `IChatEventListener` に追加）
5. `Assets/Tests/ChatCommandTest.cs`: ネストクラスでテスト追加

### Testing
```bash
# Unity Test Runner (EditMode) で実行
Window > General > Test Runner
```

テストパターン:
```csharp
public class Text  // コマンド名でネストクラス
{
    [Test]
    public void Parseable() { ... }
    [Test]
    public void InvalidTextCommand_ThrowsException() { ... }
}
```

### Scenario File Format
```text
# コメント行
@text, こんにちは！
@choice, user_answer, はい, いいえ
@if, user_answer, はい, LabelYes
@text, 「いいえ」が選ばれました。
@goto, End
@label, LabelYes
@text, 「はい」が選ばれました。
@label, End
```

## Key Files
- `Assets/Chat/Scripts/ChatCommand.cs` - 全コマンド定義（約410行）
- `Assets/Chat/Scripts/ChatEventPresenter.cs` - 実行エンジン、`IChatEventListener` インターフェース
- `Assets/Samples/Chat.unity` - メインシーン
- `Assets/Samples/SampleText.txt` - シナリオ例
- `Assets/Chat/package.json` - UPM パッケージ定義

## Important Behaviors
- 連続 `@text` は自動で0.5秒待機
- `CanMoveToNext` で再入防止（`AsyncLocal<int>` で深さ追跡）
- ラベルは最初に見つかったものを優先
- UI は TextMeshPro 使用
