namespace ChatSupportOnline.Api.Models.Enums;

/// <summary>
/// 用户角色枚举。
/// 先把角色建模出来，后面接 JWT 时就可以直接把角色写进 Claim 做授权。
/// </summary>
public enum UserRole
{
    Customer = 1,
    Agent = 2,
    Admin = 3
}
