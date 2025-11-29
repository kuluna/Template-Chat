using System.IO;
using UnityEditor;
using UnityEngine;

#nullable enable

/// <summary>
/// Chatアセットの作成を行うファクトリクラス
/// </summary>
internal static class ChatAssetFactory
{
    /// <summary>
    /// Pictures アセットを作成
    /// </summary>
    public static Pictures CreatePicturesAsset()
    {
        EditorUtilities.EnsureFolderExists(EditorConstants.ChatAssetsFolder);

        var asset = ScriptableObject.CreateInstance<Pictures>();
        var assetPath = AssetDatabase.GenerateUniqueAssetPath(
            $"{EditorConstants.ChatAssetsFolder}/Pictures.asset");

        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return asset;
    }

    /// <summary>
    /// サンプルシナリオを作成
    /// </summary>
    public static TextAsset CreateSampleScenario()
    {
        EditorUtilities.EnsureFolderExists(EditorConstants.ChatAssetsFolder);

        var destinationPath = AssetDatabase.GenerateUniqueAssetPath(
            $"{EditorConstants.ChatAssetsFolder}/Scenario.txt");

        // サンプルシナリオの内容
        var sampleContent = "@text, テストだよ！\n" +
                            "@wait, 2\n" +
                            "@text, ねぇ、聞いてる？\n";

        // ファイルを書き込み
        File.WriteAllText(destinationPath, sampleContent);
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath<TextAsset>(destinationPath);
    }
}
