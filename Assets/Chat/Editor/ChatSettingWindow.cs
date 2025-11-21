#nullable enable

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.SceneTemplate;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatSettingWindow : EditorWindow
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
    private string[]? validationMessages;

    [MenuItem("Window/Chat/Settings")]
    public static void ShowWindow()
    {
        var window = GetWindow<ChatSettingWindow>("Chat Settings");
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

    private void DrawAssetsSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Game Assets", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // Description
        EditorGUILayout.LabelField("Description:");
        description = EditorGUILayout.TextField(description, GUILayout.Height(60));
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
            ValidateSetup();
            showValidation = true;
        }

        if (showValidation && validationMessages != null)
        {
            EditorGUILayout.Space(5);
            foreach (var message in validationMessages)
            {
                var messageType = message.StartsWith("✓") ? MessageType.Info :
                                  message.StartsWith("⚠") ? MessageType.Warning :
                                  MessageType.Error;
                EditorGUILayout.HelpBox(message, messageType);
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

    private void CreatePicturesAsset()
    {
        var folderPath = "Assets/ChatAssets";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "ChatAssets");
        }

        var asset = CreateInstance<Pictures>();
        var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/Pictures.asset");

        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        picturesAsset = asset;
    }

    private void CreateSampleScenario()
    {
        var samplePath = "Assets/Chat/Samples/SampleText.txt";
        if (!File.Exists(samplePath))
        {
            EditorUtility.DisplayDialog(
                "Error",
                $"Sample text file not found at:\n{samplePath}",
                "OK"
            );
            return;
        }

        var folderPath = "Assets/ChatAssets";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "ChatAssets");
        }

        var destinationPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/Scenario.txt");
        AssetDatabase.CopyAsset(samplePath, destinationPath);
        AssetDatabase.Refresh();

        scenarioText = AssetDatabase.LoadAssetAtPath<TextAsset>(destinationPath);
    }

    private void CreateTMPFontAsset()
    {
        if (japaneseFont == null)
            return;

        try
        {
            var tmpType = Type.GetType("TMPro.TMP_FontAsset, Unity.TextMeshPro");
            if (tmpType == null)
            {
                EditorUtility.DisplayDialog("Error", "TextMeshPro is not imported.", "OK");
                return;
            }

            // Call TMPro.TMP_FontAsset.CreateFontAsset(Font)
            var createMethod = tmpType.GetMethod(
                "CreateFontAsset",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
                null,
                new Type[] { typeof(Font) },
                null
            );

            if (createMethod == null)
            {
                EditorUtility.DisplayDialog("Error", "CreateFontAsset method not found.", "OK");
                return;
            }

            var fontAsset = createMethod.Invoke(null, new object[] { japaneseFont });

            if (fontAsset == null)
            {
                EditorUtility.DisplayDialog("Error", "Failed to create TMP Font Asset.", "OK");
                return;
            }

            // Save the asset
            var folderPath = "Assets/TextMesh Pro/Fonts";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                if (!AssetDatabase.IsValidFolder("Assets/TextMesh Pro"))
                {
                    AssetDatabase.CreateFolder("Assets", "TextMesh Pro");
                }
                AssetDatabase.CreateFolder("Assets/TextMesh Pro", "Fonts");
            }

            var assetPath = AssetDatabase.GenerateUniqueAssetPath(
                $"{folderPath}/{japaneseFont.name} SDF.asset"
            );

            AssetDatabase.CreateAsset((UnityEngine.Object)fontAsset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Optionally set as default
            if (EditorUtility.DisplayDialog(
                "Success",
                $"TMP Font Asset created at:\n{assetPath}\n\nSet as default font?",
                "Yes",
                "No"
            ))
            {
                SetDefaultTMPFont(fontAsset);
            }

            japaneseFont = null;
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to create TMP Font Asset:\n{ex.Message}", "OK");
        }
    }

    private void SetDefaultTMPFont(object fontAsset)
    {
        try
        {
            var tmpSettingsType = Type.GetType("TMPro.TMP_Settings, Unity.TextMeshPro");
            if (tmpSettingsType == null)
                return;

            var settings = Resources.Load("TMP Settings");
            if (settings == null)
                return;

            var serializedObject = new SerializedObject((UnityEngine.Object)settings);
            var defaultFontProperty = serializedObject.FindProperty("m_defaultFontAsset");

            if (defaultFontProperty != null)
            {
                defaultFontProperty.objectReferenceValue = (UnityEngine.Object)fontAsset;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty((UnityEngine.Object)settings);
                AssetDatabase.SaveAssets();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to set default TMP font: {ex.Message}");
        }
    }

    private void ValidateSetup()
    {
        var messages = new System.Collections.Generic.List<string>();

        // Check description
        if (string.IsNullOrWhiteSpace(description))
        {
            messages.Add("✗ Description is not set.");
        }
        else
        {
            messages.Add("✓ Description is set.");
        }

        // Check character sprite
        if (characterSprite == null)
        {
            messages.Add("✗ Character Sprite is not set.");
        }
        else
        {
            messages.Add("✓ Character Sprite is set.");
        }

        // Check pictures asset
        if (picturesAsset == null)
        {
            messages.Add("✗ Pictures Asset is not set.");
        }
        else if (picturesAsset.pictures.Count == 0)
        {
            messages.Add("⚠ Pictures Asset has no sprites registered.");
        }
        else
        {
            messages.Add($"✓ Pictures Asset has {picturesAsset.pictures.Count} sprite(s).");
        }

        // Check scenario text
        if (scenarioText == null)
        {
            messages.Add("✗ Scenario Text is not set.");
        }
        else
        {
            messages.Add("✓ Scenario Text is set.");
        }

        // Check TMP
        var tmpType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
        if (tmpType == null)
        {
            messages.Add("✗ TextMeshPro is not imported.");
        }
        else
        {
            messages.Add("✓ TextMeshPro is imported.");
        }

        // Check prefabs
        var prefabPaths = new[]
        {
            "Assets/Chat/Prefabs/ChatNode.prefab",
            "Assets/Chat/Prefabs/ImageNode.prefab",
            "Assets/Chat/Prefabs/EndNode.prefab"
        };

        foreach (var path in prefabPaths)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                messages.Add($"✗ Prefab not found: {Path.GetFileName(path)}");
            }
            else
            {
                messages.Add($"✓ Prefab found: {Path.GetFileName(path)}");
            }
        }

        validationMessages = messages.ToArray();
    }

    private void CreateChatScene()
    {
        // Final validation
        ValidateSetup();

        var errors = validationMessages?.Where(m => m.StartsWith("✗")).ToArray();
        if (errors != null && errors.Length > 0)
        {
            EditorUtility.DisplayDialog(
                "Cannot Create Scene",
                "Please fix the following errors:\n\n" + string.Join("\n", errors),
                "OK"
            );
            return;
        }

        // Load Scene Template
        var templatePath = "Assets/Chat/ChatSceneTemplate.scenetemplate";
        var sceneTemplate = AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>(templatePath);

        if (sceneTemplate == null)
        {
            EditorUtility.DisplayDialog(
                "Error",
                $"Scene Template not found at:\n{templatePath}",
                "OK"
            );
            return;
        }

        // Save scene path
        var scenePath = EditorUtility.SaveFilePanelInProject(
            "Save Chat Scene",
            "ChatGame",
            "unity",
            "Save the new chat game scene"
        );

        if (string.IsNullOrEmpty(scenePath))
        {
            EditorUtility.DisplayDialog(
                "Cancelled",
                "Scene creation was cancelled.",
                "OK"
            );
            return;
        }

        // Instantiate scene from template
        var instantiateResult = SceneTemplateService.Instantiate(sceneTemplate, false, scenePath);

        if (instantiateResult == null || instantiateResult.scene == null)
        {
            EditorUtility.DisplayDialog(
                "Error",
                "Failed to create scene from template.",
                "OK"
            );
            return;
        }

        // Find ChatController in the new scene
        var chatController = GameObject.FindFirstObjectByType<ChatController>();

        if (chatController != null)
        {
            // Assign serialized fields using SerializedObject
            var so = new SerializedObject(chatController);

            so.FindProperty("description").stringValue = description;
            so.FindProperty("pictures").objectReferenceValue = picturesAsset;
            so.FindProperty("scenarioText").objectReferenceValue = scenarioText;
            so.FindProperty("defaultIcon").objectReferenceValue = characterSprite;

            so.ApplyModifiedProperties();
        }
        else
        {
            Debug.LogWarning("ChatController not found in the created scene. Please assign assets manually.");
        }

        // Save the scene
        EditorSceneManager.SaveScene(instantiateResult.scene);

        // Add scene to build settings at the top
        var scenesInBuild = EditorBuildSettings.scenes.ToList();
        var newSceneEntry = new EditorBuildSettingsScene(scenePath, true);

        // Remove if already exists (to avoid duplicates)
        scenesInBuild.RemoveAll(s => s.path == scenePath);

        // Insert at the top (index 0)
        scenesInBuild.Insert(0, newSceneEntry);
        EditorBuildSettings.scenes = scenesInBuild.ToArray();

        EditorUtility.DisplayDialog(
            "Success!",
            "Chat game scene has been created!\n\n" +
            $"Scene: {scenePath}\n\n" +
            "You can now enter Play Mode to test your chat game.",
            "OK"
        );
    }
}
