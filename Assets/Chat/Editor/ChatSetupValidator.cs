using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable

namespace Template.Chat.Editor
{
    /// <summary>
    /// Chat設定のバリデーションを行うクラス
    /// </summary>
    internal class ChatSetupValidator
    {
        public enum ValidationSeverity { Success, Warning, Error }

        public readonly struct ValidationResult
        {
            public string Message { get; }
            public ValidationSeverity Severity { get; }

            public ValidationResult(string message, ValidationSeverity severity)
            {
                Message = message;
                Severity = severity;
            }
        }

        /// <summary>
        /// 設定のバリデーションを実行
        /// </summary>
        public ValidationResult[] Validate(
            string description,
            Sprite? characterSprite,
            Pictures? picturesAsset,
            TextAsset? scenarioText)
        {
            var results = new List<ValidationResult>();

            // Description チェック
            results.Add(string.IsNullOrWhiteSpace(description)
                ? new("✗ ゲームのあらすじが入力されていません", ValidationSeverity.Error)
                : new("✓ ゲームのあらすじが設定されています", ValidationSeverity.Success));

            // Character Sprite チェック
            results.Add(characterSprite == null
                ? new("✗ キャラクター画像が設定されていません", ValidationSeverity.Error)
                : new("✓ キャラクター画像が設定されています", ValidationSeverity.Success));

            // Pictures Asset チェック
            if (picturesAsset == null)
            {
                results.Add(new("✗ 画像リストが設定されていません", ValidationSeverity.Error));
            }
            else if (picturesAsset.pictures.Count == 0)
            {
                results.Add(new("⚠ 画像リストに画像が1枚も登録されていません", ValidationSeverity.Warning));
            }
            else
            {
                results.Add(new($"✓ 画像リストに{picturesAsset.pictures.Count}枚の画像が登録されています", ValidationSeverity.Success));
            }

            // Scenario Text チェック
            results.Add(scenarioText == null
                ? new("✗ シナリオテキストが設定されていません", ValidationSeverity.Error)
                : new("✓ シナリオテキストが設定されています", ValidationSeverity.Success));

            // TMP チェック
            var tmpType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            results.Add(tmpType == null
                ? new("✗ TextMeshProがインポートされていません（テキスト表示に必要です）", ValidationSeverity.Error)
                : new("✓ TextMeshProがインポートされています", ValidationSeverity.Success));

            return results.ToArray();
        }

        /// <summary>
        /// エラーがあるかどうかを判定
        /// </summary>
        public bool HasErrors(ValidationResult[] results) =>
            results.Any(r => r.Severity == ValidationSeverity.Error);

        /// <summary>
        /// エラーメッセージのみを取得
        /// </summary>
        public string[] GetErrorMessages(ValidationResult[] results) =>
            results
                .Where(r => r.Severity == ValidationSeverity.Error)
                .Select(r => r.Message)
                .ToArray();
    }
}
