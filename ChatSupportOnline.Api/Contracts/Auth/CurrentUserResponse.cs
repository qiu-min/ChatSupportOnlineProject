namespace ChatSupportOnline.Api.Contracts.Auth;

/// <summary>
/// 当前登录用户信息响应。
/// 它展示了服务端如何从 JWT 还原出当前身份。
/// </summary>
public class CurrentUserResponse
{
    /// <summary>
    /// 用户主键。
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 用户名。
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 展示名称。
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 用户角色。
    /// </summary>
    public string Role { get; set; } = string.Empty;
}
