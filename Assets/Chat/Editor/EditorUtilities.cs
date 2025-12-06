using System.IO;
using UnityEditor;

#nullable enable

namespace Template.Chat.Editor
{
    /// <summary>
    /// Editor拡張用の汎用ユーティリティ関数
    /// </summary>
    internal static class EditorUtilities
    {
        /// <summary>
        /// フォルダが存在しない場合は再帰的に作成
        /// </summary>
        public static void EnsureFolderExists(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;

            var parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
            var folderName = Path.GetFileName(folderPath);

            if (parent != null && !string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolderExists(parent);
            }
            AssetDatabase.CreateFolder(parent ?? "Assets", folderName);
        }
    }
}
