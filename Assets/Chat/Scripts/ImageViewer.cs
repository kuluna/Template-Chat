using UnityEngine;
using UnityEngine.UI;

#nullable enable

namespace Template.Chat
{
    public class ImageViewer : MonoBehaviour
    {
        [SerializeField] private GameObject backgroundPanel = null!;
        [SerializeField] private Image displayImage = null!;

        private void Awake()
        {
            backgroundPanel.SetActive(false);
        }

        public void Show(Sprite sprite)
        {
            displayImage.sprite = sprite;
            backgroundPanel.SetActive(true);
        }

        public void Hide()
        {
            backgroundPanel.SetActive(false);
            displayImage.sprite = null;
        }
    }
}
