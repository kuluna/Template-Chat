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
                ? new("✗ Description is not set.", ValidationSeverity.Error)
                : new("✓ Description is set.", ValidationSeverity.Success));

            // Character Sprite チェック
            results.Add(characterSprite == null
                ? new("✗ Character Sprite is not set.", ValidationSeverity.Error)
                : new("✓ Character Sprite is set.", ValidationSeverity.Success));

            // Pictures Asset チェック
            if (picturesAsset == null)
            {
                results.Add(new("✗ Pictures Asset is not set.", ValidationSeverity.Error));
            }
            else if (picturesAsset.pictures.Count == 0)
            {
                results.Add(new("⚠ Pictures Asset has no sprites registered.", ValidationSeverity.Warning));
            }
            else
            {
                results.Add(new($"✓ Pictures Asset has {picturesAsset.pictures.Count} sprite(s).", ValidationSeverity.Success));
            }

            // Scenario Text チェック
            results.Add(scenarioText == null
                ? new("✗ Scenario Text is not set.", ValidationSeverity.Error)
                : new("✓ Scenario Text is set.", ValidationSeverity.Success));

            // TMP チェック
            var tmpType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            results.Add(tmpType == null
                ? new("✗ TextMeshPro is not imported.", ValidationSeverity.Error)
                : new("✓ TextMeshPro is imported.", ValidationSeverity.Success));

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
