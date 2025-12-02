# Template Chat

Unityで簡単にチャットをし合えるようなゲームのテンプレートです。

テンプレートを利用してゲームを作る場合は [SETUP.md](docs/SETUP.md) をお読みください。

## 開発環境のセットアップ

### 必要要件

- **Unity**: Unity6以降を推奨

### セットアップ手順

1. **リポジトリをクローン**
```bash
git clone https://github.com/kuluna/Template-Chat.git
```

2. **Unity Hub でプロジェクトを開く**
- Unity Hub を起動
- 「Add」→「Add project from disk」でクローンしたフォルダを選択
- Unity でプロジェクトを開く

3. **サンプルシーンを開く**
- `Assets/Samples/Chat.unity` を開いてプレイモードで動作確認

## プロジェクト構成

```
Assets/
├── Chat/                    # UPM パッケージ本体
│   ├── Scripts/             # コアスクリプト
│   ├── Prefabs/             # UI プレハブ
│   ├── Editor/              # エディタ拡張
├── Samples/                 # サンプルシーン・アセット
└── Tests/                   # テスト
```

### コアコンポーネント

| コンポーネント | 責務 | ファイル |
|---|---|---|
| **ChatParser** | テキスト→コマンド変換、行インデックス管理 | `Scripts/ChatParser.cs` |
| **ChatEventPresenter** | コマンド実行、変数管理、ラベルジャンプ制御 | `Scripts/ChatEventPresenter.cs` |
| **ChatController** | UI操作、MonoBehaviour lifecycle | `Scripts/ChatController.cs` |
| **Pictures** | 画像アセット管理 (ScriptableObject) | `Scripts/Pictures.cs` |

## コマンドシステム

テキストファイルでシナリオを記述します。形式: `@command, arg1, arg2, ...`

| コマンド | 構文 | 説明 |
|---|---|---|
| `@text` | `@text, <message>` | テキスト表示 |
| `@image` | `@image, <imageName>` | Pictures アセットから画像表示 |
| `@wait` | `@wait, <seconds>` | 待機（最大5秒） |
| `@choice` | `@choice, <varName>, <opt1>, <opt2>, ...` | 選択肢（最大3つ） |
| `@if` | `@if, <varName>, <expected>, <label>` | 条件分岐 |
| `@goto` | `@goto, <label>` | 無条件ジャンプ |
| `@label` | `@label, <labelName>` | ジャンプ先定義 |

### シナリオ例

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

## ライセンス

MIT License - 詳細は [LICENSE](LICENSE) を参照してください。
