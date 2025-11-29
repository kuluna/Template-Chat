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
            EditorGUILayout.LabelField("Game Assets", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Description
            EditorGUILayout.LabelField("Description:");
            description = EditorGUILayout.TextArea(description, GUILayout.Height(60));
            EditorGUILayout.Space(5);

            // Character Sprite
            characterSprite = (Sprite?)EditorGUILayout.ObjectField(
                "Character Sprite",
                characterSprite,
                typeof(Sprite),
                false
            );
            EditorGUILayout.Space(5);

            // Pictures Asset
            picturesAsset = (Pictures?)EditorGUILayout.ObjectField(
                "Pictures Asset",
                picturesAsset,
                typeof(Pictures),
                false
            );

            if (picturesAsset == null)
            {
                if (GUILayout.Button("Create Pictures Asset"))
                {
                    CreatePicturesAsset();
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox(
                    $"Registered Pictures: {picturesAsset.pictures.Count}",
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
                "Scenario Text",
                scenarioText,
                typeof(TextAsset),
                false
            );

            if (scenarioText == null)
            {
                if (GUILayout.Button("Create Sample Scenario"))
                {
                    CreateSampleScenario();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawTMPSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("TextMeshPro Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Check if TMP is imported
            var tmpType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            if (tmpType == null)
            {
                EditorGUILayout.HelpBox(
                    "TextMeshPro is not imported. Please import TMP Essential Resources from:\n" +
                    "Window > TextMeshPro > Import TMP Essential Resources",
                    MessageType.Warning
                );
            }
            else
            {
                EditorGUILayout.HelpBox("TextMeshPro is imported.", MessageType.Info);

                // Japanese Font Import Section
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Import Japanese Font", EditorStyles.boldLabel);

                japaneseFont = (Font?)EditorGUILayout.ObjectField(
                    "TrueType Font",
                    japaneseFont,
                    typeof(Font),
                    false
                );

                EditorGUI.BeginDisabledGroup(japaneseFont == null);
                if (GUILayout.Button("Create TMP Font Asset"))
                {
                    CreateTMPFontAsset();
                }
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox(
                    "推奨日本語フォント: Noto Sans JP\n" +
                    "Google Fontsから無料でダウンロード可能です。",
                    MessageType.Info
                );
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawValidationSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if (GUILayout.Button("Validate Setup"))
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
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Create Game", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            bool canCreate = !string.IsNullOrWhiteSpace(description) &&
                             characterSprite != null &&
                             picturesAsset != null &&
                             scenarioText != null;

            if (!canCreate)
            {
                EditorGUILayout.HelpBox(
                    "すべての必須項目を設定してください:\n" +
                    "- Description\n" +
                    "- Character Sprite\n" +
                    "- Pictures Asset\n" +
                    "- Scenario Text",
                    MessageType.Warning
                );
            }

            EditorGUI.BeginDisabledGroup(!canCreate);
            if (GUILayout.Button("Create Chat Game", GUILayout.Height(40)))
            {
                // Delay call to avoid GUI layout errors
                EditorApplication.delayCall += CreateChatScene;
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
        }
    }
}
