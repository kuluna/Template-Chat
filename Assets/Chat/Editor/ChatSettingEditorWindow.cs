using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

#nullable enable

namespace Template.Chat.Editor
{
    /// <summary>
    /// Chat設定用のEditorWindow
    /// UI描画は ChatSettingEditorWindow+UI.cs
    /// アクションは ChatSettingEditorWindow+Actions.cs
    /// </summary>
    public partial class ChatSettingEditorWindow : EditorWindow
    {
        // Asset Settings
        private string description = "";
        private Sprite? characterSprite;
        private Pictures? picturesAsset;
        private TextAsset? scenarioText;

        // TMP Font Settings
        private Font? japaneseFont;

        // UI State
        private Vector2 scrollPosition;
        private bool showValidation = false;
        private ChatSetupValidator.ValidationResult[]? validationResults;

        // Validators
        private readonly ChatSetupValidator validator = new();

        [MenuItem("Window/Chat/Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<ChatSettingEditorWindow>("Chat Template");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawAssetsSection();
            EditorGUILayout.Space(10);

            DrawTMPSection();
            EditorGUILayout.Space(10);

            DrawValidationSection();
            EditorGUILayout.Space(10);

            DrawCreateGameSection();

            EditorGUILayout.EndScrollView();
        }
    }
}
