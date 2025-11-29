using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace Template.Chat.Editor
{
    /// <summary>
    /// TextMeshPro フォントアセットの作成を行うクラス
    /// </summary>
    internal class TMPFontAssetCreator
    {
        /// <summary>
        /// フォントアセットを作成し、デフォルトフォントとして設定
        /// </summary>
        /// <param name="sourceFont">元となるフォント</param>
        /// <param name="errorMessage">エラー時のメッセージ</param>
        /// <returns>成功した場合は true</returns>
        public bool TryCreateAndSetAsDefault(Font sourceFont, out string? errorMessage)
        {
            errorMessage = null;

            try
            {
                // TMP_FontAsset 型を取得
                var tmpType = Type.GetType("TMPro.TMP_FontAsset, Unity.TextMeshPro");
                if (tmpType == null)
                {
                    errorMessage = "TextMeshPro is not imported.";
                    return false;
                }

                // フォントアセットを作成
                var fontAsset = CreateFontAsset(tmpType, sourceFont);
                if (fontAsset == null)
                {
                    errorMessage = "Failed to create TMP Font Asset.";
                    return false;
                }

                // アセットを保存
                var assetPath = SaveFontAsset(tmpType, fontAsset, sourceFont.name);

                // デフォルトフォントとして設定
                SetAsDefaultFont(fontAsset);

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        private object? CreateFontAsset(Type tmpType, Font sourceFont)
        {
            var createMethod = tmpType.GetMethod(
                "CreateFontAsset",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[] { typeof(Font) },
                null
            );

            if (createMethod == null)
            {
                throw new InvalidOperationException("CreateFontAsset method not found.");
            }

            return createMethod.Invoke(null, new object[] { sourceFont });
        }

        private string SaveFontAsset(Type tmpType, object fontAsset, string fontName)
        {
            EditorUtilities.EnsureFolderExists(EditorConstants.TMPFontsFolder);

            var assetPath = AssetDatabase.GenerateUniqueAssetPath(
                $"{EditorConstants.TMPFontsFolder}/{fontName} SDF.asset"
            );

            AssetDatabase.CreateAsset((UnityEngine.Object)fontAsset, assetPath);

            // アトラステクスチャをサブアセットとして追加
            AddAtlasTexturesAsSubAssets(tmpType, fontAsset, assetPath, fontName);

            // マテリアルをサブアセットとして追加
            AddMaterialAsSubAsset(tmpType, fontAsset, assetPath, fontName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return assetPath;
        }

        private void AddAtlasTexturesAsSubAssets(Type tmpType, object fontAsset, string assetPath, string fontName)
        {
            var atlasTexturesField = tmpType.GetField("m_AtlasTextures",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (atlasTexturesField == null) return;

            var atlasTextures = atlasTexturesField.GetValue(fontAsset);
            if (atlasTextures is not Texture2D[] textures) return;

            foreach (var texture in textures)
            {
                if (texture != null && !AssetDatabase.Contains(texture))
                {
                    texture.name = $"{fontName} Atlas";
                    AssetDatabase.AddObjectToAsset(texture, assetPath);
                }
            }
        }

        private void AddMaterialAsSubAsset(Type tmpType, object fontAsset, string assetPath, string fontName)
        {
            var materialProperty = tmpType.GetProperty("material");
            if (materialProperty == null) return;

            var material = materialProperty.GetValue(fontAsset) as Material;
            if (material != null && !AssetDatabase.Contains(material))
            {
                material.name = $"{fontName} Material";
                AssetDatabase.AddObjectToAsset(material, assetPath);
            }
        }

        private void SetAsDefaultFont(object fontAsset)
        {
            var tmpSettings = Resources.Load("TMP Settings");
            if (tmpSettings == null) return;

            var so = new SerializedObject(tmpSettings);
            var fontProperty = so.FindProperty("m_defaultFontAsset");
            if (fontProperty != null)
            {
                fontProperty.objectReferenceValue = fontAsset as UnityEngine.Object;
                so.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }
        }
    }
}
