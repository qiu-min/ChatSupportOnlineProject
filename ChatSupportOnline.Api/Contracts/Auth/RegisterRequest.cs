using ChatSupportOnline.Api.Models.Enums;

namespace ChatSupportOnline.Api.Contracts.Auth;

/// <summary>
/// 注册请求模型。
/// 这个模型只暴露注册真正需要的字段，避免直接复用实体类导致职责混乱。
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// 登录用户名。
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 展示名称。
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 明文密码。
    /// 控制器接收后会立即交给密码服务做哈希，不会直接入库。
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 用户角色。
    /// 学习阶段开放可选，便于快速创建客服或客户测试账号。
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Customer;
}
