using UnityEngine;

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
