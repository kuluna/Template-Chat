# ChatSettingWindow リファクタリング計画

## 概要
`ChatSettingWindow.cs`は現在743行の単一ファイルで、複数の責務を持っています。保守性と可読性を向上させるため、以下のリファクタリングを実施します。

## 現状の問題点

### 1. 単一責任の原則違反
- UI描画
- アセット作成
- Prefabコピー
- シーン生成
- TMP設定
- バリデーション

これらすべてが1つのクラスに集約されている。

### 2. 重複コード
- TMPフォント取得ロジックが`CopyRequiredPrefabs()`と`CreateChatScene()`で重複
- Prefab検索ロジックが複数箇所に散在
- リフレクションを使ったTMP操作が複数回記述

### 3. 長大なメソッド
- `CreateChatScene()`: 100行以上
- `CopyRequiredPrefabs()`: 80行以上
- リーダビリティが低下

### 4. ハードコードされた値
- パッケージパス: `"Packages/jp.kuluna.lib.chattemplate/"`
- フォルダパス: `"Assets/ChatAssets"`
- Prefab名: `"ChatNode"`, `"ImageNode"`, `"EndNode"`

## リファクタリング計画

### Phase 1: クラス分割

#### 1.1 `TMPFontManager`クラス
**責務**: TextMeshProのフォント関連操作

```csharp
public class TMPFontManager
{
    public static object? GetDefaultFont();
    public static void SetDefaultFont(object fontAsset);
    public static object? CreateFontAsset(Font font);
    public static void UpdateTextMeshProFont(GameObject target, object fontAsset);
    public static void UpdatePrefabFont(string prefabPath, object fontAsset);
}
```

**メリット**:
- TMPのリフレクション処理を1箇所に集約
- 再利用性向上
- テスト容易性向上

#### 1.2 `PrefabCopyService`クラス
**責務**: Prefabのコピーと管理

```csharp
public class PrefabCopyService
{
    private readonly string packagePath;
    private readonly string destinationPath;
    
    public void CopyRequiredPrefabs(string[] prefabNames);
    public GameObject? FindPrefab(string prefabName);
    private string? FindSourcePrefabPath(string prefabName);
}
```

**メリット**:
- Prefab検索ロジックの統一
- パス管理の一元化

#### 1.3 `ChatSceneBuilder`クラス
**責務**: シーンの構築

```csharp
public class ChatSceneBuilder
{
    public void CreateScene(SceneCreationConfig config);
    private void CreateCamera();
    private void CreateLight2D();
    private void CreateEventSystem();
    private void SetupCanvas(Canvas canvas, Camera camera);
    private void ConfigureChatController(ChatController controller, SceneCreationConfig config);
}
```

**メリット**:
- シーン構築ロジックの分離
- 段階的な構築プロセスの明確化

#### 1.4 `AssetCreationService`クラス
**責務**: Pictures、Scenarioなどのアセット作成

```csharp
public class AssetCreationService
{
    public Pictures CreatePicturesAsset(string folderPath);
    public TextAsset CreateSampleScenario(string folderPath);
}
```

#### 1.5 `ChatSettingValidator`クラス
**責務**: 設定のバリデーション

```csharp
public class ChatSettingValidator
{
    public ValidationResult Validate(ChatSettings settings);
    private void CheckDescription(string description, List<string> messages);
    private void CheckSprite(Sprite? sprite, List<string> messages);
    private void CheckPictures(Pictures? pictures, List<string> messages);
    private void CheckScenario(TextAsset? scenario, List<string> messages);
    private void CheckTMP(List<string> messages);
}
```

### Phase 2: 設定データの構造化

#### 2.1 `ChatSettings`データクラス
```csharp
[Serializable]
public class ChatSettings
{
    public string description;
    public Sprite? characterSprite;
    public Pictures? picturesAsset;
    public TextAsset? scenarioText;
}
```

#### 2.2 `SceneCreationConfig`データクラス
```csharp
public class SceneCreationConfig
{
    public string scenePath;
    public ChatSettings settings;
    public Camera mainCamera;
}
```

### Phase 3: 定数の外部化

#### 3.1 `ChatTemplateConstants`クラス
```csharp
public static class ChatTemplateConstants
{
    public const string PACKAGE_PATH = "Packages/jp.kuluna.lib.chattemplate";
    public const string ASSETS_FOLDER = "Assets/ChatAssets";
    public const string PREFABS_FOLDER = "Assets/ChatAssets/Prefabs";
    
    public static readonly string[] REQUIRED_PREFABS = { "ChatNode", "ImageNode", "EndNode" };
    
    public const string TMP_SETTINGS_PATH = "TMP Settings";
    public const string TMP_FONT_FOLDER = "Assets/TextMesh Pro/Fonts";
}
```

### Phase 4: エラーハンドリングの改善

#### 4.1 カスタム例外クラス
```csharp
public class ChatTemplateException : Exception
{
    public ChatTemplateException(string message) : base(message) { }
}

public class PrefabNotFoundException : ChatTemplateException
{
    public PrefabNotFoundException(string prefabName) 
        : base($"Prefab not found: {prefabName}") { }
}

public class TMPNotImportedException : ChatTemplateException
{
    public TMPNotImportedException() 
        : base("TextMeshPro is not imported.") { }
}
```

### Phase 5: 非同期処理の導入（オプション）

長時間処理（Prefabコピー、シーン作成）に進捗表示を追加:

```csharp
public class ProgressReporter
{
    public void ShowProgress(string title, string message, float progress);
    public void ClearProgress();
}
```

## リファクタリング後の構造

```
Assets/Chat/Editor/
├── ChatSettingWindow.cs           // UI層のみ（200行程度）
├── Services/
│   ├── TMPFontManager.cs          // TMP操作
│   ├── PrefabCopyService.cs       // Prefab管理
│   ├── ChatSceneBuilder.cs        // シーン構築
│   ├── AssetCreationService.cs    // アセット作成
│   └── ChatSettingValidator.cs    // バリデーション
├── Models/
│   ├── ChatSettings.cs            // 設定データ
│   ├── SceneCreationConfig.cs     // シーン作成設定
│   └── ValidationResult.cs        // バリデーション結果
├── Utils/
│   ├── ChatTemplateConstants.cs   // 定数
│   └── ProgressReporter.cs        // 進捗表示
└── Exceptions/
    └── ChatTemplateException.cs   // カスタム例外
```

## 実装順序

1. **Phase 1-1**: `TMPFontManager`の抽出（最も重複が多い）
2. **Phase 3**: 定数の外部化（依存関係が少ない）
3. **Phase 2**: データクラスの作成
4. **Phase 1-2～1-5**: 残りのサービスクラスの抽出
5. **Phase 4**: エラーハンドリングの改善
6. **Phase 5**: 進捗表示の追加（オプション）

## 期待される効果

### コードメトリクス改善
- **現状**: 1ファイル743行
- **目標**: 主要ファイル200行以下、各サービス100行以下

### 保守性向上
- 単一責任の原則に準拠
- 変更の影響範囲が明確
- テストが容易

### 可読性向上
- 各クラスの責務が明確
- メソッド長が短縮
- ビジネスロジックとUI層の分離

### 再利用性向上
- 各サービスが独立して使用可能
- 他のエディタ拡張での再利用が容易

## 注意事項

- リファクタリングは段階的に実施
- 各段階で動作確認を実施
- 既存の機能を破壊しない
- パブリックAPIの後方互換性を維持（可能な限り）

## テスト計画

リファクタリング後、以下のシナリオをテスト:

1. 通常フロー: すべての設定を入力してシーン作成
2. バリデーションエラー: 必須項目未入力
3. Prefabが見つからない場合
4. TMPが未インポートの場合
5. フォント作成とデフォルト設定
6. Packageからのインポート

---

**最終更新**: 2025年11月22日
**担当**: リファクタリング計画
**優先度**: 中（機能追加前に実施推奨）
