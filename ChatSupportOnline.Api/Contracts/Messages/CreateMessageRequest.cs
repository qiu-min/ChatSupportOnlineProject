namespace ChatSupportOnline.Api.Contracts.Messages;

/// <summary>
/// 发送消息请求。
/// 当前先由 HTTP 接口完成消息写入，后续再接入 SignalR 实时推送。
/// </summary>
public class CreateMessageRequest
{
    /// <summary>
    /// 所属会话 Id。
    /// </summary>
    public Guid ConversationId { get; set; }

    /// <summary>
    /// 发送者用户 Id。
    /// </summary>
    public Guid SenderUserId { get; set; }

    /// <summary>
    /// 文本内容。
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
