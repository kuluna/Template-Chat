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

/// <summary>
/// Chatシーンの構築を行うクラス
/// </summary>
internal class ChatSceneBuilder
{
    private readonly string description;
    private readonly Sprite characterSprite;
    private readonly Pictures picturesAsset;
    private readonly TextAsset scenarioText;

    public ChatSceneBuilder(
        string description,
        Sprite characterSprite,
        Pictures picturesAsset,
        TextAsset scenarioText)
    {
        this.description = description;
        this.characterSprite = characterSprite;
        this.picturesAsset = picturesAsset;
        this.scenarioText = scenarioText;
    }

    /// <summary>
    /// Chatプレハブを検索して取得
    /// </summary>
    public static GameObject? FindChatPrefab()
    {
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab Chat");

        foreach (var guid in prefabGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == "Chat")
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }

        // パッケージ内をフォールバック
        return AssetDatabase.LoadAssetAtPath<GameObject>(
            $"{EditorConstants.PackagePrefabsPath}/Chat.prefab"
        );
    }

    /// <summary>
    /// シーンを構築して保存
    /// </summary>
    public bool Build(string scenePath, GameObject chatPrefab)
    {
        // 必要なプレハブをコピー
        CopyRequiredPrefabs();

        // 新規シーンを作成
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 各オブジェクトを作成
        var camera = CreateCamera();
        CreateLight2D();
        CreateEventSystem();

        // Chatプレハブをインスタンス化
        var chatInstance = InstantiateChatPrefab(chatPrefab, camera);

        // ChatControllerを設定
        ConfigureChatController(chatInstance);

        // シーンを保存
        EditorSceneManager.SaveScene(newScene, scenePath);

        // ビルド設定を更新
        UpdateBuildSettings(scenePath);

        return true;
    }

    private Camera CreateCamera()
    {
        var cameraGO = new GameObject("Main Camera");
        var camera = cameraGO.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.orthographic = true;
        camera.orthographicSize = 5;
        cameraGO.tag = "MainCamera";
        cameraGO.AddComponent<AudioListener>();
        return camera;
    }

    private void CreateLight2D()
    {
        var light2DType = Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
        if (light2DType == null) return;

        var lightGO = new GameObject("Global Light 2D");
        var light2D = lightGO.AddComponent(light2DType) as Component;
        if (light2D == null) return;

        // リフレクションでライトタイプをGlobalに設定
        var lightTypeProperty = light2DType.GetProperty("lightType");
        if (lightTypeProperty == null) return;

        var lightTypeEnum = light2DType.Assembly.GetType("UnityEngine.Rendering.Universal.Light2D+LightType");
        if (lightTypeEnum == null) return;

        var globalValue = Enum.Parse(lightTypeEnum, "Global");
        lightTypeProperty.SetValue(light2D, globalValue);
    }

    private void CreateEventSystem()
    {
        var eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<EventSystem>();
        eventSystemGO.AddComponent<InputSystemUIInputModule>();
    }

    private GameObject InstantiateChatPrefab(GameObject chatPrefab, Camera camera)
    {
        var chatInstance = (GameObject)PrefabUtility.InstantiatePrefab(chatPrefab);

        // プレハブを展開して独立させる
        PrefabUtility.UnpackPrefabInstance(chatInstance, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

        // TMPコンポーネントにデフォルトフォントを設定
        var texts = chatInstance.GetComponentsInChildren<TextMeshProUGUI>(true);
        if (texts != null)
        {
            foreach (var text in texts)
            {
                text.font = TMP_Settings.defaultFontAsset;
            }
        }

        // Canvasを設定
        var canvas = chatInstance.GetComponentInChildren<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            canvas.worldCamera = camera;
        }

        return chatInstance;
    }

    private void ConfigureChatController(GameObject chatInstance)
    {
        var chatController = chatInstance.GetComponentInChildren<ChatController>();
        if (chatController == null)
        {
            Debug.LogWarning("ChatController not found in the Chat prefab. Please assign assets manually.");
            return;
        }

        var so = new SerializedObject(chatController);
        so.FindProperty("description").stringValue = description;
        so.FindProperty("pictures").objectReferenceValue = picturesAsset;
        so.FindProperty("scenarioText").objectReferenceValue = scenarioText;
        so.FindProperty("defaultIcon").objectReferenceValue = characterSprite;
        so.ApplyModifiedProperties();
    }

    private void UpdateBuildSettings(string scenePath)
    {
        var scenesInBuild = EditorBuildSettings.scenes.ToList();
        var newSceneEntry = new EditorBuildSettingsScene(scenePath, true);

        // 重複を削除
        scenesInBuild.RemoveAll(s => s.path == scenePath);

        // 先頭に追加
        scenesInBuild.Insert(0, newSceneEntry);
        EditorBuildSettings.scenes = scenesInBuild.ToArray();
    }

    private void CopyRequiredPrefabs()
    {
        EditorUtilities.EnsureFolderExists(EditorConstants.ChatAssetsPrefabsFolder);

        foreach (var prefabName in EditorConstants.RequiredPrefabNames)
        {
            var destinationPath = $"{EditorConstants.ChatAssetsPrefabsFolder}/{prefabName}.prefab";

            // 既に存在する場合はスキップ
            if (File.Exists(destinationPath))
                continue;

            // ソースプレハブを検索
            var sourceGuids = AssetDatabase.FindAssets($"t:Prefab {prefabName}");
            string? sourcePath = null;

            foreach (var guid in sourceGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == prefabName)
                {
                    sourcePath = path;
                    break;
                }
            }

            // パッケージ内をフォールバック
            sourcePath ??= $"{EditorConstants.PackagePrefabsPath}/{prefabName}.prefab";

            // プレハブをコピー
            if (File.Exists(sourcePath) || AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath) != null)
            {
                AssetDatabase.CopyAsset(sourcePath, destinationPath);

                // TMPフォントを設定
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(destinationPath);
                if (prefab != null)
                {
                    var texts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (var text in texts)
                    {
                        text.font = TMP_Settings.defaultFontAsset;
                        EditorUtility.SetDirty(text);
                    }
                    EditorUtility.SetDirty(prefab);
                }
            }
            else
            {
                Debug.LogWarning($"Prefab not found: {prefabName}.prefab");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
