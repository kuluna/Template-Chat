using System.Threading.Tasks;
using UnityEngine;

#nullable enable

public partial class ChatController : ChatEventPresenter.IChatEventListener
{
    public async Awaitable ShowText(TextChatCommand command)
    {
        //TODO: Implement text display logic here.
        Debug.Log($"Displaying text: {string.Join(" ", command.Args)}");
        await Task.CompletedTask;
    }

    public async Awaitable OnEndChat()
    {
        Debug.Log("Chat has ended.");
        await Task.CompletedTask;
    }
}
