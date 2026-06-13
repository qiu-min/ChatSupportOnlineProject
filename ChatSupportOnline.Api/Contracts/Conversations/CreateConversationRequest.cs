namespace ChatSupportOnline.Api.Contracts.Conversations;

/// <summary>
/// 创建会话请求。
/// 用于发起一个新的客服会话。
/// </summary>
public class CreateConversationRequest
{
    /// <summary>
    /// 会话主题。
    /// </summary>
    public string Subject { get; set; } = string.Empty;


    /// <summary>
    /// 客服 Id。
    /// 允许为空，表示先创建待接入会话。
    /// </summary>
    public Guid? AgentId { get; set; }
}
