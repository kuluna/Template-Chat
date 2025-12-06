using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable enable

namespace Template.Chat
{
    public class ChatNode : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private Color leftColor = Color.white;
        [SerializeField] private Color rightColor = Color.limeGreen;

        [Header("UI")]
        [SerializeField] private Image backgroundImage = null!;
        [SerializeField] private Image iconImage = null!;
        [Space]
        [SerializeField] private TextMeshProUGUI? messageText = null!;
        [SerializeField] private Image? pictureImage = null;

        private NodePosition nodePosition = NodePosition.Left;
        private Sprite? iconSprite;
        private string text = string.Empty;
        private Sprite? picture;

        private readonly UnityEvent<Sprite> onclickImage = new();

        private void Start()
        {
            switch (nodePosition)
            {
                case NodePosition.Left:
                    backgroundImage.color = leftColor;
                    if (iconSprite != null)
                    {
                        iconImage.sprite = iconSprite;
                        iconImage.transform.parent.gameObject.SetActive(true);
                    }
                    else
                    {
                        iconImage.transform.parent.gameObject.SetActive(false);
                        iconImage.sprite = null;
                    }

                    break;

                case NodePosition.Right:
                    backgroundImage.color = rightColor;
                    iconImage.transform.parent.gameObject.SetActive(false);
                    iconImage.sprite = null;

                    if (TryGetComponent<VerticalLayoutGroup>(out var layoutGroup))
                    {
                        layoutGroup.childAlignment = TextAnchor.UpperRight;
                    }
                    break;
            }

            if (messageText != null)
            {
                messageText.text = text;
            }
            if (pictureImage != null && picture != null)
            {
                pictureImage.sprite = picture;
            }
        }

        private void OnDestroy()
        {
            onclickImage.RemoveAllListeners();
        }

        public void SetUpText(NodePosition position, string message, Sprite? iconSprite = null)
        {
            nodePosition = position;
            text = message;
            picture = null;
            this.iconSprite = iconSprite;
        }

        public void SetUpImage(NodePosition position, Sprite picture, UnityAction<Sprite> onClick, Sprite? iconSprite = null)
        {
            nodePosition = position;
            this.picture = picture;
            text = string.Empty;
            this.iconSprite = iconSprite;

            onclickImage.RemoveAllListeners();
            onclickImage.AddListener(onClick);

            // 画像のアス比に合わせてLayoutElementの高さを調整 (幅は変更しない)
            if (pictureImage != null)
            {
                var bubble = pictureImage.GetComponentInParent<LayoutElement>();
                float aspectRatio = picture.rect.width / picture.rect.height;
                float targetHeight = bubble.minWidth / aspectRatio;
                bubble.minHeight = targetHeight;
            }
        }

        public void OnClickImage()
        {
            if (picture != null)
            {
                onclickImage.Invoke(picture);
            }
        }

        public enum NodePosition
        {
            Left,
            Right
        }
    }
}
