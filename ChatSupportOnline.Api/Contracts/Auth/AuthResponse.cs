namespace ChatSupportOnline.Api.Contracts.Auth;

/// <summary>
/// 登录或注册成功后的响应模型。
/// 前端拿到这个响应后，需要把 AccessToken 放到后续请求头中。
/// </summary>
public class AuthResponse
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
    /// 展示名。
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 角色名。
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// JWT Access Token。
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Token 过期时间，使用 UTC 时间方便前后端统一判断。
    /// </summary>
    public DateTime ExpiresAtUtc { get; set; }
}
