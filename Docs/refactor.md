# ChatSettingEditorWindow リファクタリング計画

## 📊 現状分析

### ファイル概要
- **ファイル**: `Assets/Chat/Editor/ChatSettingEditorWindow.cs`
- **行数**: 約500行
- **責務**: Editorウィンドウ全体（UI描画、アセット作成、シーン生成、TMP設定、バリデーション）

---

## 🔴 コードの問題点一覧

### 1. 単一責任原則 (SRP) の違反

| 問題箇所 | 説明 |
|---------|------|
| `CreateTMPFontAsset` (~90行) | TMPフォント作成ロジックが複雑すぎる。リフレクションを多用 |
| `CreateChatScene` (~120行) | シーン作成、オブジェクト生成、ビルド設定など複数責務 |
| `CopyRequiredPrefabs` (~50行) | プレハブコピーとTMPフォント適用が混在 |
| `ValidateSetup` | バリデーションロジックがUIと密結合 |

### 2. メソッドの長さと複雑さ

| メソッド | 行数 | 問題点 |
|---------|------|--------|
| `CreateTMPFontAsset` | ~90行 | 複雑なリフレクション、ネストが深い |
| `CreateChatScene` | ~120行 | 15以上の異なる操作を含む |
| `CopyRequiredPrefabs` | ~50行 | ループ内でファイル操作とアセット操作 |
| `DrawTMPSection` | ~55行 | 許容範囲だがさらに分割可能 |

### 3. マジックストリング（環境依存パスのみ対象）

```csharp
// 定数化対象（環境で変更される可能性あり）
"Assets/ChatAssets"                                    // 3箇所で使用
"Assets/ChatAssets/Prefabs"                            // 1箇所
"Assets/TextMesh Pro/Fonts"                            // 1箇所
"Packages/jp.kuluna.lib.chattemplate/Prefabs"         // 2箇所
new[] { "ChatNode", "ImageNode", "EndNode", "ChoiceButton" }  // プレハブ名

// 定数化不要（TMPの型名は変更されない）
// "TMPro.TextMeshProUGUI, Unity.TextMeshPro"
// "TMPro.TMP_FontAsset, Unity.TextMeshPro"

// 定数化不要（UIの微調整値は現状維持）
// new Vector2(400, 600), GUILayout.Height(60), etc.
```

### 4. 重複コード

```csharp
// フォルダ作成ロジックの重複（3箇所）
var folderPath = "Assets/ChatAssets";
if (!AssetDatabase.IsValidFolder(folderPath))
{
    AssetDatabase.CreateFolder("Assets", "ChatAssets");
}

// TMP型チェックの重複（2箇所）
var tmpType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
if (tmpType == null) { ... }

// プレハブ検索パターンの重複（2箇所）
var prefabGuids = AssetDatabase.FindAssets("t:Prefab XXX");
foreach (var guid in prefabGuids) { ... }
```

### 5. エラーハンドリングの問題

| 問題 | 箇所 |
|------|------|
| `Debug.LogWarning` のみで処理続行 | `CopyRequiredPrefabs` |
| `null` 代入後の条件分岐 | `sourcePath ??=` パターン |
| ユーザーへのフィードバック不足 | プレハブが見つからない場合 |

### 6. 依存関係の問題

- **TMP_Settings への直接依存**: `TMP_Settings.defaultFontAsset` への直接アクセス
- **リフレクションの過度な使用**: `CreateTMPFontAsset` 内で多数のリフレクション
- **SerializedObject の直接操作**: `ChatController` のプロパティ設定

---

## 📦 機能グルーピング

### Group A: UI描画関連
```
├── OnGUI()
├── DrawAssetsSection()
├── DrawTMPSection()
├── DrawValidationSection()
└── DrawCreateGameSection()
```

### Group B: アセット作成関連
```
├── CreatePicturesAsset()
├── CreateSampleScenario()
└── CopyRequiredPrefabs()
```

### Group C: TMP関連
```
└── CreateTMPFontAsset()
    ├── フォントアセット生成
    ├── アトラステクスチャ追加
    ├── マテリアル追加
    └── デフォルトフォント設定
```

### Group D: バリデーション関連
```
└── ValidateSetup()
    ├── Description チェック
    ├── CharacterSprite チェック
    ├── PicturesAsset チェック
    ├── ScenarioText チェック
    └── TMP チェック
```

### Group E: シーン作成関連
```
└── CreateChatScene()
    ├── バリデーション実行
    ├── Chat Prefab 検索・読み込み
    ├── シーン保存ダイアログ
    ├── プレハブコピー
    ├── シーン作成
    ├── カメラ作成
    ├── Light2D 作成
    ├── EventSystem 作成
    ├── Chat インスタンス化
    ├── TMP フォント適用
    ├── Canvas 設定
    ├── ChatController 設定
    ├── シーン保存
    └── ビルド設定更新
```

---

## 🔧 リファクタリングステップ

### Phase 1: 定数・ヘルパーの抽出 ⭐⭐⭐ (低リスク・即効性あり)

#### Step 1.1: 定数クラスの作成
**新規ファイル**: `EditorConstants.cs`

```csharp
#nullable enable

/// <summary>
/// Editor拡張で使用する環境依存のパス定数
/// </summary>
internal static class EditorConstants
{
    // ユーザーアセットの出力先
    public const string ChatAssetsFolder = "Assets/ChatAssets";
    public const string ChatAssetsPrefabsFolder = "Assets/ChatAssets/Prefabs";
    public const string TMPFontsFolder = "Assets/TextMesh Pro/Fonts";
    
    // パッケージ内のプレハブパス
    public const string PackagePrefabsPath = "Packages/jp.kuluna.lib.chattemplate/Prefabs";
    
    // 必要なプレハブ名
    public static readonly string[] RequiredPrefabNames = 
        { "ChatNode", "ImageNode", "EndNode", "ChoiceButton" };
}
```

#### Step 1.2: ヘルパーメソッドの抽出
**追加先**: `EditorConstants.cs` 内に static メソッドとして追加

```csharp
internal static class EditorConstants
{
    // ... 定数定義 ...
    
    /// <summary>フォルダが存在しない場合は作成</summary>
    public static void EnsureFolderExists(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath)) return;
        
        var parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
        var folderName = Path.GetFileName(folderPath);
        
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolderExists(parent);
        }
        AssetDatabase.CreateFolder(parent ?? "Assets", folderName);
    }
    
    /// <summary>名前でプレハブを検索（パッケージ内フォールバック付き）</summary>
    public static GameObject? FindPrefabByName(string prefabName)
    {
        // まずプロジェクト内を検索
        var guids = AssetDatabase.FindAssets($"t:Prefab {prefabName}");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == prefabName)
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }
        
        // パッケージ内をフォールバック
        var packagePath = $"{PackagePrefabsPath}/{prefabName}.prefab";
        return AssetDatabase.LoadAssetAtPath<GameObject>(packagePath);
    }
}
```

**確認項目**:
- [ ] コンパイルエラーがないこと
- [ ] 既存の機能が正常に動作すること

---

### Phase 2: バリデーション・ファクトリの分離 ⭐⭐

#### Step 2.1: バリデーションクラスの抽出
**新規ファイル**: `ChatSetupValidator.cs`

```csharp
internal class ChatSetupValidator
{
    public enum ValidationSeverity { Success, Warning, Error }
    
    public record ValidationResult(string Message, ValidationSeverity Severity);
    
    public ValidationResult[] Validate(
        string description,
        Sprite? characterSprite,
        Pictures? picturesAsset,
        TextAsset? scenarioText)
    {
        var results = new List<ValidationResult>();
        
        // Description チェック
        results.Add(string.IsNullOrWhiteSpace(description)
            ? new("✗ Description is not set.", ValidationSeverity.Error)
            : new("✓ Description is set.", ValidationSeverity.Success));
        
        // ... 他のチェック
        
        return results.ToArray();
    }
    
    public bool HasErrors(ValidationResult[] results) =>
        results.Any(r => r.Severity == ValidationSeverity.Error);
}
```

#### Step 2.2: アセットファクトリの抽出
**新規ファイル**: `ChatAssetFactory.cs`

```csharp
internal static class ChatAssetFactory
{
    public static Pictures CreatePicturesAsset()
    {
        EditorUtilities.EnsureFolderExists(EditorConstants.ChatAssetsFolder);
        
        var asset = ScriptableObject.CreateInstance<Pictures>();
        var assetPath = AssetDatabase.GenerateUniqueAssetPath(
            $"{EditorConstants.ChatAssetsFolder}/Pictures.asset");
        
        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        
        return asset;
    }
    
    public static TextAsset CreateSampleScenario() { ... }
}
```

**確認項目**:
- [ ] Pictures Asset の作成が正常に動作すること
- [ ] Sample Scenario の作成が正常に動作すること
- [ ] Validation が正しく実行されること

---

### Phase 3: 複雑メソッドの分割 ⭐

#### Step 3.1: TMPフォント作成の分離
**新規ファイル**: `TMPFontAssetCreator.cs`

```csharp
internal class TMPFontAssetCreator
{
    public bool TryCreateFontAsset(Font font, out string? errorMessage)
    {
        errorMessage = null;
        // リフレクションによるフォントアセット作成
        // ...
        return true;
    }
    
    private void AddAtlasTexturesAsSubAssets(object fontAsset, string assetPath) { ... }
    private void AddMaterialAsSubAsset(object fontAsset, string assetPath) { ... }
    public void SetAsDefaultFont(UnityEngine.Object fontAsset) { ... }
}
```

#### Step 3.2: シーン作成の分離
**新規ファイル**: `ChatSceneBuilder.cs`

```csharp
internal class ChatSceneBuilder
{
    private readonly string description;
    private readonly Sprite characterSprite;
    private readonly Pictures picturesAsset;
    private readonly TextAsset scenarioText;
    
    public ChatSceneBuilder(
        string description,
        Sprite characterSprite,
        Pictures picturesAsset,
        TextAsset scenarioText) { ... }
    
    public bool Build(string scenePath)
    {
        CopyRequiredPrefabs();
        CreateScene();
        CreateCamera();
        CreateLight2D();
        CreateEventSystem();
        InstantiateChatPrefab();
        ConfigureChatController();
        SaveScene(scenePath);
        UpdateBuildSettings(scenePath);
        return true;
    }
    
    private void CreateCamera() { ... }
    private void CreateLight2D() { ... }
    private void CreateEventSystem() { ... }
    // ...
}
```

**確認項目**:
- [ ] TMPフォントアセットの作成が正常に動作すること
- [ ] デフォルトフォントの設定が正常に動作すること
- [ ] シーン作成が正常に動作すること
- [ ] ビルド設定への追加が正常に動作すること

---

### Phase 4: Partial Class による整理 ⭐

プロジェクト規約に従い、`+` を使用したファイル名で partial class を分離

#### Step 4.1: UI描画の分離
**新規ファイル**: `ChatSettingEditorWindow+UI.cs`

```csharp
public partial class ChatSettingEditorWindow
{
    private void DrawAssetsSection() { ... }
    private void DrawTMPSection() { ... }
    private void DrawValidationSection() { ... }
    private void DrawCreateGameSection() { ... }
}
```

#### Step 4.2: アクションの分離
**新規ファイル**: `ChatSettingEditorWindow+Actions.cs`

```csharp
public partial class ChatSettingEditorWindow
{
    private void OnCreatePicturesAsset() { ... }
    private void OnCreateSampleScenario() { ... }
    private void OnCreateTMPFontAsset() { ... }
    private void OnValidateSetup() { ... }
    private void OnCreateChatScene() { ... }
}
```

**確認項目**:
- [ ] 全機能が正常に動作すること
- [ ] コンパイルエラーがないこと

---

## 📋 実装順序サマリー

| 順序 | ステップ | 所要時間目安 | リスク |
|------|---------|-------------|--------|
| 1 | Step 1.1 定数クラス作成 | 30分 | 低 |
| 2 | Step 1.2 ヘルパーメソッド抽出 | 1時間 | 低 |
| 3 | Step 2.1 バリデーション分離 | 1時間 | 低 |
| 4 | Step 2.2 アセットファクトリ分離 | 1時間 | 低 |
| 5 | Step 3.1 TMPフォント作成分離 | 2時間 | 中 |
| 6 | Step 3.2 シーン作成分離 | 3時間 | 中 |
| 7 | Step 4.1 UI描画の分離 | 1時間 | 低 |
| 8 | Step 4.2 アクションの分離 | 1時間 | 低 |

---

## 📁 リファクタリング後の想定ファイル構成

```
Assets/Chat/Editor/
├── ChatSettingEditorWindow.cs        // メインウィンドウ (~100行)
├── ChatSettingEditorWindow+UI.cs     // UI描画 (~80行)
├── ChatSettingEditorWindow+Actions.cs // アクション (~60行)
├── ChatSetupValidator.cs             // バリデーション (~60行)
├── ChatAssetFactory.cs               // アセット作成 (~80行)
├── ChatSceneBuilder.cs               // シーン作成 (~150行)
├── TMPFontAssetCreator.cs            // TMPフォント作成 (~100行)
└── EditorConstants.cs                // 定数・ヘルパー (~50行)
```

---

## ⚠️ 注意事項

1. **Unity Editor APIの制約**: `AssetDatabase` などはメインスレッドでのみ使用可能
2. **リフレクション依存**: TMP関連のリフレクションは Unity バージョンアップで壊れる可能性あり
3. **既存の `ChatController` との整合性**: partial class のファイル命名規則 (`+`) に従う
4. **各フェーズ後の確認**: コンパイル → エディタ起動 → 機能テスト の順で確認

---

## ✅ 進捗チェックリスト

- [x] Phase 1: 定数・ヘルパーの抽出
  - [x] Step 1.1: 定数クラス作成
  - [x] Step 1.2: ヘルパーメソッド抽出
  - [x] 動作確認完了
- [x] Phase 2: バリデーション・ファクトリの分離
  - [x] Step 2.1: バリデーションクラス抽出
  - [x] Step 2.2: アセットファクトリ抽出
  - [x] 動作確認完了
- [x] Phase 3: 複雑メソッドの分割
  - [x] Step 3.1: TMPフォント作成分離
  - [x] Step 3.2: シーン作成分離
  - [x] 動作確認完了
- [x] Phase 4: Partial Class による整理
  - [x] Step 4.1: UI描画の分離
  - [x] Step 4.2: アクションの分離
  - [x] 最終動作確認完了
