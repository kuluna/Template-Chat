using TMPro;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class DynamicFontCleaner
{
    static DynamicFontCleaner()
    {
#pragma warning disable UDR0001 // Domain Reload Analyzer
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorApplication.quitting += OnApplicationQuitting;
#pragma warning restore UDR0001 // Domain Reload Analyzer
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            var tmpFontAssets = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            foreach (var tmpFontAsset in tmpFontAssets)
            {
                if (tmpFontAsset != null && tmpFontAsset.atlasPopulationMode == AtlasPopulationMode.Dynamic)
                {
                    tmpFontAsset.ClearFontAssetData();
                    Debug.Log("DynamicFontCleaner: ClearFontAssetData " + tmpFontAsset.name);
                }
            }
        }
    }

    private static void OnApplicationQuitting()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.quitting -= OnApplicationQuitting;
    }
}
