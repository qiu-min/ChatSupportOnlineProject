namespace ChatSupportOnline.Api.Contracts.Auth;

/// <summary>
/// 登录请求模型。
/// 用户通过用户名和密码换取 JWT。
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// 登录用户名。
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 明文密码。
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
