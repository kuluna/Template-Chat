using System;
using UnityEditor;

#nullable enable

namespace Template.Chat.Editor
{
    public partial class ChatSettingEditorWindow
    {
        private void CreatePicturesAsset()
        {
            picturesAsset = ChatAssetFactory.CreatePicturesAsset();
        }

        private void CreateSampleScenario()
        {
            scenarioText = ChatAssetFactory.CreateSampleScenario();
        }

        private void ImportTMPEssentialResources()
        {
            var tmpImporterType = Type.GetType("TMPro.EditorUtilities.TMP_PackageResourceImporter, Unity.TextMeshPro.Editor");
            if (tmpImporterType != null)
            {
                var importMethod = tmpImporterType.GetMethod("ImportProjectResourcesMenu",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                importMethod?.Invoke(null, null);

                EditorApplication.delayCall += () =>
                {
                    EditorUtility.DisplayDialog(
                        "インポート完了",
                        "TextMeshPro Essential Resourcesのインポートが完了しました。",
                        "OK"
                    );
                };
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "エラー",
                    "TextMeshProのインポーターが見つかりませんでした。\n" +
                    "Package ManagerからTextMeshProパッケージをインストールしてください。",
                    "OK"
                );
            }
        }

        private void CreateTMPFontAsset()
        {
            if (japaneseFont == null)
                return;

            var creator = new TMPFontAssetCreator();
            if (creator.TryCreateAndSetAsDefault(japaneseFont, out var errorMessage))
            {
                EditorUtility.DisplayDialog(
                    "Success",
                    "The TMP Font Asset has been created and set as the default font.",
                    "OK"
                );
            }
            else
            {
                EditorUtility.DisplayDialog("Error", $"Failed to create TMP Font Asset:\n{errorMessage}", "OK");
            }
        }

        private void CreateChatScene()
        {
            // Final validation
            var results = validator.Validate(description, characterSprite, picturesAsset, scenarioText);

            if (validator.HasErrors(results))
            {
                var errorMessages = validator.GetErrorMessages(results);
                EditorUtility.DisplayDialog(
                    "Cannot Create Scene",
                    "Please fix the following errors:\n\n" + string.Join("\n", errorMessages),
                    "OK"
                );
                return;
            }

            // Chat Prefab を検索
            var chatPrefab = ChatSceneBuilder.FindChatPrefab();
            if (chatPrefab == null)
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    "Chat Prefab not found.\nPlease ensure 'Chat.prefab' exists in the package.",
                    "OK"
                );
                return;
            }

            // シーン保存先を選択
            var scenePath = EditorUtility.SaveFilePanelInProject(
                "Save Chat Scene",
                "ChatGame",
                "unity",
                "Save the new chat game scene"
            );

            if (string.IsNullOrEmpty(scenePath))
            {
                return;
            }

            // シーンを構築
            var builder = new ChatSceneBuilder(description, characterSprite!, picturesAsset!, scenarioText!);
            builder.Build(scenePath, chatPrefab);

            EditorUtility.DisplayDialog(
                "Success!",
                "Chat game scene has been created!\n\n" +
                $"Scene: {scenePath}\n\n" +
                "You can now enter Play Mode to test your chat game.",
                "OK"
            );
        }
    }
}
