using TMPro;
using UnityEngine;

#nullable enable

namespace Template.Chat
{
    public class DescriptionPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel = null!;
        [SerializeField] private TextMeshProUGUI descriptionText = null!;

        public void Show(string text)
        {
            descriptionText.text = text;
            panel.SetActive(true);

            Time.timeScale = 0f;
        }

        public void Hide()
        {
            panel.SetActive(false);
            Time.timeScale = 1f;
        }

        public void SetText(string text)
        {
            descriptionText.text = text;
        }
    }
}
