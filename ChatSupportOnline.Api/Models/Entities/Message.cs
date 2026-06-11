namespace ChatSupportOnline.Api.Models.Entities;

/// <summary>
/// 消息实体。
/// 它记录了是谁、在什么时间、向哪个会话发送了什么内容。
/// </summary>
public class Message
{
    /// <summary>
    /// 消息主键。
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 所属会话 Id。
    /// </summary>
    public Guid ConversationId { get; set; }

    /// <summary>
    /// 发送者用户 Id。
    /// </summary>
    public Guid SenderUserId { get; set; }

    /// <summary>
    /// 文本消息内容。
    /// 当前阶段先只支持文本，后续可以扩展图片、文件、系统消息等类型。
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 发送时间。
    /// </summary>
    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 所属会话导航属性。
    /// </summary>
    public Conversation? Conversation { get; set; }

    /// <summary>
    /// 发送者导航属性。
    /// </summary>
    public User? Sender { get; set; }
}
