using UnityEngine;

#nullable enable

public class ChatNode : MonoBehaviour
{
    [SerializeField] private NodePosition nodePosition = NodePosition.Left;
    [SerializeField, TextArea] private string text = string.Empty;
    [Space]
    [SerializeField] private Color leftColor = Color.white;
    [SerializeField] private Color rightColor = Color.limeGreen;

    private void Awake()
    {

    }



    public enum NodePosition
    {
        Left,
        Right
    }
}
