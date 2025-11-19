using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable enable

public class ChatNode : MonoBehaviour
{
    [SerializeField] private NodePosition nodePosition = NodePosition.Left;
    [SerializeField, TextArea] private string text = string.Empty;
    [Space]
    [SerializeField] private Color leftColor = Color.white;
    [SerializeField] private Color rightColor = Color.limeGreen;

    [Header("UI")]
    [SerializeField] private Image backgroundImage = null!;
    [SerializeField] private Image iconImage = null!;
    [SerializeField] private TextMeshProUGUI messageText = null!;

    public Sprite? iconSprite;

    private void OnEnable()
    {
        switch (nodePosition)
        {
            case NodePosition.Left:
                backgroundImage.color = leftColor;
                if (iconSprite != null)
                {
                    iconImage.sprite = iconSprite;
                    iconImage.gameObject.SetActive(true);
                }
                else
                {
                    iconImage.gameObject.SetActive(false);
                    iconImage.sprite = null;
                }

                break;

            case NodePosition.Right:
                backgroundImage.color = rightColor;
                iconImage.gameObject.SetActive(false);
                iconImage.sprite = null;
                break;
        }

        messageText.text = text;
    }

    public enum NodePosition
    {
        Left,
        Right
    }
}
