using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable enable

namespace Template.Chat
{
    public class ChatChoiceDialog : MonoBehaviour
    {
        [SerializeField] private Button buttonPrefab = null!;
        [SerializeField] private Transform panel = null!;

        private void Awake()
        {
            panel.gameObject.SetActive(false);
        }

        public void Setup(ChoiceChatCommand command, Action<string> callback)
        {
            // 既存のボタンをクリア
            ClearChoices();

            // 各選択肢に対してボタンを生成
            foreach (var choice in command.Choices)
            {
                var button = Instantiate(buttonPrefab, panel);

                // ボタンのテキストを設定
                var textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = choice;
                }

                // ボタンクリック時のコールバックを設定
                var choiceText = choice; // ラムダキャプチャ用
                button.onClick.AddListener(() => OnChoiceClicked(choiceText, callback));
            }

            // ダイアログを表示
            panel.gameObject.SetActive(true);
        }

        private void OnChoiceClicked(string choiceText, Action<string> callback)
        {
            callback(choiceText);

            ClearChoices();
            panel.gameObject.SetActive(false);
        }

        private void ClearChoices()
        {
            foreach (Transform child in panel)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
