using UnityEngine;

#nullable enable

/// <summary>
/// Editor拡張で使用する環境依存のパス定数

#nullable enable

namespace Template.Chat.Editor
{
    internal static class EditorConstants
    {
        // パッケージ情報
        public const string PackageName = "jp.kuluna.lib.chattemplate";
        public static readonly string PackagePath = $"Packages/{PackageName}/";
        public static readonly string PackageRepository =
            $"https://github.com/kuluna/Template-Chat.git?path=/Assets/Chat#main";

        // ユーザーアセットの出力先
        public const string ChatAssetsFolder = "Assets/ChatAssets";
        public const string ChatAssetsPrefabsFolder = "Assets/ChatAssets/Prefabs";
        public const string TMPFontsFolder = "Assets/TextMesh Pro/Fonts";

        // パッケージ内のプレハブパス
        public static readonly string PackagePrefabsPath = $"{PackagePath}Prefabs";

        // コピーするプレハブ名
        public static readonly string[] RequiredPrefabNames =
            { "ChatNode", "ImageNode", "EndNode", "ChoiceButton" };
    }
}
