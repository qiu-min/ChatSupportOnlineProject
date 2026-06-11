using ChatSupportOnline.Api.Models.Enums;

namespace ChatSupportOnline.Api.Models.Entities;

/// <summary>
/// 用户实体。
/// 当前阶段先服务于基础数据建模，后续 JWT 登录会继续复用这个模型。
/// </summary>
public class User
{
    /// <summary>
    /// 用户主键。
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 登录用户名。
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 展示名称，用于聊天窗口中显示昵称。
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 密码哈希。
    /// 当前阶段已经接入密码哈希服务，数据库中保存的是哈希结果而不是明文密码。
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 用户角色，后续将用于区分客服、访客和管理员权限。
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Customer;

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 创建时间，统一使用 UTC 方便后续跨时区处理。
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 作为客户参与的会话集合。
    /// </summary>
    public ICollection<Conversation> CustomerConversations { get; set; } = new List<Conversation>();

    /// <summary>
    /// 作为客服参与的会话集合。
    /// </summary>
    public ICollection<Conversation> AgentConversations { get; set; } = new List<Conversation>();

    /// <summary>
    /// 用户发送过的消息集合。
    /// </summary>
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
