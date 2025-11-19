using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable enable

public partial class ChatController : MonoBehaviour
{
    [SerializeField] private Pictures? pictures;
    [SerializeField] private TextAsset? scenarioText;
    [SerializeField] private Sprite? defaultIcon;

    [Header("Chat UI Elements")]
    [SerializeField] private ChatNode chatNodePrefab = null!;
    [SerializeField] private ChatNode imageNodePrefab = null!;
    [Space]
    [SerializeField] private ScrollRect chatScrollView = null!;
    [SerializeField] private Transform chatContentTransform = null!;

    private readonly ChatEventPresenter presenter = new();

    private void Awake()
    {
        if (pictures == null)
        {
            Debug.LogError("画像アセットが設定されていません。");
        }
        if (scenarioText == null)
        {
            Debug.LogError("シナリオテキストが設定されていません。");
        }
    }

    private async Awaitable Start()
    {
        foreach (Transform obj in chatContentTransform)
        {
            Destroy(obj.gameObject);
        }

        if (scenarioText != null)
        {
            presenter.Listener = this;
            presenter.Setup(scenarioText!.text);
            await presenter.Next();
        }
    }

    private void OnDestroy()
    {
        presenter.Listener = null;
    }

    public void ShowImage(Sprite sprite)
    {

    }

    ///////// Callbacks ///////// 

    public void OnClickNext()
    {
        if (presenter.CanMoveToNext)
        {
            _ = presenter.Next();
        }
    }
}
