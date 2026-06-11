namespace ChatSupportOnline.Api.Models.Entities;

/// <summary>
/// 会话实体。
/// 一个会话通常对应一位客户和一位客服之间的一段聊天上下文。
/// </summary>
public class Conversation
{
    /// <summary>
    /// 会话主键。
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 会话主题，方便后台列表中快速识别该会话。
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// 发起会话的客户 Id。
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// 当前接待该会话的客服 Id。
    /// 允许为空，表示会话已创建但暂时还没有分配客服。
    /// </summary>
    public Guid? AgentId { get; set; }

    /// <summary>
    /// 会话创建时间。
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最近更新时间。
    /// 当消息到来时，这个字段可以帮助我们快速按活跃度排序会话列表。
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 客户导航属性。
    /// </summary>
    public User? Customer { get; set; }

    /// <summary>
    /// 客服导航属性。
    /// </summary>
    public User? Agent { get; set; }

    /// <summary>
    /// 该会话下的消息集合。
    /// </summary>
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
