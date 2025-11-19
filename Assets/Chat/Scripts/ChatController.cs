using System.Collections.Generic;
using UnityEngine;

#nullable enable

public partial class ChatController : MonoBehaviour
{
    [SerializeField] private Pictures? pictures;
    [SerializeField] private TextAsset? scenarioText;

    [Header("Chat UI Elements")]
    [SerializeField] private ChatNode chatNodePrefab = null!;
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

    ///////// Callbacks ///////// 

    public void OnClickNext()
    {
        if (presenter.CanMoveToNext)
        {
            _ = presenter.Next();
        }
    }
}
