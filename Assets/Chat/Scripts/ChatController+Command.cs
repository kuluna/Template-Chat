using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable

public partial class ChatController : ChatEventPresenter.IChatEventListener
{
    public async Awaitable ShowImage(ImageChatCommand command)
    {
        if (pictures == null) return;

        var sprite = pictures.pictures.FirstOrDefault(p => p.pictureName == command.ImageName)?.sprite;
        if (sprite == null)
        {
            Debug.LogError($"Image '{command.ImageName}' not found in Pictures asset.");
            return;
        }

        var node = Instantiate(imageNodePrefab, chatContentTransform);
        node.SetUpImage(ChatNode.NodePosition.Left, sprite, (sprite) =>
        {
            ShowImage(sprite);
        }, defaultIcon);

        // スクロールを最下部に移動
        await Awaitable.EndOfFrameAsync();
        chatScrollView.verticalNormalizedPosition = 0f;
    }

    public async Awaitable ShowText(TextChatCommand command)
    {
        var node = Instantiate(chatNodePrefab, chatContentTransform);
        node.SetUpText(ChatNode.NodePosition.Left, command.Text, defaultIcon);

        // スクロールを最下部に移動
        await Awaitable.EndOfFrameAsync();
        chatScrollView.verticalNormalizedPosition = 0f;
    }

    public async Awaitable OnEndChat()
    {
        Debug.Log("Chat終了");
        Instantiate(endNodePrefab, chatContentTransform);
        await Task.CompletedTask;
    }

    public void ShowChoice(ChoiceChatCommand command)
    {
        currentChoiceCommand = command;
        choiceDialog.Setup(command, OnClickSelectChoice);
    }
}
