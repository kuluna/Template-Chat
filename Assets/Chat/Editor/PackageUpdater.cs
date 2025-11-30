using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Events;

#nullable enable

namespace Template.Chat.Editor
{
    /// <summary>
    /// パッケージの更新を管理するクラス
    /// </summary>
    public class PackageUpdater
    {
        [System.Serializable]
        private class PackageInfoSerializable
        {
            public string version = "";
        }

        private readonly UnityAction? onUpdate;
        private readonly UnityAction? onComplete;

        private static RemoveRequest? removeRequest;
        private static AddRequest? addRequest;

        public PackageUpdater(UnityAction? onUpdate = null, UnityAction? onComplete = null)
        {
            this.onUpdate = onUpdate;
            this.onComplete = onComplete;
        }

        /// <summary>
        /// 現在インストールされているパッケージのバージョンを取得
        /// </summary>
        public string GetPackageVersion()
        {
            var packageJsonPath = $"{EditorConstants.PackagePath}package.json";
            var jsonContent = AssetDatabase.LoadAssetAtPath<TextAsset>(packageJsonPath)?.text;

            if (string.IsNullOrEmpty(jsonContent))
            {
                // ローカル開発時はAssetsフォルダから読み込み
                packageJsonPath = "Assets/Chat/package.json";
                jsonContent = AssetDatabase.LoadAssetAtPath<TextAsset>(packageJsonPath)?.text;
            }

            if (string.IsNullOrEmpty(jsonContent))
            {
                return "不明";
            }

            var json = JsonUtility.FromJson<PackageInfoSerializable>(jsonContent);
            return json?.version ?? "不明";
        }

        /// <summary>
        /// パッケージを最新版に更新
        /// </summary>
        public void UpdatePackage()
        {
            onUpdate?.Invoke();
            removeRequest = Client.Remove(EditorConstants.PackageName);
            EditorApplication.update += RemoveProgress;
        }

        private void RemoveProgress()
        {
            if (removeRequest == null || !removeRequest.IsCompleted)
            {
                return;
            }

            EditorApplication.update -= RemoveProgress;

            if (removeRequest.Status == StatusCode.Failure)
            {
                Debug.LogError($"パッケージの削除に失敗しました: {removeRequest.Error?.message}");
                onComplete?.Invoke();
                return;
            }

            addRequest = Client.Add(EditorConstants.PackageRepository);
            EditorApplication.update += AddProgress;
        }

        private void AddProgress()
        {
            if (addRequest == null || !addRequest.IsCompleted)
            {
                return;
            }

            EditorApplication.update -= AddProgress;

            if (addRequest.Status == StatusCode.Failure)
            {
                Debug.LogError($"パッケージの追加に失敗しました: {addRequest.Error?.message}");
            }

            onComplete?.Invoke();
        }
    }
}
