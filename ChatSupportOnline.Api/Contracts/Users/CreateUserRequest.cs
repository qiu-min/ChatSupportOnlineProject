using ChatSupportOnline.Api.Models.Enums;

namespace ChatSupportOnline.Api.Contracts.Users;

/// <summary>
/// 创建用户请求。
/// 当前用于快速建立测试数据，后续可以演进成注册接口的输入模型。
/// </summary>
public class CreateUserRequest
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
    /// 当前阶段会先直接映射到 PasswordHash 字段，下一阶段接 JWT 时会改成真正哈希。
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 用户角色。
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Customer;
}
