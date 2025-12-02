using System;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace Template.Chat.Editor
{
    public partial class ChatSettingEditorWindow
    {
        private void DrawAssetsSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("チャットテンプレート", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                // Update this Library
                if (GUILayout.Button("バージョンアップを確認"))
                {
                    var updater = new PackageUpdater(
                        onUpdate: () => Debug.Log("パッケージを更新中..."),
                        onComplete: () => Debug.Log("更新処理が完了しました")
                    );
                    var currentVersion = updater.GetPackageVersion();
                    updater.UpdatePackage();
                }
            }

            EditorGUILayout.Space(5);

            // Description
            EditorGUILayout.LabelField("あらすじ");
            description = EditorGUILayout.TextArea(description, GUILayout.Height(60));
            EditorGUILayout.Space(5);

            // Character Sprite
            characterSprite = (Sprite?)EditorGUILayout.ObjectField(
                "キャラクター画像",
                characterSprite,
                typeof(Sprite),
                false
            );
            EditorGUILayout.Space(5);

            // Pictures Asset
            picturesAsset = (Pictures?)EditorGUILayout.ObjectField(
                "画像リスト",
                picturesAsset,
                typeof(Pictures),
                false
            );

            if (picturesAsset == null)
            {
                if (GUILayout.Button("画像リストを新規作成"))
                {
                    CreatePicturesAsset();
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox(
                    $"登録済みの画像: {picturesAsset.pictures.Count}枚",
                    MessageType.Info
                );

                if (GUILayout.Button("Inspectorで開く"))
                {
                    Selection.activeObject = picturesAsset;
                    EditorGUIUtility.PingObject(picturesAsset);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space(5);

            // Scenario Text
            scenarioText = (TextAsset?)EditorGUILayout.ObjectField(
                "シナリオテキスト",
                scenarioText,
                typeof(TextAsset),
                false
            );

            if (scenarioText == null)
            {
                if (GUILayout.Button("サンプルシナリオを作成"))
                {
                    CreateSampleScenario();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawTMPSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("テキスト表示の設定（TextMeshPro）", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Check if TMP Essential Resources are imported
            var tmpResourcesExists = System.IO.File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset");

            // TMP Import Button
            EditorGUI.BeginDisabledGroup(tmpResourcesExists);
            if (GUILayout.Button(tmpResourcesExists ? "TextMeshPro インポート済み ✓" : "TextMeshPro Essential Resourcesをインポート"))
            {
                ImportTMPEssentialResources();
            }
            EditorGUI.EndDisabledGroup();

            if (!tmpResourcesExists)
            {
                EditorGUILayout.HelpBox(
                    "まず上のボタンでTextMeshProリソースをインポートしてください。",
                    MessageType.Info
                );
            }

            EditorGUILayout.Space(5);

            // Japanese Font Import Section
            EditorGUILayout.LabelField("日本語フォントの設定", EditorStyles.boldLabel);

            japaneseFont = (Font?)EditorGUILayout.ObjectField(
                "フォントファイル（.ttf/.otf）",
                japaneseFont,
                typeof(Font),
                false
            );

            EditorGUI.BeginDisabledGroup(japaneseFont == null || !tmpResourcesExists);
            if (GUILayout.Button("TMPフォントアセットを作成"))
            {
                CreateTMPFontAsset();
            }
            EditorGUI.EndDisabledGroup();

            if (!tmpResourcesExists && japaneseFont != null)
            {
                EditorGUILayout.HelpBox(
                    "TMPフォントアセットを作成するには、先にTextMeshProをインポートしてください。",
                    MessageType.Warning
                );
            }

            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "推奨日本語フォント: Noto Sans JP\n" +
                "Google Fontsから無料でダウンロード可能です。",
                MessageType.Info
            );

            EditorGUILayout.EndVertical();
        }

        private void DrawValidationSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("設定チェック", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if (GUILayout.Button("設定内容を確認する"))
            {
                validationResults = validator.Validate(description, characterSprite, picturesAsset, scenarioText);
                showValidation = true;
            }

            if (showValidation && validationResults != null)
            {
                EditorGUILayout.Space(5);
                foreach (var result in validationResults)
                {
                    var messageType = result.Severity switch
                    {
                        ChatSetupValidator.ValidationSeverity.Success => MessageType.Info,
                        ChatSetupValidator.ValidationSeverity.Warning => MessageType.Warning,
                        _ => MessageType.Error
                    };
                    EditorGUILayout.HelpBox(result.Message, messageType);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawCreateGameSection()
        {
            bool canCreate = !string.IsNullOrWhiteSpace(description) &&
                             characterSprite != null &&
                             picturesAsset != null &&
                             scenarioText != null;

            if (!canCreate)
            {
                EditorGUILayout.HelpBox(
                    "すべての必須項目を設定してください:\n" +
                    "- ゲームの説明文\n" +
                    "- キャラクター画像\n" +
                    "- 画像アセット\n" +
                    "- シナリオテキスト",
                    MessageType.Warning
                );
            }

            EditorGUI.BeginDisabledGroup(!canCreate);
            if (GUILayout.Button("ゲームを作成する", GUILayout.Height(40)))
            {
                // Delay call to avoid GUI layout errors
                EditorApplication.delayCall += CreateChatScene;
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
