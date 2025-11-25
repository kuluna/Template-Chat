#nullable enable

using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

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
        var folderPath = "Assets/ChatAssets";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "ChatAssets");
        }

        var destinationPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/Scenario.txt");

        // Create sample scenario content
        var sampleContent = "@text, テストだよ！\n" +
                            "@wait, 2\n" +
                            "@text, ねぇ、聞いてる？\n";

        // Write the file
        File.WriteAllText(destinationPath, sampleContent);
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

            EditorUtility.DisplayDialog(
                    "Success",
                    "The TMP Font Asset has been created and set as the default font.",
                    "OK"
                );
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to create TMP Font Asset:\n{ex.Message}", "OK");
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

        validationMessages = messages.ToArray();
    }

    private void CopyRequiredPrefabs()
    {
        var folderPath = "Assets/ChatAssets/Prefabs";

        // Create Prefabs folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/ChatAssets"))
        {
            AssetDatabase.CreateFolder("Assets", "ChatAssets");
        }
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/ChatAssets", "Prefabs");
        }

        TMP_FontAsset? defaultFont = null;
        var tmpSettings = Resources.Load("TMP Settings");
        if (tmpSettings != null)
        {
            var so = new SerializedObject(tmpSettings);
            var fontProperty = so.FindProperty("m_defaultFontAsset");
            if (fontProperty != null)
            {
                defaultFont = fontProperty.objectReferenceValue as TMP_FontAsset;
            }
        }

        var prefabNames = new[] { "ChatNode", "ImageNode", "EndNode" };
        var copiedPrefabs = new System.Collections.Generic.List<string>();

        foreach (var prefabName in prefabNames)
        {
            var destinationPath = $"{folderPath}/{prefabName}.prefab";

            // Skip if already exists
            if (File.Exists(destinationPath))
                continue;

            // Find source prefab
            var sourceGuids = AssetDatabase.FindAssets($"t:Prefab {prefabName}");
            string? sourcePath = null;

            foreach (var guid in sourceGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == prefabName && path.StartsWith("Packages/"))
                {
                    sourcePath = path;
                    break;
                }
            }

            // Fallback to direct package path
            if (sourcePath == null)
            {
                sourcePath = $"Packages/jp.kuluna.lib.chattemplate/Prefabs/{prefabName}.prefab";
            }

            // Copy prefab
            if (File.Exists(sourcePath) || AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath) != null)
            {
                AssetDatabase.CopyAsset(sourcePath, destinationPath);
                copiedPrefabs.Add(destinationPath);
            }
            else
            {
                Debug.LogWarning($"Prefab not found: {prefabName}.prefab");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (defaultFont != null)
        {
            foreach (var prefabPath in copiedPrefabs)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                    continue;

                var tmpComponents = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
                if (tmpComponents.Length > 0)
                {
                    foreach (var tmpComponent in tmpComponents)
                    {
                        var so = new SerializedObject(tmpComponent);
                        var fontAssetProperty = so.FindProperty("m_fontAsset");
                        if (fontAssetProperty != null)
                        {
                            fontAssetProperty.objectReferenceValue = defaultFont;
                            so.ApplyModifiedProperties();
                        }
                    }

                    EditorUtility.SetDirty(prefab);
                }
            }

            AssetDatabase.SaveAssets();
        }
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

        // Load Chat Prefab
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab Chat");
        GameObject? chatPrefab = null;

        foreach (var guid in prefabGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == "Chat")
            {
                chatPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                break;
            }
        }

        // Fallback to direct package path if not found
        if (chatPrefab == null)
        {
            chatPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Packages/jp.kuluna.lib.chattemplate/Prefabs/Chat.prefab"
            );
        }

        if (chatPrefab == null)
        {
            EditorUtility.DisplayDialog(
                "Error",
                "Chat Prefab not found.\nPlease ensure 'Chat.prefab' exists in the package.",
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
            return;
        }

        // Copy necessary prefabs to Assets folder
        CopyRequiredPrefabs();

        // Create new empty scene
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Create Camera
        var cameraGO = new GameObject("Main Camera");
        var camera = cameraGO.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.orthographic = true;
        camera.orthographicSize = 5;
        cameraGO.tag = "MainCamera";
        cameraGO.AddComponent<AudioListener>();

        // Create Light2D (if available)
        var light2DType = Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
        if (light2DType != null)
        {
            var lightGO = new GameObject("Global Light 2D");
            var light2D = lightGO.AddComponent(light2DType) as Component;
            if (light2D != null)
            {
                // Set light type to Global using reflection
                var lightTypeProperty = light2DType.GetProperty("lightType");
                if (lightTypeProperty != null)
                {
                    var lightTypeEnum = light2DType.Assembly.GetType("UnityEngine.Rendering.Universal.Light2D+LightType");
                    if (lightTypeEnum != null)
                    {
                        var globalValue = Enum.Parse(lightTypeEnum, "Global");
                        lightTypeProperty.SetValue(light2D, globalValue);
                    }
                }
            }
        }

        // Create EventSystem
        var eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<EventSystem>();
        eventSystemGO.AddComponent<InputSystemUIInputModule>();

        // Instantiate Chat Prefab
        var chatInstance = (GameObject)PrefabUtility.InstantiatePrefab(chatPrefab);

        // Unpack prefab to make it independent
        PrefabUtility.UnpackPrefabInstance(chatInstance, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

        // Find and configure Canvas
        var canvas = chatInstance.GetComponentInChildren<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            canvas.worldCamera = camera;
        }

        // Update TMP font in Canvas hierarchy
        if (canvas != null)
        {
            var tmpSettings = Resources.Load("TMP Settings");
            if (tmpSettings != null)
            {
                var so = new SerializedObject((UnityEngine.Object)tmpSettings);
                var fontProperty = so.FindProperty("m_defaultFontAsset");
                if (fontProperty != null && fontProperty.objectReferenceValue != null)
                {
                    var defaultFont = fontProperty.objectReferenceValue as TMP_FontAsset;
                    if (defaultFont != null)
                    {
                        var tmpComponents = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
                        foreach (var tmpComponent in tmpComponents)
                        {
                            var tmpSO = new SerializedObject(tmpComponent);
                            var fontAssetProperty = tmpSO.FindProperty("m_fontAsset");
                            if (fontAssetProperty != null)
                            {
                                fontAssetProperty.objectReferenceValue = defaultFont;
                                tmpSO.ApplyModifiedProperties();
                            }
                        }
                    }
                }
            }
        }

        // Find ChatController and assign assets
        var chatController = chatInstance.GetComponentInChildren<ChatController>();
        if (chatController != null)
        {
            var so = new SerializedObject(chatController);
            so.FindProperty("description").stringValue = description;
            so.FindProperty("pictures").objectReferenceValue = picturesAsset;
            so.FindProperty("scenarioText").objectReferenceValue = scenarioText;
            so.FindProperty("defaultIcon").objectReferenceValue = characterSprite;
            so.ApplyModifiedProperties();
        }
        else
        {
            Debug.LogWarning("ChatController not found in the Chat prefab. Please assign assets manually.");
        }

        // Save the scene
        EditorSceneManager.SaveScene(newScene, scenePath);

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
